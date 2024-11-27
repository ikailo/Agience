from typing import List, Optional
from .agience_entity import AgienceEntity
from .agency import Agency
from .plugin import Plugin
from .host import Host
from .enum import CompletionAction


class Agent(AgienceEntity):
    def __init__(self,
                 name: Optional[str] = None,
                 description: Optional[str] = None,
                 persona: Optional[str] = None,
                 is_enabled: bool = True,
                 agency_id: str = "",
                 agency: Optional[Agency] = None,
                 plugins: Optional[List[Plugin]] = None,
                 chat_completion_function_name: Optional[str] = None,
                 auto_start_function_name: Optional[str] = None,
                 auto_start_function_completion_action: Optional[CompletionAction] = None,
                 host_id: Optional[str] = None,
                 host: Optional[Host] = None,
                 id: str = ""):
        super().__init__(id=id)
        self.name = name
        self.description = description
        self.persona = persona
        self.is_enabled = is_enabled
        self.agency_id = agency_id
        self.agency = agency
        self.plugins = plugins or []
        self.chat_completion_function_name = chat_completion_function_name
        self.auto_start_function_name = auto_start_function_name
        self.auto_start_function_completion_action = auto_start_function_completion_action

        # Host relationship (single host for now)
        self.host_id = host_id
        self._host = host  # Private attribute to mimic JsonIgnore
