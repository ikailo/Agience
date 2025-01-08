from typing import Optional, TypeVar, Type, Dict, Any
from pydantic import BaseModel
import inspect
import sys
from importlib import import_module

T = TypeVar('T', bound=BaseModel)


class Mapper:
    """
    A mapper class inspired by AutoMapper but implemented using Pydantic.
    """

    def __init__(self):
        self._mapping_configs: Dict[tuple[Type, Type], Any] = {}

    def create_map(self, source_type: Type, target_type: Type, custom_mappings: Optional[Dict[str, str]] = None):
        """Register a mapping between two types"""
        self._mapping_configs[(source_type, target_type)
                              ] = custom_mappings or {}

    def map(self, source: Any, target_type: Type[T]) -> T:
        """Map source object to target type"""
        if not isinstance(source, (BaseModel, dict)):
            raise ValueError("Source must be a Pydantic model or dictionary")

        # Convert source to dict if it's a Pydantic model
        source_dict = source.dict() if isinstance(source, BaseModel) else source

        # Get custom mappings for this type pair
        source_type = type(source) if not isinstance(source, dict) else dict
        custom_mappings = self._mapping_configs.get(
            (source_type, target_type), {})

        # Apply custom mappings
        mapped_dict = {}
        for target_field in target_type.__fields__:
            source_field = custom_mappings.get(target_field, target_field)
            if source_field in source_dict:
                mapped_dict[target_field] = source_dict[source_field]

        # Create target instance
        return target_type(**mapped_dict)


class AutoMapperConfig:
    _mapper: Optional[Mapper] = None

    @classmethod
    def get_mapper(cls) -> Mapper:
        """Get or create the mapper instance"""
        if cls._mapper is None:
            cls._mapper = Mapper()
            cls._load_mapping_profiles()
        return cls._mapper

    @classmethod
    def _load_mapping_profiles(cls):
        """Load all mapping profiles from the current package"""
        mapper = cls._mapper
        if not mapper:
            return

        # Get the current module's package
        current_module = sys.modules[__name__]
        package_name = current_module.__package__ or ''

        # Find all modules in the package
        for name, module in sys.modules.items():
            if name.startswith(package_name) and module:
                # Look for classes that end with 'MappingProfile'
                for _, obj in inspect.getmembers(module):
                    if (inspect.isclass(obj) and
                            obj.__name__.endswith('MappingProfile')):
                        profile = obj()
                        profile.create_maps(mapper)

# # Example mapping profile


# class MappingProfile:
#     def create_maps(self, mapper: Mapper):
#         # Example mapping configuration
#         # mapper.create_map(SourceModel, TargetModel, {'source_field': 'target_field'})
#         pass

# # Example usage


# class SourceModel(BaseModel):
#     name: str
#     age: int


# class TargetModel(BaseModel):
#     full_name: str
#     years: int


# class UserMappingProfile(MappingProfile):
#     def create_maps(self, mapper: Mapper):
#         mapper.create_map(SourceModel, TargetModel, {
#             'name': 'full_name',
#             'age': 'years'
#         })

# # Usage example


# def example_usage():
#     mapper = AutoMapperConfig.get_mapper()

#     source = SourceModel(name="John Doe", age=30)
#     target = mapper.map(source, TargetModel)

#     print(target.full_name)  # Output: John Doe
#     print(target.years)      # Output: 30
