namespace Agience.Core.Models.Enums
{
    public enum Visibility
    {
        Private,
        Public
    }

    public enum AuthorizationType
    {
        Public,
        OAuth2,
        ApiKey
    }

    public enum PluginProvider
    {
        Prompt,        
        SKPlugin,
        Collection
    }

    public enum PluginSource
    {
        UserDefined,
        HostDefined,
        UploadPackage,
        PublicRepository        
    }

    public enum CompletionAction
    {
        Idle,
        Restart
    }

    public enum CredentialStatus
    {
        NoAuthorizer,
        NoCredential,
        Complete,
        Authorized
    }
}