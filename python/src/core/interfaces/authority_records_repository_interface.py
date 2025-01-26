from abc import ABC, abstractmethod
from typing import List, Optional, Sequence
from core.models.entities.agent import Agent
from core.models.entities.host import Host
from core.models.entities.plugin import Plugin


class IAuthorityRecordsRepository(ABC):
    """Interface for authority records repository operations."""

    @abstractmethod
    async def get_host_by_id(self, host_id: str) -> Optional[Host]:
        """
        Retrieve a host by its ID.

        Args:
            host_id: The unique identifier of the host.

        Returns:
            Optional[Host]: The host if found, None otherwise.
        """
        pass

    @abstractmethod
    async def get_agents_for_host_by_id(self, host_id: str) -> Sequence[Agent]:
        """
        Retrieve all agents associated with a specific host.

        Args:
            host_id: The unique identifier of the host.

        Returns:
            Sequence[Agent]: A sequence of agents associated with the host.
        """
        pass

    @abstractmethod
    async def get_host_id_for_agent_by_id(self, agent_id: str) -> Optional[str]:
        """
        Retrieve the host ID for a specific agent.

        Args:
            agent_id: The unique identifier of the agent.

        Returns:
            Optional[str]: The host ID if found, None otherwise.
        """
        pass

    @abstractmethod
    async def sync_plugins_for_host_by_id(
        self,
        host_id: str,
        plugins: List[Plugin]
    ) -> Sequence[Plugin]:
        """
        Synchronize plugins for a specific host.

        Args:
            host_id: The unique identifier of the host.
            plugins: List of plugins to synchronize.

        Returns:
            Sequence[Plugin]: The synchronized plugins.
        """
        pass

    @abstractmethod
    async def get_credential_for_agent_by_name(
        self,
        agent_id: str,
        credential_name: str
    ) -> str:
        """
        Retrieve a credential for a specific agent by name.

        Args:
            agent_id: The unique identifier of the agent.
            credential_name: The name of the credential to retrieve.

        Returns:
            str: The credential value.
        """
        pass
