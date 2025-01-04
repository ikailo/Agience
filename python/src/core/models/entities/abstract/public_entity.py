from dataclasses import dataclass, field
from models.entities.abstract.owned_entity import OwnedEntity
from models.enums.enums import Visibility


@dataclass
class PublicEntity(OwnedEntity):
    visibility: Visibility = field(default=Visibility.Private)

    class Config(OwnedEntity.Config):
        json_schema_extra = {
            **OwnedEntity.Config.json_schema_extra,
            "properties": {
                **OwnedEntity.Config.json_schema_extra["properties"],
                "visibility": {"alias": "visibility"}
            }
        }
