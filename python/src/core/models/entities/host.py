from typing import List, Optional
from .agience_entity import AgienceEntity
from models.entities.agent import Agent
from .enum import Visibility


class Host(AgienceEntity):
    def __init__(self,
                 name: Optional[str] = None,
                 agents: Optional[List[Agent]] = None,
                 visibility: Visibility = Visibility.Private,
                 id: str = ""):
        super().__init__(id=id)
        self.name = name
        self.agents = agents or []
        self.visibility = visibility
