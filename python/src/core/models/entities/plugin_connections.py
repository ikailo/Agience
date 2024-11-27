from typing import Optional
from .agience_entity import AgienceEntity


class PluginConnection(AgienceEntity):
    def __init__(self,
                 name: Optional[str] = None,
                 description: Optional[str] = None,
                 plugin_id: str = "",
                 id: str = ""):
        super().__init__(id=id)
        self.name = name
        self.description = description
        self.plugin_id = plugin_id
