from pydantic import BaseModel, Field
from typing import Dict, Optional, Iterator, Union
import json
from collections.abc import Mapping


class Data(BaseModel, Mapping):
    _structured: Dict[str, Optional[str]] = Field(default_factory=dict)
    _raw: Optional[str] = None

    class Config:
        arbitrary_types_allowed = True
        json_encoders = {
            'Data': lambda v: v.raw
        }

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
                        # Handle non-string values by serializing them
                        if not isinstance(element, str):
                            self._structured[key] = json.dumps(element)
                        else:
                            self._structured[key] = element
            except (json.JSONDecodeError, TypeError):
                # Similar to C#'s catch blocks - do nothing on JSON parsing errors
                pass

    def add(self, key: str, value: Optional[str]):
        """Add a key-value pair to the structured data"""
        self._structured[key] = value
        self._raw = None

    def __getitem__(self, key: str) -> Optional[str]:
        return self._structured.get(key)

    def __setitem__(self, key: str, value: Optional[str]):
        self._structured[key] = value
        self._raw = None

    def __iter__(self) -> Iterator[str]:
        return iter(self._structured)

    def __len__(self) -> int:
        return len(self._structured)

    def __str__(self) -> str:
        return self.raw or ""

    @classmethod
    def __get_validators__(cls):
        # Pydantic validation
        yield cls.validate

    @classmethod
    def validate(cls, value):
        if isinstance(value, str):
            return cls(raw=value)
        elif isinstance(value, cls):
            return value
        elif isinstance(value, dict):
            instance = cls()
            for k, v in value.items():
                instance[k] = v
            return instance
        raise ValueError(f'Cannot convert {type(value)} to Data')

    def json(self, **kwargs):
        return self.raw


class DataJsonEncoder(json.JSONEncoder):
    def default(self, obj):
        if isinstance(obj, Data):
            return obj.raw
        return super().default(obj)


class DataJsonDecoder(json.JSONDecoder):
    def decode(self, s):
        obj = super().decode(s)
        if isinstance(obj, str):
            return Data(raw=obj)
        return obj
