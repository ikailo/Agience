from urllib.parse import urlencode, quote_plus
from models.entities import AuthorizationType, Authorizer as BaseAuthorizer

class Authorizer(BaseAuthorizer):
    def __init__(self):
        super().__init__()  # Call the base class initializer if needed

    def get_authorization_uri(self, authority_uri: str, state: str) -> str:
        """
        Constructs an authorization URI based on the authorization type.

        :param authority_uri: The base URI of the authority.
        :param state: A unique state string for authorization.
        :return: The constructed authorization URI, or None if `AuthorizationType` is None.
        """
        if self.auth_type == AuthorizationType.NONE:
            return None

        elif self.auth_type == AuthorizationType.OAUTH2:
            client_id = quote_plus(self.client_id)
            redirect_uri = quote_plus(f"{authority_uri}{self.redirect_uri}")
            scope = quote_plus(self.scope)

            return (
                f"{self.auth_uri}?"
                f"client_id={client_id}&redirect_uri={redirect_uri}&response_type=code&"
                f"scope={scope}&state={state}&prompt=consent&access_type=offline"
            )

            # NOTE: `access_type` and `prompt` are Google-specific. Adjust based on provider requirements.

        elif self.auth_type == AuthorizationType.API_KEY:
            return quote_plus(f"{authority_uri}/manage/authorizer/{self.id}/authorize?state={state}")

        else:
            raise ValueError("Unknown authorization type")
