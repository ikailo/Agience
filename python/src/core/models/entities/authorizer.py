from enum import Enum
from typing import Optional
from pydantic import Field, computed_field
from core.models.entities.abstract.public_entity import PublicEntity
from core.models.enums.enums import AuthorizationType


class Authorizer(PublicEntity):
    client_id: Optional[str] = Field(None, alias="client_id")
    client_secret: Optional[str] = Field(
        None, alias="client_secret")
    auth_uri: Optional[str] = Field(None, alias="auth_uri")
    token_uri: Optional[str] = Field(None, alias="token_uri")
    scopes: Optional[str] = Field(None, alias="scopes")
    auth_type: AuthorizationType = Field(
        default=AuthorizationType.Public, alias="auth_type")

    @computed_field(alias="redirect_uri")
    def redirect_uri_path(self) -> str:
        return f"/oauth/authorizer/{self.id}/callback"

    class Config:
        allow_population_by_field_name = True
