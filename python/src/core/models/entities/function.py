from .agience_entity import AgienceEntity
from typing import Optional


class Function(AgienceEntity):
    def __init__(self,
                 name: Optional[str] = None,
                 description: Optional[str] = None,
                 prompt: Optional[str] = None,
                 id: str = ""):
        super().__init__(id=id)
        self.name = name
        self.description = description
        self.prompt = prompt

        # Commented out as in original:
        # self.input_variables = []
        # self.output_variable = None
        # self.execution_settings = {}
