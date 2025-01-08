from abc import ABC, abstractmethod
from typing import Optional, Any
from pydantic import BaseModel
from core.logging.event_log_args import EventLogArgs


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
