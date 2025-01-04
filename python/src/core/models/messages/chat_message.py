from dataclasses import dataclass
from typing import Optional


@dataclass
class ChatMessage:
    author_role: str
    content: str

    @property
    def author_role(self) -> Optional[str]:
        return self.author_role

    @property
    def content(self) -> Optional[str]:
        return self.content
