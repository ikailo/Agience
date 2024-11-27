from typing import Optional
from .agience_entity import AgienceEntity


class Credential(AgienceEntity):
    def __init__(self,
                 # TODO: SECURITY: Use a key vault
                 secret: Optional[str] = None,
                 id: str = ""):
        super().__init__(id=id)
        self.secret = secret
