from enum import Enum
from dataclasses import dataclass
from typing import Optional, Union


class BrokerMessageType(Enum):
    EVENT = "EVENT"
    INFORMATION = "INFORMATION"
    CONTEXT = "CONTEXT"
    UNKNOWN = "UNKNOWN"


@dataclass
class Data:
    raw: str
    # Add other data fields as needed


@dataclass
class Information:
    status: str
    # Add other information fields as needed


@dataclass
class BrokerMessage:
    type: BrokerMessageType = BrokerMessageType.UNKNOWN
    topic: Optional[str] = None
    _content: Optional[Union[Data, Information]] = None

    @property
    def sender_id(self) -> Optional[str]:
        return self.topic.split('/')[0] if self.topic else None

    @property
    def destination(self) -> Optional[str]:
        return self.topic[self.topic.index('/')+1:] if self.topic else None

    @property
    def data(self) -> Optional[Data]:
        return self._content if self.type == BrokerMessageType.EVENT else None

    @data.setter
    def data(self, value: Optional[Data]):
        if self.type == BrokerMessageType.EVENT:
            self._content = value

    @property
    def information(self) -> Optional[Information]:
        return self._content if self.type == BrokerMessageType.INFORMATION else None

    @information.setter
    def information(self, value: Optional[Information]):
        if self.type == BrokerMessageType.INFORMATION:
            self._content = value
