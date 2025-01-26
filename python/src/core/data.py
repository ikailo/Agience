from typing import Dict, Optional, Iterator, Any, Union
from pydantic import BaseModel, field_serializer, field_validator
import json
from collections.abc import Mapping


class Data(BaseModel, Mapping):
    _structured: Dict[str, Any] = {}
    _raw: Optional[str] = None

    class Config:
        arbitrary_types_allowed = True

    @property
    def raw(self) -> Optional[str]:
        if self._raw is None:
            # Convert any nested dictionaries to their native form before serializing
            cleaned_dict = {}
            for key, value in self._structured.items():
                if isinstance(value, str):
                    try:
                        # Try to parse string as JSON if it looks like JSON
                        if value.startswith('{') and value.endswith('}'):
                            cleaned_dict[key] = json.loads(value)
                        else:
                            cleaned_dict[key] = value
                    except json.JSONDecodeError:
                        cleaned_dict[key] = value
                else:
                    cleaned_dict[key] = value
            self._raw = json.dumps(cleaned_dict)
        return self._raw

    @raw.setter
    def raw(self, value: Optional[str]) -> None:
        self._raw = value
        self._structured.clear()

        if value and value.startswith("{") and value.endswith("}"):
            try:
                elements = json.loads(value)
                if isinstance(elements, dict):
                    for key, element in elements.items():
                        if isinstance(element, (dict, list)):
                            self._structured[key] = json.dumps(element)
                        else:
                            self._structured[key] = element
            except (json.JSONDecodeError, TypeError):
                pass

    def add(self, key: str, value: Any) -> None:
        if isinstance(value, (dict, list)):
            # Store complex objects as native Python objects
            self._structured[key] = value
        else:
            self._structured[key] = value
        self._raw = None

    def __getitem__(self, key: str) -> Optional[str]:
        return self._structured.get(key)

    def __setitem__(self, key: str, value: Any) -> None:
        if isinstance(value, (dict, list)):
            self._structured[key] = value
        else:
            self._structured[key] = value
        self._raw = None

    def __iter__(self) -> Iterator[str]:
        return iter(self._structured)

    def __len__(self) -> int:
        return len(self._structured)

    def __str__(self) -> str:
        return str(self.raw)

    @classmethod
    def __get_validators__(cls):
        yield cls.validate

    @classmethod
    def validate(cls, value: Union[str, 'Data', None]) -> Optional['Data']:
        if value is None:
            return None
        if isinstance(value, Data):
            return value
        if isinstance(value, str):
            return cls(raw=value)
        raise ValueError(f'Cannot convert {type(value)} to Data')

    @field_serializer('raw', check_fields=False)
    def serialize_raw(self, value: Optional[str], _info) -> Optional[str]:
        return value

    def model_dump(self) -> Dict[str, Any]:
        return {"raw": self.raw}

    def copy(self) -> 'Data':
        return Data(raw=self.raw)
