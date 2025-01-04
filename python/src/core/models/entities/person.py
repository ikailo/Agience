from dataclasses import dataclass
from models.entities.abstract.named_entity import NamedEntity


@dataclass
class Person(NamedEntity):
    pass  # No additional fields needed since it just inherits from NamedEntity

    class Config(NamedEntity.Config):
        json_schema_extra = {
            **NamedEntity.Config.json_schema_extra,
            "properties": {
                **NamedEntity.Config.json_schema_extra["properties"]
            }
        }
