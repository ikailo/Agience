from typing import Optional
from pydantic import BaseModel
from semantic_kernel.contents import ChatMessageContent


class AgienceChatMessageArgs(BaseModel):
    message: Optional[ChatMessageContent] = None
    agent_id: Optional[str] = None

    @property
    def latest_message(self) -> Optional[str]:
        return self.message.content if self.message else None
