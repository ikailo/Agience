from pydantic import Field
from typing import Optional
from datetime import datetime
from models.entities.abstract.named_entity import NamedEntity


class Key(NamedEntity):
    # ... means required in Pydantic
    host_id: str = Field(..., alias="host_id")
    is_active: bool = Field(default=True, alias="is_active")
    created_date: Optional[datetime] = Field(
        default=None, alias="created_date")

    class Config:
        allow_population_by_field_name = True
