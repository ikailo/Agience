from typing import TypeVar, Generic, List, Dict, Any, Optional, Type
from abc import ABC, abstractmethod
from .event_logger import ILogger
from .event_logger_provider import EventLoggerProvider
from .scoped_composite_logger import ScopedCompositeLogger

# Generic type variable for logger types
TLogger = TypeVar('TLogger', bound=ILogger)
T = TypeVar('T')


class ILoggerProvider(ABC):
    @abstractmethod
    def create_logger(self, category_name: str) -> ILogger:
        pass

    @abstractmethod
    def dispose(self) -> None:
        pass


class ILoggerFactory(ABC):
    @abstractmethod
    def create_logger(self, category_name: str) -> ILogger:
        pass

    @abstractmethod
    def create_logger_generic(self, T: Type) -> ILogger:
        pass

    @abstractmethod
    def add_provider(self, provider: ILoggerProvider) -> None:
        pass

    @abstractmethod
    def dispose(self) -> None:
        pass


class EventLoggerFactory(ILoggerFactory):
    def __init__(self, agent_id: Optional[str]):
        self._providers: List[ILoggerProvider] = []
        self._agent_id = agent_id

    def create_logger(self, category_name: str) -> ILogger:
        return self._create_scoped_logger(ILogger, category_name, self._agent_id)

    def create_logger_generic(self, T: Type) -> ILogger:
        category_name = T.__module__ + '.' + T.__name__
        return self._create_scoped_logger(ILogger, category_name, self._agent_id)

    def _create_scoped_logger(self, logger_type: Type[TLogger],
                              category_name: str,
                              agent_id: Optional[str]) -> TLogger:
        loggers: List[ILogger] = []

        for provider in self._providers:
            if isinstance(provider, EventLoggerProvider):
                logger = provider.create_logger_generic(logger_type, agent_id)
            else:
                logger = provider.create_logger(category_name)
            loggers.append(logger)

        scope_data: Dict[str, Any] = {}
        if agent_id:
            scope_data["AgentId"] = agent_id

        return ScopedCompositeLogger(loggers, scope_data)  # type: ignore

    def add_provider(self, provider: ILoggerProvider) -> None:
        self._providers.append(provider)

    def dispose(self) -> None:
        for provider in self._providers:
            provider.dispose()
        self._providers.clear()


# Example usage
# def create_logger_factory(agent_id: Optional[str]) -> EventLoggerFactory:
#     factory = EventLoggerFactory(agent_id)
#     factory.add_provider(EventLoggerProvider())
#     return factory


# Example usage
# factory = create_logger_factory("agent1")
# logger = factory.create_logger("CategoryName")
# typed_logger = factory.create_logger_generic(str)  # For ILogger<string> equivalent

# try:
#     # Use the logger
#     pass
# finally:
#     factory.dispose()
