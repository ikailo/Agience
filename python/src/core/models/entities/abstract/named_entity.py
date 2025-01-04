from models.entities.abstract.base_entity import BaseEntity


class NamedEntity(BaseEntity):
    name: str = ""
