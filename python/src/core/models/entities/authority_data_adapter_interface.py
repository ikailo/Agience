from typing import Optional, List
from abc import ABC, abstractmethod
from .host import Host
from .agent import Agent
from .plugin import Plugin


class IAuthorityDataAdapter(ABC):
    @abstractmethod
    async def get_host_by_id_no_tracking(self, host_id: str) -> Host:
        pass

    @abstractmethod
    async def get_agents_for_host_id_no_tracking(self, host_id: str) -> List[Agent]:
        pass

    @abstractmethod
    async def get_plugins_for_host_id_no_tracking(self, host_id: str) -> List[Plugin]:
        pass

    @abstractmethod
    async def get_host_id_for_agent_id_no_tracking(self, agent_id: str) -> Optional[str]:
        pass
