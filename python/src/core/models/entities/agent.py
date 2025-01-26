from pydantic import Field
from typing import Optional, List
from core.models.entities.abstract.owned_entity import OwnedEntity
from core.models.entities.topic import Topic
from core.models.entities.plugin import Plugin
from core.models.enums.enums import CompletionAction


class Agent(OwnedEntity):
    topics: List[Topic] = Field(default_factory=list, alias="topics")
    plugins: List[Plugin] = Field(default_factory=list, alias="plugins")
    persona: Optional[str] = Field(None, alias="persona")
    is_enabled: bool = Field(default=True, alias="is_enabled")
    executive_function_id: Optional[str] = Field(
        None, alias="executive_function_id")
    auto_start_function_id: Optional[str] = Field(
        None, alias="auto_start_function_id")
    on_auto_start_function_complete: Optional[CompletionAction] = Field(
        None, alias="on_auto_start_function_complete")
    host_id: Optional[str] = Field(None, alias="host_id")

    class Config:
        allow_population_by_field_name = True
        use_enum_values = True
