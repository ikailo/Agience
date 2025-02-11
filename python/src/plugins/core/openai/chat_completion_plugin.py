from typing import Optional, List
from pydantic import BaseModel, Field

from core.services.agience_credential_service import AgienceCredentialService
from core.attributes.agience_connection_attribute import AgienceConnection

from semantic_kernel import Kernel
from semantic_kernel.contents.chat_history import ChatHistory
from semantic_kernel.connectors.ai.open_ai import OpenAIPromptExecutionSettings
from semantic_kernel.connectors.ai.open_ai import OpenAIChatCompletion
from semantic_kernel.functions import kernel_function


class ChatCompletionPlugin:
    def __init__(self, credential_service: AgienceCredentialService, kernel: Kernel):
        self._credential_service = credential_service
        self._kernel = kernel

    @AgienceConnection("OpenAI")
    @kernel_function(
        description="Get multiple chat content choices for the prompt and settings.",
        name="get_chat_message_contents"
    )
    async def get_chat_message_contents_async(
        self,
        chat_history: ChatHistory,
        execution_settings: Optional[OpenAIPromptExecutionSettings] = None,
        agent_id: Optional[str] = None
    ):
        connection_name: str = self.get_chat_message_contents_async.connection_name
        if not connection_name:
            raise ValueError("Connection name is missing.")

        api_key = await self._credential_service.get_credential(connection_name)
        if not api_key:
            raise ValueError("OpenAI API Key is missing.")

        chat_completion_service = OpenAIChatCompletion(
            ai_model_id="gpt-3.5-turbo",
            api_key=api_key,
        )

        raw_responses = await chat_completion_service.get_chat_message_contents(
            chat_history=chat_history,
            settings=execution_settings
        )

        return raw_responses
