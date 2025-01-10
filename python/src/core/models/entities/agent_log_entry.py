from datetime import datetime
from typing import Optional
from pydantic import Field
from core.models.entities.abstract.base_entity import BaseEntity


class AgentLogEntry(BaseEntity):
    agent_id: str = Field(alias="agent_id")
    log_text: Optional[str] = Field(default=None, alias="log_text")
    created_date: Optional[datetime] = Field(
        default=None, alias="created_date")

    class Config:
        exclude_none = True
        populate_by_name = True
