import base64
import hashlib
import datetime
from dataclasses import dataclass, field

@dataclass
class Information:
   id: str
   parent_id: str = None
   input: dict = field(default_factory=dict)
   input_agent_id: str = None
   input_timestamp: str = None
   output: dict = field(default_factory=dict)
   output_agent_id: str = None
   output_timestamp: str = None
   function_id: str = None

   def __post_init__(self):
       self.id = base64.urlsafe_b64encode(hashlib.sha256(str(self.id).encode()).digest()).decode()

   @classmethod
   def new(cls, parent_id=None):
       return cls(id=str(uuid.uuid4()), parent_id=parent_id)