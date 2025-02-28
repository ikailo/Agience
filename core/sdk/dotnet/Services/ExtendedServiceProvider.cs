using Microsoft.Extensions.DependencyInjection;


namespace Agience.Core.Services
{
    public class ExtendedServiceProvider : IServiceProvider
    {
        public IServiceCollection Services => _collection;

        private readonly IServiceProvider _provider;
        private readonly IServiceCollection _collection = new ServiceCollection();

        public ExtendedServiceProvider(IServiceProvider provider)
        {
            _provider = provider;
            _collection.AddScoped<IServiceProvider>(sp => this);
        }

        public object? GetService(Type serviceType)
        {
            return _collection.BuildServiceProvider().GetService(serviceType) ?? _provider.GetService(serviceType);
        }        
    }
}
