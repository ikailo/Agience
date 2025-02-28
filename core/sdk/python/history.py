import concurrent.futures
import datetime
from typing import Optional, Dict
from dataclasses import dataclass, field
from collections import defaultdict

@dataclass
class InformationVertex:
   id: Optional[str] = None
   input: Optional[dict] = None
   input_timestamp: Optional[datetime.datetime] = None
   output: Optional[dict] = None
   output_timestamp: Optional[datetime.datetime] = None
   function_id: Optional[str] = None

@dataclass
class InformationEdge:
   source: InformationVertex
   target: InformationVertex

class History:
   def __init__(self, id: Optional[str] = None, owner_id: Optional[str] = None):
       self.id = id
       self.owner_id = owner_id
       self.vertices: Dict[str, InformationVertex] = {}
       self.edges: Dict[str, InformationEdge] = {}
       self.vertex_locks: Dict[str, object] = defaultdict(object)
       self.lock = concurrent.futures.RLock()

   def add(self, information):
       with self.vertex_locks[information.id], self.lock:
           if information.id not in self.vertices:
               self.vertices[information.id] = InformationVertex(
                   id=information.id,
                   input=information.input,
                   input_timestamp=datetime.datetime.fromisoformat(information.input_timestamp) if information.input_timestamp else None,
                   output=information.output,
                   output_timestamp=datetime.datetime.fromisoformat(information.output_timestamp) if information.output_timestamp else None,
                   function_id=information.function_id,
               )
           else:
               self.vertices[information.id].input = information.input
               self.vertices[information.id].input_timestamp = datetime.datetime.fromisoformat(information.input_timestamp) if information.input_timestamp else None
               self.vertices[information.id].output = information.output
               self.vertices[information.id].output_timestamp = datetime.datetime.fromisoformat(information.output_timestamp) if information.output_timestamp else None
               self.vertices[information.id].function_id = information.function_id

       if information.parent_id:
           with self.vertex_locks[information.parent_id], self.lock:
               if information.parent_id not in self.vertices:
                   self.vertices[information.parent_id] = InformationVertex(id=information.parent_id)
               self.edges[f"{information.parent_id}-{information.id}"] = InformationEdge(
                   source=self.vertices[information.parent_id],
                   target=self.vertices[information.id]
               )