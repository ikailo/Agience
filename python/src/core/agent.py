import asyncio
import logging
from typing import Callable, Dict, Optional
from dataclasses import dataclass, field
from paho.mqtt.client import Client
from threading import Timer
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime

from models.messages.broker_message import BrokerMessage, BrokerMessageType


logger = logging.getLogger(__name__)


@dataclass
class Agent:
    id: str
    name: str
    authority: "Authority"
    broker: "Broker"
    agency: "Agency"
    persona: str
    kernel: "Kernel"
    logger: logging.Logger = field(default_factory=logging.getLogger)
    chat_message_received: Optional[Callable[[dict], None]] = None
    chat_history: list = field(default_factory=list)
    is_connected: bool = False
    disposed: bool = False

    def __post_init__(self):
        self.representative_claim_timer = Timer(
            interval=5, function=self._send_representative_claim
        )
        self.executor = ThreadPoolExecutor(max_workers=5)

    async def connect(self):
        if not self.is_connected:
            # Subscribe to the MQTT broker topic for this agent
            await self.broker.subscribe(
                self.authority.agent_topic("+", self.id), self._receive_message
            )

            # Send a "join" message to notify others
            self._send_join()
            self.representative_claim_timer.start()
            self.is_connected = True

    async def disconnect(self):
        if self.is_connected:
            # Send "leave" and "representative_resign" messages before disconnecting
            self._send_representative_resign()
            self._send_leave()

            # Unsubscribe from the MQTT broker
            await self.broker.unsubscribe(self.authority.agent_topic("+", self.id))
            self.is_connected = False

    async def _receive_message(self, message: BrokerMessage):
        # Handle received MQTT messages for this agent
        logger.debug("Received message on topic %s: %s", message.topic, message.data)
        # TODO: Implement message-specific logic
        pass

    def _send_join(self):
        logger.debug("Sending 'join' message")
        self.broker.publish(
            BrokerMessage(
                type=BrokerMessageType.EVENT,
                topic=self.authority.agency_topic(self.id, self.agency.id),
                data={"type": "join", "timestamp": self.broker.timestamp, "agent_id": self.id},
            )
        )

    def _send_leave(self):
        logger.debug("Sending 'leave' message")
        self.broker.publish(
            BrokerMessage(
                type=BrokerMessageType.EVENT,
                topic=self.authority.agency_topic(self.id, self.agency.id),
                data={"type": "leave", "timestamp": self.broker.timestamp, "agent_id": self.id},
            )
        )

    def _send_representative_claim(self):
        if self.agency.representative_id is not None:
            return  # Another agent already claimed the role

        logger.debug("Sending 'representative_claim' message")
        self.broker.publish(
            BrokerMessage(
                type=BrokerMessageType.EVENT,
                topic=self.authority.agency_topic(self.id, self.agency.id),
                data={"type": "representative_claim", "timestamp": self.broker.timestamp, "agent_id": self.id},
            )
        )

    def _send_representative_resign(self):
        if self.agency.representative_id != self.id:
            return  # Only the current representative can resign

        logger.debug("Sending 'representative_resign' message")
        self.broker.publish(
            BrokerMessage(
                type=BrokerMessageType.EVENT,
                topic=self.authority.agency_topic(self.id, self.agency.id),
                data={"type": "representative_resign", "timestamp": self.broker.timestamp, "agent_id": self.id},
            )
        )

    async def prompt_async(self, user_message: str):
        self.chat_history.append({"role": "user", "message": user_message})
        # Call the kernel (simulating an AI LLM call) for a response
        assistant_message = await self.kernel.get_chat_response(
            self.chat_history, self.persona
        )
        if assistant_message:
            self.chat_history.append({"role": "assistant", "message": assistant_message})

            # Notify listeners of the new message
            if self.chat_message_received:
                self.chat_message_received(
                    {"agent_id": self.id, "message": assistant_message}
                )

    def dispose(self):
        if not self.disposed:
            self.representative_claim_timer.cancel()
            self.executor.shutdown(wait=False)
            self.disposed = True
