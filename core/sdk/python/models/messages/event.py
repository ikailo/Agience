from enum import Enum, auto


class Event(Enum):
    LOAD_PLUGINS = auto()
    UNLOAD_PLUGINS = auto()
    CONNECT_AGENT = auto()
    DISCONNECT_AGENT = auto()
