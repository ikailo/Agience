import json
import base64
import logging
from typing import Dict, List, Optional, Callable, Any
from dataclasses import dataclass
import aiohttp
import asyncio
from enum import Enum


class BrokerMessageType(Enum):
    EVENT = "EVENT"


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
        logger=None
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

        # Events (using callbacks since Python doesn't have native events)
        self.agent_connected_callback: Optional[Callable] = None
        self.agency_connected_callback: Optional[Callable] = None
        self.agent_disconnected_callback: Optional[Callable] = None
        self.agency_disconnected_callback: Optional[Callable] = None
        self.agent_log_entry_received: Optional[Callable] = None
        self.agency_log_entry_received: Optional[Callable] = None

    @property
    def id(self) -> str:
        return self._id

    @property
    def agents(self) -> Dict[str, Any]:
        return self._agents.copy()

    @property
    def agencies(self) -> Dict[str, Any]:
        return self._agencies.copy()

    async def run(self):
        await self.start()
        while self.is_connected:
            await asyncio.sleep(0.1)

    async def stop(self):
        self._logger.info("Stopping Host")
        await self.disconnect()

    async def start(self):
        self._logger.info("Starting Host")
        while not self.is_connected:
            try:
                await self.connect()
            except Exception as ex:
                self._logger.error(f"Unable to Connect: {str(ex)}")
                self._logger.info("Retrying in 10 seconds")
                await asyncio.sleep(10)  # TODO: Implement backoff

    async def connect(self):
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
            await self._broker.subscribe(
                self._authority.host_topic("+", "0"),
                self._broker_receive_message
            )
            await self._broker.subscribe(
                self._authority.host_topic("+", self.id),
                self._broker_receive_message
            )

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
        if self.is_connected:
            for agent in self._agents.values():
                await agent.disconnect()

            await self._broker.unsubscribe(self._authority.host_topic("+", "0"))
            await self._broker.unsubscribe(self._authority.host_topic("+", self.id))
            await self._broker.disconnect()
            self.is_connected = False

    async def get_access_token(self) -> Optional[str]:
        token_endpoint = self._authority.token_endpoint
        if not token_endpoint:
            raise ValueError("tokenEndpoint is null")

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
        self._agent_factory.add_host_plugin_from_type(name, plugin_type)

    async def _broker_receive_message(self, message: dict):
        if not message.get("sender_id") or not message.get("data"):
            return

        data = message["data"]
        msg_type = message.get("type")

        if (msg_type == BrokerMessageType.EVENT and
            data.get("type") == "host_welcome" and
                data.get("host")):

            host_data = json.loads(data["host"])
            plugins = json.loads(data.get("plugins", "[]"))
            agents = json.loads(data.get("agents", "[]"))

            if not host_data.get("id"):
                self._logger.error("Invalid Host")
            else:
                self._logger.info(f"Received Host Welcome Message from {
                                  host_data.get('name')}")
                await self._receive_host_welcome(host_data, plugins, agents)

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

        elif (msg_type == BrokerMessageType.EVENT and
              data.get("type") == "agent_disconnect" and
              data.get("agent_id")):

            await self._receive_agent_disconnect(data["agent_id"])

    def to_dict(self):
        return {
            "id": self.id,
            "is_connected": self.is_connected,
            # Add other necessary fields for serialization
        }
