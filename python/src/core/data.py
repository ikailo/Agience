import json
from typing import Optional, Dict

class Data:
    def __init__(self, raw: Optional[str] = None):
        self._structured: Dict[str, str] = {}
        self._raw = raw

    @property
    def raw(self) -> str:
        if self._raw is None:
            self._raw = json.dumps(self._structured)
        return self._raw

    @raw.setter
    def raw(self, value: str):
        self._raw = value
        self._structured.clear()
        if value and value.startswith('{') and value.endswith('}'):
            try:
                self._structured = json.loads(value)
            except (json.JSONDecodeError, ValueError):
                pass

    def add(self, key: str, value: Optional[str]):
        self._structured[key] = value
        self._raw = None

    def __getitem__(self, key: str) -> Optional[str]:
        return self._structured.get(key)

    def __setitem__(self, key: str, value: Optional[str]):
        self._structured[key] = value
        self._raw = None

    def __iter__(self):
        return iter(self._structured.items())

    def __str__(self):
        return self.raw