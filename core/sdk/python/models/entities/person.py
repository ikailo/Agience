from dataclasses import dataclass
from core.models.entities.abstract.named_entity import NamedEntity


@dataclass
class Person(NamedEntity):
    pass  # No additional fields needed since it just inherits from NamedEntity
