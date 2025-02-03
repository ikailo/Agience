from typing import Type, TypeVar, Callable, Dict, Any, Optional
from pydantic import BaseModel

T = TypeVar('T')


class ServiceDescriptor(BaseModel):
    service_type: Type
    implementation: Any
    lifetime: str  # 'singleton', 'scoped', 'transient'
    factory: Callable = None


class ServiceContainer:
    def __init__(self):
        self._services: Dict[Type, ServiceDescriptor] = {}
        self._scoped_instances: Dict[Type, Any] = {}

    def add_singleton(self, service_type: Type[T], implementation: Type[T] = None):
        self._add_service(service_type, implementation, 'singleton')

    def add_scoped(self, service_type: Type[T], implementation: Type[T] = None):
        self._add_service(service_type, implementation, 'scoped')

    def add_transient(self, service_type: Type[T], implementation: Type[T] = None):
        self._add_service(service_type, implementation, 'transient')

    def add_factory(self, service_type: Type[T], factory: Callable[..., T]):
        self._services[service_type] = ServiceDescriptor(
            service_type=service_type,
            implementation=None,
            lifetime='transient',
            factory=factory
        )

    def _add_service(self, service_type: Type[T], implementation: Type[T], lifetime: str):
        if implementation is None:
            implementation = service_type
        self._services[service_type] = ServiceDescriptor(
            service_type=service_type,
            implementation=implementation,
            lifetime=lifetime
        )

    def get_service(self, service_type: Type[T], **kwargs) -> T:
        descriptor = self._services.get(service_type)
        if not descriptor:
            raise ValueError(f"Service {service_type} not registered.")

        if descriptor.lifetime == 'singleton':
            if not hasattr(descriptor.implementation, '_instance'):
                descriptor.implementation._instance = descriptor.implementation(
                    **kwargs)
            return descriptor.implementation._instance

        elif descriptor.lifetime == 'scoped':
            if service_type not in self._scoped_instances:
                self._scoped_instances[service_type] = descriptor.implementation(
                    **kwargs)
            return self._scoped_instances[service_type]

        elif descriptor.lifetime == 'transient':
            if descriptor.factory:
                return descriptor.factory(self, **kwargs)
            return descriptor.implementation(**kwargs)

        else:
            raise ValueError(f"Invalid lifetime {
                             descriptor.lifetime} for service {service_type}.")

    def create_scope(self):
        return ServiceScope(self)


class ServiceScope:
    def __init__(self, container: ServiceContainer):
        self._container = container
        self._scoped_instances = {}

    def get_service(self, service_type: Type[T], **kwargs) -> T:
        if service_type in self._scoped_instances:
            return self._scoped_instances[service_type]

        descriptor = self._container._services.get(service_type)
        if not descriptor:
            raise ValueError(f"Service {service_type} not registered.")

        if descriptor.lifetime == 'scoped':
            instance = descriptor.implementation(**kwargs)
            self._scoped_instances[service_type] = instance
            return instance

        return self._container.get_service(service_type, **kwargs)
