from typing import TypeVar, Generic, List, Dict, Any, Optional, Callable, Iterator
from contextlib import contextmanager
from .event_logger import ILogger
from .event_log_args import LogLevel, EventId

T = TypeVar('T')


class CompositeDisposable:
    def __init__(self, disposables: List[Any]):
        self._disposables = disposables

    def __enter__(self):
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        for disposable in self._disposables:
            if hasattr(disposable, '__exit__'):
                disposable.__exit__(exc_type, exc_val, exc_tb)
            elif hasattr(disposable, 'dispose'):
                disposable.dispose()


class AgienceScopedCompositeLoggerBase(ILogger):
    def __init__(self, loggers: List[ILogger], scope: Optional[Dict[str, Any]] = None):
        self._loggers = loggers
        self._scope = scope

    @contextmanager
    def _current_scope(self):
        if not self._scope:
            yield
            return

        scope_str = ", ".join(f"{key}:{value}" for key,
                              value in self._scope.items())
        with self.begin_scope(scope_str):
            yield

    def begin_scope(self, state: Any) -> CompositeDisposable:
        scopes = []

        for logger in self._loggers:
            scope = logger.begin_scope(state)
            if scope is not None:
                scopes.append(scope)

        return CompositeDisposable(scopes)

    def is_enabled(self, log_level: LogLevel) -> bool:
        return any(logger.is_enabled(log_level) for logger in self._loggers)

    def log(self, log_level: LogLevel, event_id: EventId, state: Any,
            exception: Optional[Exception],
            formatter: Callable[[Any, Optional[Exception]], str]) -> None:
        with self._current_scope():
            for logger in self._loggers:
                if logger.is_enabled(log_level):
                    logger.log(log_level, event_id, state,
                               exception, formatter)


class ScopedCompositeLogger(Generic[T], AgienceScopedCompositeLoggerBase):
    def __init__(self, loggers: List[ILogger], scope: Optional[Dict[str, Any]] = None):
        super().__init__(loggers, scope)


class AgienceScopedCompositeLogger(AgienceScopedCompositeLoggerBase):
    def __init__(self, loggers: List[ILogger], scope: Optional[Dict[str, Any]] = None):
        super().__init__(loggers, scope)


# # Example usage
# def create_composite_logger(loggers: List[ILogger], scope: Optional[Dict[str, Any]] = None) -> ILogger:
#     return ScopedCompositeLogger[str](loggers, scope)


# # Create some loggers
# loggers = [
#     EventLogger("agent1"),
#     EventLogger("agent2")
# ]

# # Create a composite logger with scope
# scope = {"AgentId": "composite1", "Environment": "prod"}
# composite_logger = create_composite_logger(loggers, scope)

# # Use the logger
# composite_logger.log(
#     LogLevel.Information,
#     EventId(id=1, name="TestEvent"),
#     "Test message",
#     None,
#     lambda state, exc: f"{state}"
# )

# # Using scopes
# with composite_logger.begin_scope({"RequestId": "123"}):
#     composite_logger.log(
#         LogLevel.Information,
#         EventId(id=2, name="ScopedEvent"),
#         "Scoped message",
#         None,
#         lambda state, exc: f"{state}"
#     )
