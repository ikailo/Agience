import uuid
import hashlib
import base64
from typing import Optional
from dataclasses import dataclass, field

from .data import Data


@dataclass
class Information:
    # Initialize with generated ID and optional parent ID
    id: str = field(init=False)
    parent_id: Optional[str] = None

    # Input-related fields
    input_data: Optional[Data] = field(default=None, init=False)
    input_agent_id: Optional[str] = field(default=None, init=False)
    input_timestamp: Optional[str] = field(default=None, init=False)

    # Output-related fields
    output_data: Optional[Data] = field(default=None, init=False)
    output_agent_id: Optional[str] = field(default=None, init=False)
    output_timestamp: Optional[str] = field(default=None, init=False)

    # Function ID
    function_id: Optional[str] = field(default=None, init=False)

    def __post_init__(self):
        # Generate ID using SHA-256 of a new UUID
        uuid_bytes = uuid.uuid4().bytes
        sha256_hash = hashlib.sha256(uuid_bytes).digest()
        # Use URL-safe base64 encoding and remove padding
        self.id = base64.urlsafe_b64encode(
            sha256_hash).decode('ascii').rstrip('=')

    @property
    def input(self) -> Optional[Data]:
        return self.input_data

    @input.setter
    def input(self, value: Optional[Data]):
        self.input_data = value

    @property
    def input_agent_id(self) -> Optional[str]:
        return self.input_agent_id

    @input_agent_id.setter
    def input_agent_id(self, value: Optional[str]):
        self.input_agent_id = value

    @property
    def input_timestamp(self) -> Optional[str]:
        return self.input_timestamp

    @input_timestamp.setter
    def input_timestamp(self, value: Optional[str]):
        self.input_timestamp = value

    @property
    def output(self) -> Optional[Data]:
        return self.output_data

    @output.setter
    def output(self, value: Optional[Data]):
        self.output_data = value

    @property
    def output_agent_id(self) -> Optional[str]:
        return self.output_agent_id

    @output_agent_id.setter
    def output_agent_id(self, value: Optional[str]):
        self.output_agent_id = value

    @property
    def output_timestamp(self) -> Optional[str]:
        return self.output_timestamp

    @output_timestamp.setter
    def output_timestamp(self, value: Optional[str]):
        self.output_timestamp = value

    @property
    def function_id(self) -> Optional[str]:
        return self.function_id

    @function_id.setter
    def function_id(self, value: Optional[str]):
        self.function_id = value

    @property
    def parent_id(self) -> Optional[str]:
        return self.parent_id

    @parent_id.setter
    def parent_id(self, value: Optional[str]):
        self.parent_id = value
