from datetime import datetime
from .agience_entity import AgienceEntity


class Agency(AgienceEntity):
    def __init__(self, name=None, description=None, **kwargs):
        super().__init__(**kwargs)
        self.name = name
        self.description = description
