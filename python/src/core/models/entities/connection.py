from pydantic import Field
from typing import Optional
from datetime import datetime  # Assuming PublicEntity has timestamp fields
from models.entities.abstract.public_entity import PublicEntity


class Connection(PublicEntity):
    resource_uri: Optional[str] = Field(None, alias="resource_uri")

    class Config:
        allow_population_by_field_name = True
