from typing import Optional
from core.agent import Agent


class IInteractionService:
    async def send_to_agent(self, receiver: Agent) -> Optional[str]:
        pass

    async def receive_from_agent(self, sender: Agent, message: str) -> None:
        pass

    async def start(self) -> None:
        pass
