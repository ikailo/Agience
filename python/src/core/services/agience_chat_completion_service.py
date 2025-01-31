import asyncio
from typing import Dict, List, Optional, AsyncIterable, Any

from semantic_kernel.kernel import Kernel
from semantic_kernel.functions import KernelFunction
from semantic_kernel.connectors.ai.prompt_execution_settings import PromptExecutionSettings
from semantic_kernel.contents import ChatMessageContent, StreamingChatMessageContent
from semantic_kernel.contents.chat_history import ChatHistory
from semantic_kernel.connectors.ai.chat_completion_client_base import ChatCompletionClientBase
from semantic_kernel.functions.kernel_arguments import KernelArguments


class AgienceChatCompletionService(ChatCompletionClientBase):

    chat_completion_function: KernelFunction
    attributes: Dict[str, Any] = {}

    class Config:
        arbitrary_types_allowed = True

    async def get_chat_message_contents(
        self,
        kernel: Kernel,
        chat_history: ChatHistory,
        execution_settings: Optional[PromptExecutionSettings] = None,
        cancellation_token: Optional[asyncio.Event] = None,
        **kwargs,
    ) -> List[ChatMessageContent]:
        args = KernelArguments(
            settings=execution_settings,
            chat_history=chat_history,
            # TODO: Text agent_id
            agent_id=getattr(kernel, "agent_id", None) if kernel else None
        )

        result = await self.chat_completion_function.invoke(
            kernel=kernel,
            arguments=args,
            cancellation_token=cancellation_token
        )

        return result or []

    async def get_streaming_chat_message_contents(
        self,
        chat_history: ChatHistory,
        execution_settings: Optional[PromptExecutionSettings] = None,
        kernel: Optional[Kernel] = None,
        cancellation_token=Optional[asyncio.Event]
    ) -> AsyncIterable[StreamingChatMessageContent]:
        raise NotImplementedError()
