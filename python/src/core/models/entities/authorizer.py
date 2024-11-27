from typing import Optional
from .agience_entity import AgienceEntity
from .enum import Visibility, AuthorizationType


class Authorizer(AgienceEntity):
    def __init__(self,
                 name: Optional[str] = None,
                 manager_id: Optional[str] = None,
                 client_id: Optional[str] = None,
                 # TODO: SECURITY: Use a key vault
                 client_secret: Optional[str] = None,
                 auth_uri: Optional[str] = None,
                 token_uri: Optional[str] = None,
                 scope: Optional[str] = None,
                 auth_type: Optional[AuthorizationType] = None,
                 visibility: Optional[Visibility] = Visibility.Private,
                 id: str = ""):
        super().__init__(id=id)
        self.name = name
        self.manager_id = manager_id
        self.client_id = client_id
        self.client_secret = client_secret
        self.auth_uri = auth_uri
        self.token_uri = token_uri
        self.scope = scope
        self.auth_type = auth_type
        self.visibility = visibility

    @property
    def redirect_uri(self) -> str:
        # TODO: Build the Redirect URI based on the type of authorizer, in a different class
        # Include the Authority part
        return f"/manage/authorizer/{self.id}/callback"
