from dataclasses import dataclass, field
from typing import Optional
from models.entities.abstract.described_entity import DescribedEntity
from models.entities.person import Person


@dataclass
class OwnedEntity(DescribedEntity):
    owner_id: Optional[str] = None
    owner: Optional[Person] = None

    class Config(DescribedEntity.Config):
        json_schema_extra = {
            **DescribedEntity.Config.json_schema_extra,
            "properties": {
                **DescribedEntity.Config.json_schema_extra["properties"],
                "owner_id": {"alias": "owner_id"},
                "owner": {"alias": "owner"}
            }
        }
