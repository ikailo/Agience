from enum import Enum, auto


class Visibility(Enum):
    Private = 0
    Public = 1


class AuthorizationType(Enum):
    None_ = 0  # Using None_ since 'None' is reserved in Python
    OAuth2 = 1
    ApiKey = 2


class PluginType(Enum):
    Curated = 0
    Compiled = 1


class CompletionAction(Enum):
    Idle = 0
    Restart = 1
