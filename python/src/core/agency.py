import asyncio
import logging
from typing import Callable, Dict, List, Optional
from datetime import datetime
from dataclasses import dataclass, field


from models.messages.broker_message import BrokerMessage, BrokerMessageType

logger = logging.getLogger(__name__)


@dataclass
class Agency:
    id: str
    name: str
    authority: "Authority"
    broker: "Broker"
    logger: logging.Logger = field(default_factory=logging.getLogger)
    chat_message_received: Optional[Callable[[dict], None]] = None
    representative_id: Optional[str] = None
    is_connected: bool = False

    # Internal data stores
    agent_join_timestamps: Dict[str, datetime] = field(default_factory=dict)
    agents: Dict[str, "Agent"] = field(default_factory=dict)
    local_agent_ids: List[str] = field(default_factory=list)
    chat_history: List[dict] = field(default_factory=list)

    async def connect(self):
        """Connects the agency to the MQTT broker."""
        if not self.is_connected:
            topic = self.authority.agency_topic("+", self.id)
            await self.broker.subscribe(topic, self._broker_receive_message)
            self.is_connected = True
            logger.info(f"Agency {self.name} connected to broker.")

    async def disconnect(self):
        """Disconnects the agency from the MQTT broker."""
        if self.is_connected:
            topic = self.authority.agency_topic("+", self.id)
            await self.broker.unsubscribe(topic)
            self.is_connected = False
            logger.info(f"Agency {self.name} disconnected from broker.")

    async def _broker_receive_message(self, message: BrokerMessage):
        """Handles messages received from the broker."""
        if message.sender_id is None:
            return

        # Handle "join" messages
        if (
            message.type == BrokerMessageType.EVENT
            and message.data.get("type") == "join"
            and message.data.get("agent_id") is not None
            and message.data.get("timestamp") is not None
        ):
            timestamp = self._parse_timestamp(message.data["timestamp"])
            agent_id = message.data["agent_id"]
            if agent_id == message.sender_id and timestamp:
                self._receive_join(agent_id, timestamp)

        # Handle "representative_claim" messages
        elif (
            message.type == BrokerMessageType.EVENT
            and message.data.get("type") == "representative_claim"
            and message.data.get("agent_id") is not None
            and message.data.get("timestamp") is not None
        ):
            timestamp = self._parse_timestamp(message.data["timestamp"])
            agent_id = message.data["agent_id"]
            if agent_id == message.sender_id and timestamp:
                self._receive_representative_claim(agent_id, timestamp)

    def _parse_timestamp(self, timestamp: str) -> Optional[datetime]:
        """Parses a timestamp string into a datetime object."""
        try:
            return datetime.fromisoformat(timestamp)
        except ValueError:
            logger.error(f"Invalid timestamp format: {timestamp}")
            return None

    def _receive_join(self, agent_id: str, timestamp: datetime):
        """Handles an agent joining the agency."""
        logger.info(f"Agent {agent_id} joined agency {self.name}.")
        self.agent_join_timestamps[agent_id] = timestamp
        if self.representative_id and self.representative_id in self.local_agent_ids:
            self._send_agent_welcome(agent_id)

    def _send_agent_welcome(self, agent_id: str):
        """Sends a welcome message to a newly joined agent."""
        agent = self.agents.get(agent_id)
        if agent:
            logger.info(f"Sending welcome message to agent {agent.name}.")
            self.broker.publish(
                BrokerMessage(
                    type=BrokerMessageType.EVENT,
                    topic=self.authority.agent_topic(self.id, agent.id),
                    data={
                        "type": "welcome",
                        "timestamp": self.broker.timestamp,
                        "agency": {"id": self.id, "name": self.name},
                        "representative_id": self.representative_id,
                    },
                )
            )

    def _receive_representative_claim(self, agent_id: str, timestamp: datetime):
        """Handles a representative claim event."""
        logger.info(f"Agent {agent_id} claimed representative role for agency {self.name}.")
        if self.representative_id != agent_id:
            self.representative_id = agent_id
            logger.info(f"Set representative to agent {agent_id}.")

    def add_local_agent(self, agent: "Agent"):
        """Adds a local agent to the agency."""
        self.agents[agent.id] = agent
        self.local_agent_ids.append(agent.id)

    def get_local_agent(self, agent_id: str) -> Optional["Agent"]:
        """Retrieves a local agent by ID."""
        return self.agents.get(agent_id) if agent_id in self.local_agent_ids else None

    async def inform_async(self, message: str):
        """Sends an informational message to the chat."""
        chat_message = {"role": "user", "message": message}
        self.chat_history.append(chat_message)
        if self.chat_message_received:
            self.chat_message_received(
                {"agency_id": self.id, "message": chat_message}
            )

    async def get_history(self) -> List[dict]:
        """Retrieves the agency's chat history."""
        return self.chat_history
