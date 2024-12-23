import asyncio
import json
from typing import Optional, List, Dict
from datetime import datetime
import logging

# Custom imports (to match the .NET structure)
# from models.entities import Broker, BrokerMessage, BrokerMessageType
from models.messages.broker_message import BrokerMessage, BrokerMessageType
# from models.entities.authority_data_adapter_interface import AuthorityDataAdapter

logger = logging.getLogger(__name__)


class Authority:
    BROKER_URI_KEY = "broker_uri"
    OPENID_CONFIG_PATH = "/.well-known/openid-configuration"

    def __init__(
        self,
        authority_uri: str,
        # TODO: Fix type hinting for broker, authority_data_adapter
        # broker: Broker,
        broker,
        # authority_data_adapter: AuthorityDataAdapter,
        authority_data_adapter,
        authority_uri_internal: Optional[str] = None,
        broker_uri_internal: Optional[str] = None,
    ):
        if not authority_uri:
            raise ValueError("authority_uri cannot be null or empty")

        self._authority_uri = authority_uri
        self._authority_uri_internal = authority_uri_internal
        self._broker = broker
        self._data_adapter = authority_data_adapter
        self._broker_uri = broker_uri_internal
        self._token_endpoint = None
        self._is_connected = False

    @property
    def id(self) -> str:
        """Returns the Authority ID derived from its host."""
        return self._authority_uri.split("//")[1].split("/")[0]

    @property
    def broker_uri(self) -> Optional[str]:
        """Returns the Broker URI."""
        return self._broker_uri

    @property
    def token_endpoint(self) -> Optional[str]:
        """Returns the Token Endpoint."""
        return self._token_endpoint

    @property
    def is_connected(self) -> bool:
        """Indicates whether the Authority is connected."""
        return self._is_connected

    async def initialize_with_backoff(self, max_delay_seconds: int = 16):
        """Initializes the Authority with exponential backoff."""
        delay = 1

        while True:
            try:
                # Perform initialization using the OpenID configuration
                authority_uri = self._authority_uri_internal or self._authority_uri
                logger.info(f"Initializing Authority: {authority_uri}")

                # Simulate retrieving OpenID configuration
                config = await self._retrieve_openid_configuration(authority_uri)

                # Set Broker URI and Token Endpoint
                self._broker_uri = config.get(
                    self.BROKER_URI_KEY, self._broker_uri)
                self._token_endpoint = config.get("token_endpoint")

                logger.info("Authority initialized successfully.")
                break

            except Exception as e:
                logger.error(f"Failed to initialize Authority: {e}")
                logger.info(f"Retrying in {delay} seconds...")
                await asyncio.sleep(delay)
                delay = min(delay * 2, max_delay_seconds)

    async def _retrieve_openid_configuration(self, authority_uri: str) -> Dict:
        """Simulates retrieving OpenID configuration (replace with actual implementation)."""
        # Replace with actual HTTP request logic
        return {
            self.BROKER_URI_KEY: "mqtt://broker.example.com",
            "token_endpoint": f"{authority_uri}/oauth2/token",
        }

    async def connect(self, access_token: str):
        """Connects the Authority to the Broker."""
        if not self._is_connected:
            if not self._broker_uri:
                await self.initialize_with_backoff()

            await self._broker.connect(access_token, self._broker_uri)

            if self._broker.is_connected:
                topic = self.authority_topic("+")
                await self._broker.subscribe(topic, self._broker_receive_message)
                self._is_connected = True
                logger.info("Authority connected to the Broker.")

    async def disconnect(self):
        """Disconnects the Authority from the Broker."""
        if self._is_connected:
            topic = self.authority_topic("+")
            await self._broker.unsubscribe(topic)
            await self._broker.disconnect()
            self._is_connected = False
            logger.info("Authority disconnected from the Broker.")

    async def _broker_receive_message(self, message: BrokerMessage):
        """Handles incoming messages from the Broker."""
        if not message.sender_id or not message.data:
            return

        if (
            message.type == BrokerMessageType.EVENT
            and message.data.get("type") == "host_connect"
            and message.data.get("host") is not None
        ):
            host = json.loads(message.data["host"])
            if host.get("id") == message.sender_id:
                await self._on_host_connected(host["id"])

    async def _on_host_connected(self, host_id: str):
        """Handles a host connection event."""
        logger.info(f"Host connected: {host_id}")

        # Retrieve data for the host
        host = await self._data_adapter.get_host_by_id_no_tracking(host_id)
        plugins = await self._data_adapter.get_plugins_for_host_id_no_tracking(host_id)
        agents = await self._data_adapter.get_agents_for_host_id_no_tracking(host_id)

        logger.info(f"Host: {host['name']}, Plugins: {
                    len(plugins)}, Agents: {len(agents)}")
        self._send_host_welcome_event(host, plugins, agents)

    def _send_host_welcome_event(self, host: dict, plugins: List[dict], agents: List[dict]):
        """Sends a Host Welcome Event to the Broker."""
        if not self._is_connected:
            raise RuntimeError("Authority is not connected.")

        logger.info(f"Publishing Host Welcome Event: {host['name']}")
        self._broker.publish(
            BrokerMessage(
                type=BrokerMessageType.EVENT,
                topic=self.host_topic(self.id, host["id"]),
                data={
                    "type": "host_welcome",
                    "timestamp": self._broker.timestamp,
                    "host": json.dumps(host),
                    "plugins": json.dumps(plugins),
                    "agents": json.dumps(agents),
                },
            )
        )

    async def send_agent_connect_event(self, agent: dict):
        """Sends an Agent Connect Event."""
        if not self._is_connected:
            raise RuntimeError("Authority is not connected.")

        host_id = await self._data_adapter.get_host_id_for_agent_id_no_tracking(agent["id"])
        logger.info(f"Agent connected: {agent['name']}")

        self._broker.publish(
            BrokerMessage(
                type=BrokerMessageType.EVENT,
                topic=self.host_topic(self.id, host_id),
                data={
                    "type": "agent_connect",
                    "timestamp": self._broker.timestamp,
                    "agent": json.dumps(agent),
                },
            )
        )

    async def send_agent_disconnect_event(self, agent: dict):
        """Sends an Agent Disconnect Event."""
        if not self._is_connected:
            raise RuntimeError("Authority is not connected.")

        host_id = await self._data_adapter.get_host_id_for_agent_id_no_tracking(agent["id"])
        logger.info(f"Agent disconnected: {agent['name']}")

        self._broker.publish(
            BrokerMessage(
                type=BrokerMessageType.EVENT,
                topic=self.host_topic(self.id, host_id),
                data={
                    "type": "agent_disconnect",
                    "timestamp": self._broker.timestamp,
                    "agent_id": agent["id"],
                },
            )
        )

    def topic(self, sender_id: str, host_id: Optional[str] = None, agency_id: Optional[str] = None, agent_id: Optional[str] = None) -> str:
        """Generates a topic string."""
        return f"{sender_id}/{self.id}/{host_id or '-'}/{agency_id or '-'}/{agent_id or '-'}"

    def authority_topic(self, sender_id: str) -> str:
        """Generates an Authority topic string."""
        return self.topic(sender_id)

    def host_topic(self, sender_id: str, host_id: Optional[str]) -> str:
        """Generates a Host topic string."""
        return self.topic(sender_id, host_id)

    def agency_topic(self, sender_id: str, agency_id: str) -> str:
        """Generates an Agency topic string."""
        return self.topic(sender_id, agency_id=agency_id)

    def agent_topic(self, sender_id: str, agent_id: str) -> str:
        """Generates an Agent topic string."""
        return self.topic(sender_id, agent_id=agent_id)
