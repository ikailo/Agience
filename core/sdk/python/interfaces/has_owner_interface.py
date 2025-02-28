from abc import ABC, abstractmethod
from typing import Optional
from pydantic import BaseModel, Field


class IHasOwner(BaseModel):
    """Interface for objects that have an owner."""

    owner_id: Optional[str] = Field(
        default=None,
        description="The unique identifier of the owner",
        alias="owner_id"
    )
