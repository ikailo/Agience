from pydantic import BaseModel, Field
from typing import Optional
import hashlib
import uuid
import base64

from core.data import Data


class Information(BaseModel):
    id: str
    parent_id: Optional[str] = None
    input_data: Optional['Data'] = Field(None, alias='input')
    input_agent_id: Optional[str] = None
    input_timestamp: Optional[str] = None
    output_data: Optional['Data'] = Field(None, alias='output')
    output_agent_id: Optional[str] = None
    output_timestamp: Optional[str] = None
    function_id: Optional[str] = None

    def __init__(self, parent_id: Optional[str] = None, **data):
        guid_bytes = uuid.uuid4().bytes
        sha256_hash = hashlib.sha256(guid_bytes).digest()
        id_value = base64.urlsafe_b64encode(
            sha256_hash).rstrip(b'=').decode('ascii')

        super().__init__(id=id_value, parent_id=parent_id, **data)

    class Config:
        allow_population_by_field_name = True
