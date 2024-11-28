class ChatMessage:
    def __init__(self, author_role: str, content: str):
        self._author_role = author_role
        self._content = content

    @property
    def author_role(self) -> str:
        return self._author_role

    @property
    def content(self) -> str:
        return self._content
