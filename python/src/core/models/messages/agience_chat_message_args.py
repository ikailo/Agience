from dataclasses import dataclass
from typing import Optional
from semantic_kernel.contents import ChatMessageContent


@dataclass
class AgienceChatMessageArgs:
    message: Optional[ChatMessageContent] = None
    agent_id: Optional[str] = None

    @property
    def latest_message(self) -> Optional[str]:
        return self.message.content if self.message else None
