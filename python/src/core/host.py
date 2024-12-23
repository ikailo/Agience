import json
import base64
import logging
from typing import Dict, List, Optional, Callable, Any, Union
from dataclasses import dataclass
import aiohttp
import asyncio

# from models.entities.host import Host as HostModel
# from models.entities.agent import Agent as AgentModel
# from models.entities.plugin import Plugin as PluginModel
from models.messages.broker_message import BrokerMessageType


@dataclass
class TokenResponse:
    access_token: Optional[str] = None
    token_type: Optional[str] = None
    expires_in: Optional[int] = None


class Host:
    def __init__(
        self,
        host_id: str,
        host_secret: str,
        authority,
        broker,
        agent_factory,
        logger: Optional[logging.Logger] = None
    ):
        if not host_id or not host_secret:
            raise ValueError("host_id and host_secret must not be empty")

        self._id = host_id
        self._host_secret = host_secret
        self._authority = authority
        self._broker = broker
        self._agent_factory = agent_factory
        self._logger = logger or logging.getLogger(__name__)

        self._agents: Dict[str, Any] = {}
        self._agencies: Dict[str, Any] = {}
        self.is_connected = False

        # Event-like callbacks
        self.agent_connected: Optional[Callable] = None
        self.agency_connected: Optional[Callable] = None
        self.agent_disconnected: Optional[Callable] = None
        self.agency_disconnected: Optional[Callable] = None
        self.agent_log_entry_received: Optional[Callable] = None
        self.agency_log_entry_received: Optional[Callable] = None

    @property
    def id(self) -> str:
        return self._id

    @property
    def is_connected(self) -> bool:
        return self._is_connected

    @is_connected.setter
    def is_connected(self, value: bool):
        self._is_connected = value

    @property
    def agents(self) -> Dict[str, Any]:
        return dict(self._agents)

    @property
    def agencies(self) -> Dict[str, Any]:
        return dict(self._agencies)

    async def run(self):
        """
        Run the host, maintaining connection until stopped
        """
        await self.start()
        while self.is_connected:
            await asyncio.sleep(0.1)

    async def stop(self):
        """
        Stop the host
        """
        self._logger.info("Stopping Host")
        await self.disconnect()

    async def start(self):
        """
        Start the host with connection retry logic
        """
        self._logger.info("Starting Host")
        while not self.is_connected:
            try:
                await self.connect()
            except Exception as ex:
                self._logger.error(f"Unable to Connect: {str(ex)}")
                self._logger.info("Retrying in 10 seconds")
                await asyncio.sleep(10)  # TODO: Implement exponential backoff

    async def connect(self):
        """
        Establish connection to the broker and subscribe to topics
        """
        self._logger.info("Connecting Host")

        await self._authority.initialize_with_backoff()

        broker_uri = self._authority.broker_uri
        if not broker_uri:
            raise ValueError("BrokerUri is null")

        access_token = await self.get_access_token()
        if not access_token:
            raise ValueError("access_token is null")

        await self._broker.connect(access_token, broker_uri)

        if self._broker.is_connected:
            # Subscribe to broadcast and specific host topics
            await self._broker.subscribe(
                self._authority.host_topic("+", "0"),
                self._broker_receive_message
            )
            await self._broker.subscribe(
                self._authority.host_topic("+", self.id),
                self._broker_receive_message
            )

            # Publish host connect event
            await self._broker.publish_async({
                "type": BrokerMessageType.EVENT,
                "topic": self._authority.authority_topic(self.id),
                "data": {
                    "type": "host_connect",
                    "timestamp": self._broker.timestamp,
                    "host": json.dumps(self.to_dict())
                }
            })

            self.is_connected = True
        else:
            raise Exception("Broker Connection Failed")

        self._logger.info("Host Connected")

    async def disconnect(self):
        """
        Disconnect all agents and broker
        """
        if self.is_connected:
            # Disconnect all agents
            for agent in list(self._agents.values()):
                await agent.disconnect()

            # Unsubscribe from topics and disconnect broker
            await self._broker.unsubscribe(self._authority.host_topic("+", "0"))
            await self._broker.unsubscribe(self._authority.host_topic("+", self.id))
            await self._broker.disconnect()
            self.is_connected = False

    async def get_access_token(self) -> Optional[str]:
        """
        Obtain access token from token endpoint
        """
        token_endpoint = self._authority.token_endpoint
        if not token_endpoint:
            raise ValueError("tokenEndpoint is null")

        # Create basic auth header
        auth_str = base64.b64encode(
            f"{self.id}:{self._host_secret}".encode()
        ).decode()

        async with aiohttp.ClientSession() as session:
            headers = {"Authorization": f"Basic {auth_str}"}
            data = {
                "grant_type": "client_credentials",
                "scope": "connect"
            }

            async with session.post(token_endpoint, headers=headers, data=data) as resp:
                if resp.status == 200:
                    response_data = await resp.json()
                    return TokenResponse(**response_data).access_token
        return None

    def add_plugin_from_type(self, name: str, plugin_type: type):
        """
        Add a plugin to the host from a given type
        """
        self._agent_factory.add_host_plugin_from_type(name, plugin_type)

    async def _receive_host_welcome(
        self,
        host_data: Dict[str, Any],
        plugins: List[Dict[str, Any]],
        agents: List[Dict[str, Any]]
    ):
        """
        Process host welcome message, add plugins and connect agents
        """
        # Add plugins
        for plugin_data in plugins:
            self._agent_factory.add_host_plugin(plugin_data)

        # Connect agents
        for agent_data in agents:
            await self._receive_agent_connect(agent_data)

    async def _receive_agent_connect(self, agent_data: Dict[str, Any]):
        """
        Process agent connection
        """
        # Create agent using agent factory
        agent = self._agent_factory.create_agent(agent_data)

        # Connect agency first if not already connected
        if agent.agency.id not in self._agencies:
            self._agencies[agent.agency.id] = agent.agency
            await agent.agency.connect()

            self._logger.info(f"{agent.agency.name} Connected")

            # Invoke agency connected callback if set
            if self.agency_connected:
                await self.agency_connected(agent.agency)

        # Connect agent
        self._agents[agent.id] = agent
        await agent.connect()

        self._logger.info(f"{agent.name} Connected")

        # Invoke agent connected callback if set
        if self.agent_connected:
            await self.agent_connected(agent)

    async def _receive_agent_disconnect(self, agent_id: str):
        """
        Process agent disconnection
        """
        agent = self._agents.get(agent_id)
        if not agent:
            return

        await agent.disconnect()

        self._logger.info(f"{agent.name} Disconnected")

        # Invoke agent disconnected callback if set
        if self.agent_disconnected:
            await self.agent_disconnected(agent_id)

        # Check if agency needs to be disconnected
        agency_agents = [
            a for a in self._agents.values()
            if a.agency.id == agent.agency.id
        ]

        if not agency_agents and agent.agency.id in self._agencies:
            await agent.agency.disconnect()
            del self._agencies[agent.agency.id]

            self._logger.info(f"{agent.agency.name} Disconnected")

            # Invoke agency disconnected callback if set
            if self.agency_disconnected:
                await self.agency_disconnected(agent.agency.id)

        # Dispose of the agent
        self._agent_factory.dispose_agent(agent_id)
        del self._agents[agent_id]

    async def _broker_receive_message(self, message: Dict[str, Any]):
        """
        Process incoming broker messages
        """
        if not message.get("sender_id") or not message.get("data"):
            return

        data = message.get("data", {})
        msg_type = message.get("type")

        # Host Welcome Message
        if (msg_type == BrokerMessageType.EVENT and
            data.get("type") == "host_welcome" and
                data.get("host")):

            host_data = json.loads(data["host"])
            plugins = json.loads(data.get("plugins", "[]"))
            agents = json.loads(data.get("agents", "[]"))

            if not host_data.get("id"):
                self._logger.error("Invalid Host")
                return

            self._logger.info(f"Received Host Welcome Message from {
                              host_data.get('name', 'Unknown')}")
            await self._receive_host_welcome(host_data, plugins, agents)

        # Agent Connect Message
        elif (msg_type == BrokerMessageType.EVENT and
              data.get("type") == "agent_connect" and
              data.get("agent")):

            agent_data = json.loads(data["agent"])
            if not agent_data.get("id"):
                self._logger.error("Invalid Agent")
                return

            if not agent_data.get("agency", {}).get("id"):
                self._logger.error("Agent has an invalid Agency")
                return

            await self._receive_agent_connect(agent_data)

        # Agent Disconnect Message
        elif (msg_type == BrokerMessageType.EVENT and
              data.get("type") == "agent_disconnect" and
              data.get("agent_id")):

            await self._receive_agent_disconnect(data["agent_id"])

    def to_dict(self) -> Dict[str, Any]:
        """
        Convert host to a dictionary representation
        """
        return {
            "id": self.id,
            "is_connected": self.is_connected,
            # Add other necessary fields for serialization
        }
