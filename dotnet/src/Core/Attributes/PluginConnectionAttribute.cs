namespace Agience.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class PluginConnectionAttribute : Attribute
    {
        public string ConnectionName { get; }

        public PluginConnectionAttribute(string connectionName)
        {
            ConnectionName = connectionName;
        }
    }
}