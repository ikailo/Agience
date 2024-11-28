from dataclasses import dataclass
from typing import Optional


@dataclass
class HostConfig:
    authority_uri: Optional[str] = None
    host_id: Optional[str] = None
    host_secret: Optional[str] = None
