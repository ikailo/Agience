import asyncio
# from authority import Authority
from broker import Broker
# from host import Host
# from agent import Agent

token = ""


async def main():
    broker = Broker(None)

    await broker.connect(token, "")

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

asyncio.run(main())
