from typing import List, Generator
from abc import ABC, abstractmethod
from pydantic import BaseModel
from contextlib import contextmanager
import inspect
from typing import Any, Dict, Type, TypeVar, Optional, Callable
from dataclasses import dataclass
from typing import Any, Callable, Type, Optional
from enum import Enum


class ServiceLifetime(Enum):
    SINGLETON = "singleton"
    SCOPED = "scoped"
    TRANSIENT = "transient"


class ServiceNotFoundError(Exception):
    pass


class CircularDependencyError(Exception):
    pass


@dataclass
class ServiceDescriptor:
    service_type: Type
    implementation_type: Optional[Type] = None
    factory: Optional[Callable[..., Any]] = None
    lifetime: ServiceLifetime = ServiceLifetime.TRANSIENT
    instance: Any = None


T = TypeVar('T')


class ServiceCollection:
    def __init__(self):
        self._descriptors: Dict[Type, ServiceDescriptor] = {}

    def add_singleton(self, service_type: Type[T], implementation_type: Optional[Type[T]] = None) -> 'ServiceCollection':
        return self._add_service(service_type, implementation_type, ServiceLifetime.SINGLETON)

    def add_scoped(self, service_type: Type[T], implementation_type: Optional[Type[T]] = None) -> 'ServiceCollection':
        return self._add_service(service_type, implementation_type, ServiceLifetime.SCOPED)

    def add_transient(self, service_type: Type[T], implementation_type: Optional[Type[T]] = None) -> 'ServiceCollection':
        return self._add_service(service_type, implementation_type, ServiceLifetime.TRANSIENT)

    def add_singleton_factory(self, service_type: Type[T], factory: Callable[..., T]) -> 'ServiceCollection':
        return self._add_factory(service_type, factory, ServiceLifetime.SINGLETON)

    def _add_service(self, service_type: Type[T], implementation_type: Optional[Type[T]], lifetime: ServiceLifetime) -> 'ServiceCollection':
        self._descriptors[service_type] = ServiceDescriptor(
            service_type=service_type,
            implementation_type=implementation_type or service_type,
            lifetime=lifetime
        )
        return self

    def _add_factory(self, service_type: Type[T], factory: Callable[..., T], lifetime: ServiceLifetime) -> 'ServiceCollection':
        self._descriptors[service_type] = ServiceDescriptor(
            service_type=service_type,
            factory=factory,
            lifetime=lifetime
        )
        return self

    def build(self) -> 'ServiceProvider':
        return ServiceProvider(self._descriptors)


class ServiceProvider:
    def __init__(self, descriptors: Dict[Type, ServiceDescriptor]):
        self._descriptors = descriptors
        self._scoped_instances: Dict[Type, Any] = {}
        self._singleton_instances: Dict[Type, Any] = {}

    @contextmanager
    def create_scope(self) -> Generator['ServiceProvider', None, None]:
        scoped_provider = ServiceProvider(self._descriptors)
        scoped_provider._singleton_instances = self._singleton_instances
        try:
            yield scoped_provider
        finally:
            scoped_provider._scoped_instances.clear()

    def get_service(self, service_type: Type[T]) -> T:
        return self._get_service(service_type, set())

    def get_required_service(self, service_type: Type[T]) -> T:
        service = self._get_service(service_type, set())
        if service is None:
            raise ServiceNotFoundError(
                f"Required service {service_type.__name__} not found")
        return service

    def _get_service(self, service_type: Type[T], resolution_stack: set) -> T:
        if service_type in resolution_stack:
            raise CircularDependencyError(
                f"Circular dependency detected: {service_type}")

        descriptor = self._descriptors.get(service_type)
        if not descriptor:
            raise ServiceNotFoundError(
                f"No service registered for type: {service_type}")

        # Check for existing instance based on lifetime
        if descriptor.lifetime == ServiceLifetime.SINGLETON:
            if service_type in self._singleton_instances:
                return self._singleton_instances[service_type]
        elif descriptor.lifetime == ServiceLifetime.SCOPED:
            if service_type in self._scoped_instances:
                return self._scoped_instances[service_type]

        # Create new instance
        resolution_stack.add(service_type)
        try:
            if descriptor.factory:
                instance = descriptor.factory(self)
            else:
                # Get constructor parameters
                params = inspect.signature(
                    descriptor.implementation_type.__init__).parameters
                kwargs = {}

                for name, param in params.items():
                    if name == 'self':
                        continue

                    # Get parameter type annotation
                    param_type = param.annotation
                    if param_type == inspect.Parameter.empty:
                        raise ValueError(f"Missing type annotation for parameter {
                                         name} in {service_type}")

                    # Recursively resolve dependencies
                    kwargs[name] = self._get_service(
                        param_type, resolution_stack)

                instance = descriptor.implementation_type(**kwargs)
        finally:
            resolution_stack.remove(service_type)

        # Store instance based on lifetime
        if descriptor.lifetime == ServiceLifetime.SINGLETON:
            self._singleton_instances[service_type] = instance
        elif descriptor.lifetime == ServiceLifetime.SCOPED:
            self._scoped_instances[service_type] = instance

        return instance

# Example usage:
