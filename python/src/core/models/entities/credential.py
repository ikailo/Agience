from pydantic import Field
from typing import Optional
from datetime import datetime
from core.models.entities.abstract.base_entity import BaseEntity
from core.models.enums.enums import CredentialStatus


class Credential(BaseEntity):
    agent_id: Optional[str] = Field(None, alias="agent_id")
    connection_id: Optional[str] = Field(None, alias="connection_id")
    authorizer_id: Optional[str] = Field(None, alias="authorizer_id")
    status: CredentialStatus = Field(
        default=CredentialStatus.NoAuthorizer, alias="status")
    refresh_token: Optional[str] = Field(None, alias="refresh_token")
    access_token: Optional[str] = Field(None, alias="access_token")
    expiry_date: Optional[datetime] = Field(None, alias="expiry_date")

    class Config:
        allow_population_by_field_name = True
        use_enum_values = True
