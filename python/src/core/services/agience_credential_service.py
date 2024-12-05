from authority import Authority
from broker import Broker


class AgienceCredentialService:

    def __init__(self, agent_id: str, authority: Authority, broker: Broker):
        self._agent_id = agent_id
        self._authority = authority
        self._broker = broker
        self._credentials = {}

    def get_credential(self, name: str) -> str | None:
        if name in self._credentials:
            return self._credentials[name]

        # TODO: Get credential from Authority or Authorizer
        return None

    def add_credential(self, name: str, credential: str) -> None:
        self._credentials[name] = credential
