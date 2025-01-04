from datetime import datetime
from typing import Dict, Optional
from dataclasses import dataclass, field


@dataclass
class BaseEntity:
    id: Optional[str] = None
    created_date: Optional[datetime] = None
    metadata: Dict[str, Optional[object]] = field(default_factory=dict)

    class Config:
        # This is similar to the JsonPropertyName attributes in C#
        json_schema_extra = {
            "properties": {
                "id": {"alias": "id"},
                "created_date": {"alias": "created_date"}
            }
        }
        # Similar to JsonIgnore in C#
        json_exclude = {"metadata"}
