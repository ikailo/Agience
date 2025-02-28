from pydantic import BaseModel
from core.models.entities.abstract.owned_entity import OwnedEntity
from core.models.enums.enums import Visibility


class PublicEntity(OwnedEntity):
    visibility: Visibility = Visibility.Private
