from datetime import datetime
from typing import Dict, Optional
from pydantic import BaseModel, Field


class BaseEntity(BaseModel):
    id: Optional[str] = None
    created_date: Optional[datetime] = None
    metadata: Dict[str, Optional[object]] = Field(
        default_factory=dict, exclude=True)
