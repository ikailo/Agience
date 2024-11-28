from enum import Enum
from typing import Optional, Union

class BrokerMessageType(Enum):
    EVENT = "EVENT"
    INFORMATION = "INFORMATION"
    CONTEXT = "CONTEXT"
    UNKNOWN = "UNKNOWN"

class BrokerMessage:
    def __init__(self, 
                 type_: BrokerMessageType = BrokerMessageType.UNKNOWN, 
                 topic: Optional[str] = None):
        self.type = type_
        self.topic = topic
        self._content = None

    @property
    def sender_id(self) -> Optional[str]:
        return self.topic.split('/')[0] if self.topic else None

    @property
    def destination(self) -> Optional[str]:
        return self.topic[self.topic.index('/') + 1:] if self.topic and '/' in self.topic else None

    @property
    def data(self) -> Optional['Data']:
        if self.type == BrokerMessageType.EVENT:
            return self._content
        return None

    @data.setter
    def data(self, value: Optional['Data']):
        if self.type == BrokerMessageType.EVENT:
            self._content = value

    @property
    def information(self) -> Optional['Information']:
        if self.type == BrokerMessageType.INFORMATION:
            return self._content
        return None

    @information.setter
    def information(self, value: Optional['Information']):
        if self.type == BrokerMessageType.INFORMATION:
            self._content = value
