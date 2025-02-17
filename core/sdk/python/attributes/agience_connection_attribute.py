from typing import Callable, TypeVar, Any
from functools import wraps

T = TypeVar('T', bound=Callable[..., Any])


class AgienceConnection:
    """
    A decorator for marking methods as Agience connections.
    """

    def __init__(self, name: str):
        self.name = name

    def __call__(self, func: T) -> T:
        @wraps(func)
        def wrapper(*args: Any, **kwargs: Any) -> Any:
            wrapper.connection_name = self.name
            return func(*args, **kwargs)

        # Store the connection name as metadata on the function
        wrapper.connection_name = self.name
        wrapper.__agience_connection_name__ = self.name

        return wrapper  # type: ignore

# Example usage:
#
# class MyClass:
#     @AgienceConnection("connection_name")
#     def my_method(self):
#         pass
