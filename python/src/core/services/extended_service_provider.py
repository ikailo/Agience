from typing import Type, Any
from utils.service_container import ServiceCollection, ServiceProvider, ServiceNotFoundError


class ExtendedServiceProvider:
    def __init__(self, provider: 'ServiceProvider'):
        self._provider = provider
        self._collection = ServiceCollection()
        self._collection.add_scoped('ServiceProvider', lambda sp: self)

    @property
    def services(self) -> ServiceCollection:
        return self._collection

    def get_service(self, service_type: Type) -> Any:
        local_provider = self._collection.build()
        try:
            return local_provider.get_service(service_type) or self._provider.get_service(service_type)
        except ServiceNotFoundError:
            return self._provider.get_service(service_type)
