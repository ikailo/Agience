from semantic_kernel.functions import kernel_function

from core.agent import Agent
from utils.service_container import ServiceProvider
from plugins.core.interaction.interaction_service_interface import IInteractionService


class InteractionPlugin:
    def __init__(self, interaction_service: IInteractionService, service_provider: ServiceProvider):
        self._interaction = interaction_service
        self._service_provider = service_provider

    @kernel_function(description="Show a message")
    async def send(self, message: str) -> None:
        agent = self._service_provider.get_required_service(Agent)
        await self._interaction.receive_from_agent(agent, message)

    @kernel_function(description="Receive a message")
    async def receive(self) -> str:
        agent = self._service_provider.get_required_service(Agent)
        result = await self._interaction.send_to_agent(agent)
        return result if result is not None else ""

    @kernel_function(description="Show a message and collect a response")
    async def interact(self, message: str) -> str:
        await self.send(message)
        return await self.receive()
