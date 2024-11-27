from typing import List, Optional
from .agience_entity import AgienceEntity
from .function import Function
from enum import Enum


class PluginType(Enum):
    Curated = "Curated"


class Visibility(Enum):
    Private = "Private"


class Plugin(AgienceEntity):
    def __init__(self,
                 name: Optional[str] = None,
                 description: Optional[str] = None,
                 type: PluginType = PluginType.Curated,
                 visibility: Visibility = Visibility.Private,
                 functions: Optional[List[Function]] = None,
                 id: str = ""):
        super().__init__(id=id)
        self.name = name
        self.description = description
        self.type = type
        self.visibility = visibility
        self.functions = functions or []  # NotMapped equivalent - just a runtime property
