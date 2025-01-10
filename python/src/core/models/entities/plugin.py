from pydantic import Field
from typing import Optional, List, Type
from core.models.entities.abstract.public_entity import PublicEntity
from core.models.entities.function import Function
from core.models.enums.enums import PluginProvider, PluginSource


class Plugin(PublicEntity):
    type: Optional[Type] = Field(None, exclude=True)
    unique_name: Optional[str] = Field(None, alias="unique_name")
    plugin_provider: PluginProvider = Field(
        default=PluginProvider.Prompt, alias="plugin_provider")
    plugin_source: PluginSource = Field(
        default=PluginSource.UserDefined, alias="plugin_source")
    functions: List[Function] = Field(default_factory=list, alias="functions")

    class Config:
        allow_population_by_field_name = True
        use_enum_values = True
