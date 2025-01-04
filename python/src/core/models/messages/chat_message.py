from typing import Optional
from pydantic import BaseModel, Field


class ChatMessage(BaseModel):
    _author_role: str = Field(alias="author_role")
    _content: str = Field(alias="content")

    def __init__(self, author_role: str, content: str):
        super().__init__(author_role=author_role, content=content)

    @property
    def author_role(self) -> Optional[str]:
        return self._author_role

    @property
    def content(self) -> Optional[str]:
        return self._content

    class Config:
        populate_by_name = True
        frozen = True
