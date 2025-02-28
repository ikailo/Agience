class TopicGenerator:
    EVENT_PREFIX = "event/"
    CONNECT_PREFIX = "connect/"

    def __init__(self, authority_id: str, sender_id: str):
        if authority_id is None:
            raise ValueError("authority_id cannot be None")
        if sender_id is None:
            raise ValueError("sender_id cannot be None")

        self._authority_id = authority_id
        self._sender_id = "-" if sender_id == authority_id else sender_id

    def connect_to(self, topic: str) -> str:
        return f"{self.CONNECT_PREFIX}{topic}"

    def subscribe_as(self, host_id: str | None, agent_id: str | None) -> str:
        return f"{self.EVENT_PREFIX}+/{self._authority_id}/{host_id or '-'}/{agent_id or '-'}"

    def publish_to(self, host_id: str | None, agent_id: str | None) -> str:
        return f"{self.EVENT_PREFIX}{self._sender_id}/{self._authority_id}/{host_id or '-'}/{agent_id or '-'}"

    def subscribe_as_agent(self) -> str:
        return self.subscribe_as(None, self._sender_id)

    def publish_to_agent(self, agent_id: str | None) -> str:
        return self.publish_to(None, agent_id)

    def publish_to_host(self, host_id: str) -> str:
        return self.publish_to(host_id, None)

    def subscribe_as_host(self) -> str:
        return self.subscribe_as(self._sender_id, None)

    def publish_to_authority(self) -> str:
        return self.publish_to(None, None)

    def subscribe_as_authority(self) -> str:
        return self.subscribe_as(None, None)
