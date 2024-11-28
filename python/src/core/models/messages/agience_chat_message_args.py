class AgienceChatMessageArgs:
    def __init__(self, agency_id: str, message: 'ChatMessageContent', agent_id: str):
        self.agency_id = agency_id
        self.message = message
        self.agent_id = agent_id
