from pydantic import Field
from typing import List
from core.models.entities.abstract.public_entity import PublicEntity
from core.models.entities.agent import Agent
from core.models.entities.plugin import Plugin


class Host(PublicEntity):
    agents: List[Agent] = Field(default_factory=list, alias="agents")
    plugins: List[Plugin] = Field(default_factory=list, alias="plugins")

    class Config:
        allow_population_by_field_name = True
