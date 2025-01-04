from pydantic import BaseModel
from models.entities.abstract.owned_entity import OwnedEntity
from models.enums.enums import Visibility


class PublicEntity(OwnedEntity):
    visibility: Visibility = Visibility.Private
