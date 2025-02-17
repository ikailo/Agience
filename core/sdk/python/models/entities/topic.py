from pydantic import Field
from typing import Optional
from core.models.entities.abstract.public_entity import PublicEntity


class Topic(PublicEntity):
    address: Optional[str] = Field(default=None)

    class Config:
        allow_population_by_field_name = True
