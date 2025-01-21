import logging
from typing import List, Any, Optional
from semantic_kernel.kernel import Kernel

from core.models.enums.enums import PluginProvider
from core.models.entities.agent import Agent as AgentModel
from core.models.entities.plugin import Plugin

from core.agent import Agent
from core.authority import Authority
from core.broker import Broker

from core.services.agience_credential_service import AgienceCredentialService


class AgentFactory:
    # TODO: Class not implemented
    def __init__(
        self,
        service_provider: Any,
        broker: Broker,
        authority: Authority,
        logger: Optional[logging.Logger] = None
    ):
        self.service_provider = service_provider
        self.broker = broker
        self.authority = authority
        self.logger = logger
        self.agents: List[Agent] = []

    def create_agent(self, model_agent: AgentModel) -> Agent:
        logger = logging.getLogger(f"Agent {model_agent.name}:")
        kernel = Kernel()

        # TODO: add agent id to kernel
        # kernel.dict["agent_id"] = model_agent.id

        # Create credential service
        credential_service = AgienceCredentialService(
            model_agent.id,
            self.authority,
            self.broker
        )

        # Create agent
        agent = Agent(
            model_agent.id,
            model_agent.name,
            self.authority,
            self.broker,
            model_agent.persona,
            kernel,
            logger
        )

        self.agents.append(agent)

        # Initialize plugins
        self._initialize_plugins(agent, model_agent)

        # Add executive function if specified
        if model_agent.executive_function_id:
            self._add_executive_function(agent, model_agent)

        return agent

    def _initialize_plugins(self, agent: Agent, model_agent: AgentModel):
        for plugin in model_agent.plugins:
            if not plugin.name:
                self.logger.warning("Plugin name is empty.")
                continue

            try:
                if plugin.plugin_provider == PluginProvider.SKPlugin:
                    pass

                elif plugin.plugin_provider == PluginProvider.Prompt:
                    pass

            except Exception as ex:
                self.logger.error(f"Failed to initialize plugin: {
                                  plugin.name}", exc_info=ex)

    def _create_plugin_instance(self, plugin_name: str) -> Any:
        pass

    def _create_kernel_plugin_compiled(self, plugin_instance: Any, plugin_name: str) -> Any:
        pass

    def _create_kernel_plugin_prompt(self, plugin: Plugin) -> Any:
        pass

    def _add_executive_function(self, agent: Agent, model_agent: AgentModel):
        if not model_agent.executive_function_id:
            return
        pass

    def dispose_agent(self, agent_id: str):
        agent = next((a for a in self.agents if a.id == agent_id), None)
        if agent:
            self.agents.remove(agent)
            # agent.dispose()

    def dispose(self):
        for agent in self.agents[:]:
            self.dispose_agent(agent.id)
        self.agents.clear()
