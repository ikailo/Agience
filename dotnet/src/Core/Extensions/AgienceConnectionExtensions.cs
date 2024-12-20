using Agience.Core.Attributes;
using System.Reflection;

namespace Agience.Core.Extensions
{
    public static class AgienceConnectionExtensions
    {
        public static string GetAgienceConnectionName(this object instance)
        {
            var method = instance.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.GetCustomAttributes(typeof(AgienceConnectionAttribute), false).Any());

            if (method == null)
                throw new InvalidOperationException("No method with the AgienceConnection attribute found.");

            var attribute = method.GetCustomAttribute<AgienceConnectionAttribute>();
            return attribute?.Name ?? throw new InvalidOperationException("AgienceConnection attribute is missing a name.");
        }
    }

}
