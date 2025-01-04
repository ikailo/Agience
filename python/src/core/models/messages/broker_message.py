from dataclasses import dataclass, field
from enum import Enum
from typing import Optional, Union
from core.data import Data
from core.information import Information


class BrokerMessageType(Enum):
    EVENT = "EVENT"
    INFORMATION = "INFORMATION"
    CONTEXT = "CONTEXT"
    UNKNOWN = "UNKNOWN"


@dataclass
class BrokerMessage:
    type: BrokerMessageType = field(default=BrokerMessageType.UNKNOWN)
    topic: Optional[str] = None

    _content: Optional[Union[Data, Information]
                       ] = field(default=None, repr=False)

    @property
    def sender_id(self) -> Optional[str]:
        if self.topic:
            parts = self.topic.split('/')
            if len(parts) > 1:
                return parts[1]
        return None

    @property
    def destination(self) -> Optional[str]:
        if self.topic:
            parts = self.topic.split('/')
            if len(parts) > 2:
                return "/".join(parts[2:])
        return None

    @property
    def data(self) -> Optional[Data]:
        if self.type == BrokerMessageType.EVENT:
            return self._content
        return None

    @data.setter
    def data(self, value: Optional[Data]):
        if self.type == BrokerMessageType.EVENT:
            self._content = value

    @property
    def information(self) -> Optional[Information]:
        if self.type == BrokerMessageType.INFORMATION:
            return self._content
        return None

    @information.setter
    def information(self, value: Optional[Information]):
        if self.type == BrokerMessageType.INFORMATION:
            self._content = value
