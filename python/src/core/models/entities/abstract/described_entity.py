from models.entities.abstract.named_entity import NamedEntity


class DescribedEntity(NamedEntity):
    description: str = ""
