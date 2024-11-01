namespace Agience.Core.Models.Entities
{
    public enum Visibility
    {
        Private = 0,
        Public = 1
    }

    public enum AuthorizationType
    {
        None = 0,
        OAuth2 = 1,
        ApiKey = 2
    }

    public enum PluginType
    {
        Curated = 0,
        Compiled = 1
    }

    public enum CompletionAction
    {
        Idle = 0,
        Restart = 1        
    }
}