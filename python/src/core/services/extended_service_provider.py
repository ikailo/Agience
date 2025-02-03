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

    def build_local_provider(self):
        return self._collection.build()

    # def get_service(self, service_type: Type) -> Any:
    #     if not self._local_provider:
    #         self._local_provider = self._collection.build()

    #     try:
    #         return self._local_provider.get_service(service_type)
    #     except ServiceNotFoundError:
    #         return self._provider.get_service(service_type)

    def get_service(self, service_type: Type) -> Any:
        try:
            return self.build_local_provider().get_service(service_type)
        except ServiceNotFoundError:
            return self._provider.get_service(service_type)

    def get_required_service(self, service_type: Type) -> Any:
        service = self.get_service(service_type)
        if service is None:
            raise ServiceNotFoundError(
                f"Required service {service_type.__name__} not found")
        return service
