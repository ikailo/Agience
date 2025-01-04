from dataclasses import dataclass, field
from typing import Optional
from models.entities.abstract.named_entity import NamedEntity


@dataclass
class DescribedEntity(NamedEntity):
    description: str = field(default="")

    class Config(NamedEntity.Config):
        json_schema_extra = {
            **NamedEntity.Config.json_schema_extra,
            "properties": {
                **NamedEntity.Config.json_schema_extra["properties"],
                "description": {"alias": "description"}
            }
        }
