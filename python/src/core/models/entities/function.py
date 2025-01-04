from pydantic import Field
from typing import Optional, List
from models.entities.abstract.described_entity import DescribedEntity
from models.entities.parameter import Parameter


class Function(DescribedEntity):
    instruction: Optional[str] = Field(None, alias="instruction")
    inputs: List[Parameter] = Field(default_factory=list, alias="inputs")
    outputs: List[Parameter] = Field(default_factory=list, alias="outputs")

    class Config:
        allow_population_by_field_name = True
