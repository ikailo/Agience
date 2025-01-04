from dataclasses import dataclass, field
from typing import Optional
from models.entities.abstract.base_entity import BaseEntity


@dataclass
class NamedEntity(BaseEntity):
    name: str = field(default="")

    class Config(BaseEntity.Config):
        json_schema_extra = {
            **BaseEntity.Config.json_schema_extra,
            "properties": {
                **BaseEntity.Config.json_schema_extra["properties"],
                "name": {"alias": "name"}
            }
        }
