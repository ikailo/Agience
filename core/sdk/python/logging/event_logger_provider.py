from typing import Type, TypeVar, List, Optional, Any, Dict
from abc import ABC, abstractmethod
import inspect
from .event_logger import EventLogger, AgienceEventLogger, AgienceEventLoggerBase, ILogger
from .event_log_args import EventLogArgs

T = TypeVar('T')


class IEventLogHandler(ABC):
    @abstractmethod
    def on_log_entry_received(self, sender: Any, e: EventLogArgs) -> None:
        pass


class NullLogger(ILogger):
    """Implementation of the null object pattern for ILogger"""
    _instance = None

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance

    def begin_scope(self, state: Any) -> None:
        return None

    def is_enabled(self, log_level: Any) -> bool:
        return False

    def log(self, *args, **kwargs) -> None:
        pass


class ServiceProvider:
    """Simple service provider implementation"""

    def __init__(self):
        self._services: Dict[Type, List[Any]] = {}

    def register_service(self, service_type: Type, implementation: Any) -> None:
        if service_type not in self._services:
            self._services[service_type] = []
        self._services[service_type].append(implementation)

    def get_services(self, service_type: Type) -> List[Any]:
        return self._services.get(service_type, [])


class EventLoggerProvider:
    def __init__(self, service_provider: ServiceProvider):
        self._service_provider = service_provider
        self._created_loggers: List[ILogger] = []

    def create_logger(self, category_name: str) -> ILogger:
        return NullLogger()

    def create_logger_with_agent(self, category_name: str, agent_id: Optional[str]) -> ILogger:
        return self._create_logger_internal(object, agent_id)

    def create_logger_generic(self, T: Type, agent_id: Optional[str]) -> ILogger:
        return self._create_logger_internal(T, agent_id)

    def _create_logger_internal(self, logger_type: Type, agent_id: Optional[str]) -> ILogger:
        # Create logger instance
        if logger_type != object:
            logger = EventLogger[logger_type](agent_id)
        else:
            logger = AgienceEventLogger(agent_id)

        # Add event handlers if it's an AgienceEventLoggerBase
        if isinstance(logger, AgienceEventLoggerBase):
            handlers = self._service_provider.get_services(IEventLogHandler)

            for handler in handlers:
                # Create closure for handler
                def create_handler(h):
                    def handle_event(sender: Any, e: EventLogArgs):
                        h.on_log_entry_received(sender, e)
                    return handle_event

                logger.add_log_entry_received_handler(create_handler(handler))

        self._created_loggers.append(logger)
        return logger

    def dispose(self) -> None:
        for logger in self._created_loggers:
            if hasattr(logger, 'dispose'):
                logger.dispose()
        self._created_loggers.clear()

# # Example usage


# class ConsoleEventLogHandler(IEventLogHandler):
#     def on_log_entry_received(self, sender: Any, e: EventLogArgs) -> None:
#         print(f"Log from {e.agent_id}: {e.formatter(e.state, e.exception)}")

# # Example setup


# def create_logger_provider() -> EventLoggerProvider:
#     # Set up service provider
#     service_provider = ServiceProvider()

#     # Register log handlers
#     service_provider.register_service(
#         IEventLogHandler, ConsoleEventLogHandler())

#     # Create provider
#     return EventLoggerProvider(service_provider)

# Create provider
# provider = create_logger_provider()

# try:
#     # Create different types of loggers
#     generic_logger = provider.create_logger_generic(str, "agent1")
#     regular_logger = provider.create_logger_with_agent("CategoryName", "agent2")
#     null_logger = provider.create_logger("CategoryName")

#     # Use loggers
#     generic_logger.log(
#         LogLevel.Information,
#         EventId(id=1, name="TestEvent"),
#         "Test message",
#         None,
#         lambda state, exc: f"{state}"
#     )
# finally:
#     provider.dispose()
