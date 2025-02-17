from abc import ABC, abstractmethod
from typing import Any, Callable, Generic, Optional, TypeVar
from dataclasses import dataclass
from .event_log_args import EventLogArgs, LogLevel, EventId

# Generic type variable for logger type parameter
T = TypeVar('T')
TState = TypeVar('TState')


# TODO: This might need python specific changes
class ILogger(ABC):
    @abstractmethod
    def begin_scope(self, state: Any) -> None:
        pass

    @abstractmethod
    def is_enabled(self, log_level: LogLevel) -> bool:
        pass

    @abstractmethod
    def log(self, log_level: LogLevel, event_id: EventId, state: Any,
            exception: Optional[Exception],
            formatter: Callable[[Any, Optional[Exception]], str]) -> None:
        pass


class AgienceEventLoggerBase(ILogger):
    def __init__(self, agent_id: str):
        self._agent_id = agent_id
        self._log_entry_received_handlers = []  # Python's equivalent of C#'s event

    def add_log_entry_received_handler(self, handler: Callable[[Any, EventLogArgs], None]):
        """Subscribe to log entry events"""
        self._log_entry_received_handlers.append(handler)

    def remove_log_entry_received_handler(self, handler: Callable[[Any, EventLogArgs], None]):
        """Unsubscribe from log entry events"""
        self._log_entry_received_handlers.remove(handler)

    def begin_scope(self, state: Any) -> None:
        return None

    def is_enabled(self, log_level: LogLevel) -> bool:
        return log_level >= LogLevel.Information

    def log(self, log_level: LogLevel, event_id: EventId, state: Any,
            exception: Optional[Exception],
            formatter: Callable[[Any, Optional[Exception]], str]) -> None:
        if not self.is_enabled(log_level):
            return

        # Create event args
        args = EventLogArgs(
            agent_id=self._agent_id,
            log_level=log_level,
            event_id=event_id,
            state=state,
            exception=exception,
            formatter=lambda s, e: formatter(s, e)
        )

        # Notify all handlers
        for handler in self._log_entry_received_handlers:
            handler(self, args)


class EventLogger(Generic[T], AgienceEventLoggerBase):
    def __init__(self, agent_id: str):
        super().__init__(agent_id)


class AgienceEventLogger(AgienceEventLoggerBase):
    def __init__(self, agent_id: str):
        super().__init__(agent_id)


# Example usage
# def log_handler(sender: Any, args: EventLogArgs):
#     print(f"Log from {args.agent_id}: {
#           args.formatter(args.state, args.exception)}")


# # Create logger
# logger = EventLogger[str]("agent1")  # Generic type parameter
# logger.add_log_entry_received_handler(log_handler)

# # Log something
# logger.log(
#     LogLevel.Information,
#     EventId(id=1, name="TestEvent"),
#     "Test message",
#     None,
#     lambda state, exc: f"{state}"
# )
