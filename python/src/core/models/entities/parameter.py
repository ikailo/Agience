from pydantic import Field
from typing import Optional
from datetime import datetime
from models.entities.abstract.described_entity import DescribedEntity


class Parameter(DescribedEntity):
    type: Optional[str] = Field(None, alias="type")
    input_function_id: Optional[str] = Field(None, alias="input_function_id")
    output_function_id: Optional[str] = Field(None, alias="output_function_id")

    class Config:
        allow_population_by_field_name = True
        # json_encoders = {
        #     datetime: lambda v: v.isoformat()
        # }
