import os
from dotenv import load_dotenv
import asyncio
from broker import Broker
from host import Host

HOST_ID = os.getenv('HOST_ID')
HOST_SECRET = os.getenv('HOST_SECRET')
BROKER_URI = os.getenv('BROKER_URI')


async def main():
    # Initialize the Broker
    broker = Broker(None)

    # Initialize Host with ID and Secret
    # TODO: Need to initialize Authority and Agent Factory before the host
    host = Host(host_id=HOST_ID, host_secret=HOST_SECRET, authority=None,
                broker=broker, agent_factory=None, logger=None)

    # Get the access token for the host
    host_access_token = host.get_access_token()

    # Connect to the broker using the access token
    await broker.connect(host_access_token, BROKER_URI)


asyncio.run(main())

# authority = Authority(
#     authority_uri="https://auth.example.com",
#     broker=broker,
#     authority_data_adapter=MockAuthorityDataAdapter(),
# )
# await authority.initialize_with_backoff()

# host = Host(
#     host_id="host123",
#     host_secret="supersecret",
#     authority=authority,
#     broker=broker,
#     agent_factory=MockAgentFactory(),
# )
# await host.start()

# agent = Agent(
#     id="agent001",
#     name="MyAgent",
#     authority=authority,
#     broker=broker,
#     plugins=[MockPlugin()],
# )
# await agent.connect()
