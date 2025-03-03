from enum import Enum


class Visibility(Enum):
    Private = 0
    Public = 1


class AuthorizationType(Enum):
    Public = 0
    OAuth2 = 1
    ApiKey = 2


class PluginProvider(Enum):
    Prompt = 0
    SKPlugin = 1
    Collection = 2


class PluginSource(Enum):
    UserDefined = 0
    HostDefined = 1
    UploadPackage = 2
    PublicRepository = 3


class CompletionAction(Enum):
    Idle = 0
    Restart = 1


class CredentialStatus(Enum):
    NoAuthorizer = 0
    NoCredential = 1
    Complete = 2
    Authorized = 3
