from semantic_kernel.kernel import Kernel
from semantic_kernel.functions import KernelFunction
from typing import Dict, List, Optional, AsyncIterable, Any

from semantic_kernel.connectors.ai.prompt_execution_settings import PromptExecutionSettings
from semantic_kernel.contents import ChatMessageContent, StreamingChatMessageContent
from semantic_kernel.contents.chat_history import ChatHistory
from semantic_kernel.connectors.ai.chat_completion_client_base import ChatCompletionClientBase
from semantic_kernel.functions.kernel_arguments import KernelArguments


class AgienceChatCompletionService(ChatCompletionClientBase):
    def __init__(self, chat_completion_function: KernelFunction):
        self.chat_completion_function = chat_completion_function
        self.attributes: Dict[str, Any] = {}

    async def get_chat_message_contents(
        self,
        # TODO: this was optional in .net code (not a priority)
        kernel: Kernel,
        chat_history: ChatHistory,
        execution_settings: Optional[PromptExecutionSettings] = None,
        cancellation_token=None
    ) -> List[ChatMessageContent]:
        args = KernelArguments(
            chat_history=chat_history,
            execution_settings=execution_settings,
            agent_id=kernel.model_dump.__get__("agent_id") if kernel else None
        )

        result = await self.chat_completion_function.invoke(
            kernel=kernel,
            arguments=args,
            cancellation_token=cancellation_token
        )

        return result or []

    # TODO: cancellation_token type (not a priority)
    async def get_streaming_chat_message_contents(
        self,
        chat_history: ChatHistory,
        execution_settings: Optional[PromptExecutionSettings] = None,
        kernel: Optional[Kernel] = None,
        cancellation_token=None
    ) -> AsyncIterable[StreamingChatMessageContent]:
        raise NotImplementedError()
