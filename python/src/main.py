import os
from dotenv import load_dotenv
import asyncio
from core.broker import Broker
from core.authority import Authority
from core.host import Host
from core.agent_factory import AgentFactory
from core.models.messages.broker_message import BrokerMessage, BrokerMessageType
import logging
import time

load_dotenv()

HOST_ID = os.getenv('HOST_ID')
HOST_SECRET = os.getenv('HOST_SECRET')
BROKER_URI = os.getenv('BROKER_URI')
AUTHORITY_URI = os.getenv('AUTHORITY_URI')


async def message_handler(message: BrokerMessage):
    print("===========================================")
    print(f"Received message on topic: {message.topic}")
    print(f"Message type: {message.type}")
    print(f"Message data: {message.data}")
    print("===========================================")


logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("authority")


async def main():
    # Initialize the Broker
    broker = Broker(logger=logging.getLogger("broker"))
    # broker_2 = Broker(None)

    # Initialize the Authority
    # TODO: service_scope_factory should not be null, implement
    authority = Authority(authority_uri=AUTHORITY_URI, broker=broker, service_scope_factory=None,
                          logger=logging.getLogger("authority"), authority_uri_internal=None, broker_uri_internal=None)

    agent_factory = AgentFactory(
        service_provider=None, broker=broker, authority=authority, logger=logging.getLogger("agent_factory"))

    # Initialize Host with ID and Secret
    host = Host(host_id=HOST_ID, host_secret=HOST_SECRET,
                authority=authority, broker=broker, agent_factory=agent_factory, logger=logging.getLogger("host"))

    await host.connect()


asyncio.run(main())
