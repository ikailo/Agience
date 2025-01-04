import json
from typing import Dict, Iterator, Optional, Union, Any
from collections.abc import MutableMapping


class Data(MutableMapping):
    def __init__(self):
        self._structured: Dict[str, Optional[str]] = {}
        self._raw: Optional[str] = None

    @property
    def raw(self) -> Optional[str]:
        if self._raw is None:
            self._raw = json.dumps(self._structured)
        return self._raw

    @raw.setter
    def raw(self, value: Optional[str]):
        self._raw = value
        self._structured.clear()

        if value and value.startswith("{") and value.endswith("}"):
            try:
                elements = json.loads(value)
                if isinstance(elements, dict):
                    for key, element in elements.items():
                        self._structured[key] = (
                            json.dumps(element) if not isinstance(element, str)
                            else element
                        )
            except (json.JSONDecodeError, TypeError):
                # do nothing, similar to C#'s catch blocks
                pass

    def __getitem__(self, key: str) -> Optional[str]:
        return self._structured.get(key)

    def __setitem__(self, key: str, value: Optional[str]):
        self._structured[key] = value
        self._raw = None

    def __delitem__(self, key: str):
        del self._structured[key]
        self._raw = None

    def __iter__(self) -> Iterator[str]:
        return iter(self._structured)

    def __len__(self) -> int:
        return len(self._structured)

    def __str__(self) -> str:
        return self.raw or ""

    @classmethod
    def from_string(cls, raw: Optional[str]) -> 'Data':
        data = cls()
        data.raw = raw
        return data

    def add(self, key: str, value: Optional[str]):
        self._structured[key] = value
        self._raw = None


# Custom JSON encoder/decoder functions to mimic C#'s JsonConverter

def data_encoder(obj: Any) -> str:
    if isinstance(obj, Data):
        return obj.raw or ""
    raise TypeError(f"Object of type {type(obj)} is not JSON serializable")


def data_decoder(obj_dict: Dict) -> Data:
    data = Data()
    data.raw = json.dumps(obj_dict)
    return data
