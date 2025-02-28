from typing import Optional
from core.models.entities.abstract.described_entity import DescribedEntity
from core.models.entities.person import Person


class OwnedEntity(DescribedEntity):
    owner_id: Optional[str] = None
    owner: Optional[Person] = None
