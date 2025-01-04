from pydantic import BaseModel
from typing import Optional


class HostConfig(BaseModel):
    authority_uri_internal: Optional[str] = None
    broker_uri_internal: Optional[str] = None
    authority_uri: Optional[str] = None
    host_id: Optional[str] = None
    host_secret: Optional[str] = None

    class Config:
        allow_population_by_field_name = True
