import logging
from core.host import Host
from core.agent import Agent


class Worker:
    def __init__(self, host: Host, logger: logging.Logger):
        self._logger = logger
        self._host = host
        self._host.agent_connected = self._host_agent_connected

    async def execute_async(self):
        self._logger.info("Starting Host")

        await self._host.run()

        self._logger.info("Host Stopped")

    async def _host_agent_connected(self, agent: Agent):
        self._logger.info(f"Agent {agent.name} Ready")
