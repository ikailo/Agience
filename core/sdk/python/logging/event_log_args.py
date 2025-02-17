from typing import Any, Callable, Optional
from enum import IntEnum
from pydantic import BaseModel


class LogLevel(IntEnum):
    Trace = 0
    Debug = 1
    Information = 2
    Warning = 3
    Error = 4
    Critical = 5
    NoLog = 6


class EventId(BaseModel):
    id: int = 0
    name: Optional[str] = None


class EventLogArgs(BaseModel):
    log_level: LogLevel
    event_id: EventId
    state: Optional[Any] = None
    exception: Optional[Exception] = None
    formatter: Optional[Callable[[Any, Optional[Exception]], str]] = None
    agent_id: Optional[str] = None
    scope: Optional[Any] = None

    class Config:
        arbitrary_types_allowed = True
