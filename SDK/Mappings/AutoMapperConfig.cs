using AutoMapper;
using System.Reflection;

namespace Agience.SDK.Mappings
{
    internal static class AutoMapperConfig
    {
        private static IMapper? _mapper;

        // TODO: Determine if it makes sense to load this via DI

        internal static IMapper GetMapper()
        {
            if (_mapper == null)
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddMaps(Assembly.GetExecutingAssembly());
                    //cfg.AddProfile<MappingProfile>();
                });
                _mapper = config.CreateMapper();
            }

            return _mapper;
        }
    }
}
