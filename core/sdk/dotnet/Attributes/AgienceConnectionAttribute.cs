namespace Agience.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class AgienceConnectionAttribute : Attribute
    {
        public string Name { get; }

        public AgienceConnectionAttribute(string name)
        {
            Name = name;
        }
    }

}