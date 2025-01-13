from enum import Enum
from typing import Optional, Union, Any
from pydantic import BaseModel, Field, computed_field, PrivateAttr
from core.data import Data
from core.information import Information


class BrokerMessageType(Enum):
    EVENT = "EVENT"
    INFORMATION = "INFORMATION"
    CONTEXT = "CONTEXT"
    UNKNOWN = "UNKNOWN"


class BrokerMessage(BaseModel):
    type: BrokerMessageType = Field(default=BrokerMessageType.UNKNOWN)
    topic: Optional[str] = None

    data: Optional[Data] = None
    information: Optional[Information] = None

    def model_post_init(self, __context: Any) -> None:
        if self.type == BrokerMessageType.EVENT:
            self.information = None
        elif self.type == BrokerMessageType.INFORMATION:
            self.data = None

    @computed_field
    def sender_id(self) -> Optional[str]:
        if self.topic:
            parts = self.topic.split('/')
            if len(parts) > 1:
                return parts[1]
        return None

    @computed_field
    def destination(self) -> Optional[str]:
        if self.topic:
            parts = self.topic.split('/')
            if len(parts) > 2:
                return "/".join(parts[2:])
        return None
