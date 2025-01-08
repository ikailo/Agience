from abc import ABC, abstractmethod
from typing import Optional, Any
from pydantic import BaseModel

# TODO: Replace with actual event log arguments


class EventLogArgs(BaseModel):
    """
    Pydantic model for event log arguments.
    Note: Define the specific fields needed based on your EventLogArgs C# class
    """
    pass


class IEventLogHandler(ABC):
    """Interface for handling event log entries."""

    @abstractmethod
    def on_log_entry_received(self, sender: Optional[Any], args: EventLogArgs) -> None:
        """
        Handle received log entries.

        Args:
            sender: The object that triggered the event, if any.
            args: Event arguments containing log entry data.
        """
        pass
