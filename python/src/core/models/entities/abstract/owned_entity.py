from typing import Optional
from models.entities.abstract.described_entity import DescribedEntity
from models.entities.person import Person


class OwnedEntity(DescribedEntity):
    owner_id: Optional[str] = None
    owner: Optional[Person] = None
