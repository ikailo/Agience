from typing import Optional, Any
from logging import Logger
import asyncio

from semantic_kernel import Kernel
from semantic_kernel.connectors.ai.prompt_execution_settings import PromptExecutionSettings
from semantic_kernel.contents.chat_history import ChatHistory
from semantic_kernel.connectors.ai.chat_completion_client_base import ChatCompletionClientBase

from core.models.entities.agent import Agent as AgentModel
from core.models.messages.broker_message import BrokerMessage, BrokerMessageType
from core.authority import Authority
from core.broker import Broker
from core.topic_generator import TopicGenerator

from core.services.agience_chat_completion_service import AgienceChatCompletionService
from core.services.agience_credential_service import AgienceCredentialService
from core.services.extended_service_provider import ExtendedServiceProvider


class Agent(AgentModel):
    def __init__(
        self,
        id: str,
        name: str,
        authority: 'Authority',
        broker: 'Broker',
        persona: str,
        kernel: Kernel,
        logger: Logger,
        service_provider: ExtendedServiceProvider,
        **data: Any
    ):
        super().__init__(
            id=id,
            name=name,
            persona=persona,
            **data
        )

        # Private attributes
        self._authority = authority
        self._broker = broker
        self._kernel = kernel
        self._logger = logger
        self._disposed = False
        self._is_connected = False
        self._chat_history = ChatHistory()
        self._topic_generator = TopicGenerator(authority.id, id)

        self._service_provider = service_provider

        self._prompt_execution_settings = PromptExecutionSettings(
            extension_data={
                "model": "gpt-4",
                "temperature": 0.7,
                "max_tokens": 2000,
                "tool_call_behavior": "auto",
            }
        )

    @property
    def is_connected(self) -> bool:
        return self._is_connected

    @property
    def chat_history(self) -> ChatHistory:
        return self._chat_history

    # self._broker_receive_message depends on AgienceCredentialService (not implemented yet)
    async def connect(self) -> None:
        if not self.is_enabled:
            self._logger.warning(f"Agent {self.id} is not enabled")
            return

        if not self._is_connected:
            try:
                # Subscribe to agent's topic
                await self._broker.subscribe(
                    self._topic_generator.subscribe_as_agent(),
                    self._broker_receive_message
                )

                # Subscribe to each topic
                for topic in self.topics:
                    await self._broker.subscribe(
                        self._topic_generator.connect_to(topic.name),
                        self._broker_receive_message
                    )

                self._is_connected = True
                self._logger.info(f"Agent {self.id} connected successfully")
            except Exception as e:
                self._logger.error(f"Failed to connect agent {
                                   self.id}: {str(e)}")
                raise

    async def _broker_receive_message(self, message: BrokerMessage) -> None:
        try:
            # Handle incoming credential
            if (message.type == BrokerMessageType.EVENT
                and message.data
                and message.data.get("type") == "credential_response"
                and message.data.get("credential_name")
                    and message.data.get("encrypted_credential")):

                name = message.data["credential_name"]
                credential = message.data["encrypted_credential"]

                credential_service: AgienceCredentialService = await self._service_provider.get_service(AgienceCredentialService)
                await credential_service.add_encrypted_credential(name, credential)
        except Exception as e:
            self._logger.error(f"Error processing broker message: {str(e)}")
            raise

    async def disconnect(self) -> None:
        """Disconnect the agent from its topics"""
        if self._is_connected:
            try:
                await self._broker.unsubscribe(self._topic_generator.subscribe_as_agent())
                self._is_connected = False
                self._logger.info(f"Agent {self.id} disconnected successfully")
            except Exception as e:
                self._logger.error(f"Failed to disconnect agent {
                                   self.id}: {str(e)}")
                raise

    async def prompt_async(
        self,
        user_message: str,
        cancellation_token: Optional[asyncio.Event] = None
    ) -> Optional[str]:
        try:
            # Add the user's message to the chat history
            self._chat_history.add_user_message(user_message)

            chat_completion: ChatCompletionClientBase = self._service_provider.get_required_service(
                ChatCompletionClientBase
            )

            # Get the response from the chat completion service
            result = await chat_completion.get_chat_message_contents(
                chat_history=self._chat_history,
                execution_settings=self._prompt_execution_settings,
                kernel=self._kernel,
                cancellation_token=cancellation_token
            )

            if result and result[0]:
                assistant_message = str(result[0].content)
                self._chat_history.add_assistant_message(assistant_message)
                return assistant_message

            return None

        except Exception as e:
            self._logger.error(f"Error in prompt_async: {
                               str(e)}", exc_info=True)
            raise

    # TODO: Cleanup properly (not priority)
    def __del__(self):
        """Cleanup when the agent is destroyed"""
        if not self._disposed:
            if hasattr(self._logger, 'dispose'):
                self._logger.dispose()
            self._disposed = True
