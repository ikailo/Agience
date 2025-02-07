import logging
from typing import List, Any, Optional, Type
from semantic_kernel.kernel import Kernel
from semantic_kernel.functions.kernel_plugin import KernelPlugin
from semantic_kernel.functions.kernel_function import KernelFunction
from semantic_kernel.functions.kernel_function_from_prompt import KernelFunctionFromPrompt
from semantic_kernel.connectors.ai.chat_completion_client_base import ChatCompletionClientBase

from core.models.enums.enums import PluginProvider
from core.models.entities.agent import Agent as AgentModel
from core.models.entities.plugin import Plugin

from core.agent import Agent
from core.authority import Authority
from core.broker import Broker
from core.host import Host

from core.services.agience_chat_completion_service import AgienceChatCompletionService
from core.services.agience_credential_service import AgienceCredentialService
from core.services.extended_service_provider import ExtendedServiceProvider

from utils.service_container import ServiceProvider


class AgentFactory:
    def __init__(
        self,
        main_service_provider: ServiceProvider,
        broker: Broker,
        authority: Authority,
        logger: Optional[logging.Logger] = None
    ):
        self.service_provider = main_service_provider.create_scope()
        self.broker = broker
        self.authority = authority
        self.logger = logger
        self.agents: List[Agent] = []

    def create_agent(self, model_agent: AgentModel) -> Agent:

        agent_service_provider = ExtendedServiceProvider(
            self.service_provider
        )
        host = agent_service_provider.get_required_service(Host)

        self._configure_kernel_services(agent_service_provider, model_agent)

        agent_plugins: list[KernelPlugin] = []
        kernel = Kernel(plugins=agent_plugins, agent_id=model_agent.id)

        agent_service_provider.services.add_singleton_factory(
            Kernel, lambda _: kernel)

        self._add_executive_function(
            host, model_agent, agent_service_provider)

        # Create agent
        agent = Agent(
            model_agent.id,
            model_agent.name,
            self.authority,
            self.broker,
            model_agent.persona,
            kernel,
            service_provider=agent_service_provider,
            logger=logging.getLogger(f"Agent {model_agent.name}:"),
        )

        agent_service_provider.services.add_singleton(lambda _: agent)
        self.agents.append(agent)

        # TODO: Fix later
        # self._initialize_plugins(
        #     host=host,
        #     model_agent=model_agent,
        #     agent_plugins=agent_plugins,
        #     service_provider=kernel_service_provider
        # )

        return agent

    def _configure_kernel_services(
        self,
        agent_service_provider: ExtendedServiceProvider,
        model_agent: 'AgentModel'
    ) -> None:
        agent_service_provider.services.add_singleton_factory(
            AgienceCredentialService,
            lambda _: AgienceCredentialService(
                model_agent.id,
                self.authority,
                self.broker
            )
        )

    def _initialize_plugins(self, host: 'Host', agent_plugins: list[KernelPlugin], model_agent: AgentModel, service_provider: ExtendedServiceProvider):
        for plugin in model_agent.plugins:
            if not plugin.name:
                self.logger.warning("Plugin name is empty.")
                continue

            try:
                if plugin.plugin_provider == PluginProvider.SKPlugin:
                    plugin_type = next(
                        (p.type for p in host.plugins if p.name == plugin.name),
                        None
                    )

                    if plugin_type:
                        plugin_name = getattr(
                            plugin_type, 'full_name', None) or plugin_type.__name__

                        if plugin_name in host.plugin_instances:
                            plugin_instance = host.plugin_instances[plugin_name]
                            kernel_plugin = self._create_kernel_plugin_compiled(
                                plugin_instance,
                                plugin.name
                            )
                            agent_plugins.append(kernel_plugin)
                        else:
                            kernel_plugin = self._create_kernel_plugin_compiled_instance(
                                service_provider,
                                plugin_type,
                                plugin.name
                            )
                            agent_plugins.append(kernel_plugin)

                elif plugin.plugin_provider == PluginProvider.Prompt:
                    prompt_plugin = self._create_kernel_plugin_prompt(plugin)
                    agent_plugins.append(prompt_plugin)

            except Exception as ex:
                self.logger.error(f"Failed to initialize plugin: {
                                  plugin.name}", exc_info=ex)

    def _add_executive_function(self, host: 'Host', model_agent: AgentModel, service_provider: ExtendedServiceProvider):
        if not model_agent.executive_function_id:
            return

        executive_function_name = ""
        factory = None

        for host_plugin in host.plugins:
            if not any(f.id == model_agent.executive_function_id for f in host_plugin.functions):
                continue

            if host_plugin.plugin_provider == PluginProvider.SKPlugin and host_plugin.type is not None:
                plugin_type = host_plugin.type
                plugin_name = host_plugin.name

                # Find the function and remove 'Async' from its name
                matching_function = next(
                    (f for f in host_plugin.functions if f.id ==
                     model_agent.executive_function_id),
                    None
                )
                executive_function_name = matching_function.name if matching_function else ""

                executive_function_name = executive_function_name.replace(
                    "Async", "").replace("_async", "") if matching_function else ""

                def create_compiled_factory(sp: ExtendedServiceProvider) -> ChatCompletionClientBase:
                    kernel_plugin = self._create_kernel_plugin_compiled_instance(
                        service_provider=sp,
                        plugin_type=plugin_type,
                        plugin_name=plugin_name
                    )

                    executive_function = kernel_plugin.functions.get(
                        executive_function_name)

                    if executive_function:
                        return AgienceChatCompletionService(chat_completion_function=executive_function, ai_model_id=model_agent.id)
                    raise ValueError(f"Executive function '{
                                     executive_function_name}' not found")

                factory = create_compiled_factory

            elif host_plugin.plugin_provider == PluginProvider.Prompt:
                kernel_plugin = self._create_kernel_plugin_prompt(host_plugin)

                matching_function = next(
                    (f for f in host_plugin.functions if f.id ==
                     model_agent.executive_function_id),
                    None
                )

                # not needed (or need further discussion)
                executive_function_name = matching_function.name.replace(
                    "Async", "").replace("async", "") if matching_function else ""

                def create_prompt_factory(sp: ServiceProvider) -> ChatCompletionClientBase:
                    if kernel_plugin.functions.get(executive_function_name):
                        return AgienceChatCompletionService(executive_function_name)
                    raise ValueError(f"Executive function '{
                                     executive_function_name}' not found")

                factory = create_prompt_factory

        if factory is not None:
            service_provider.services.add_singleton_factory(
                ChatCompletionClientBase, lambda _: factory(service_provider)
            )

        else:
            self.logger.warning(f"Could not find a plugin with the executive function id {
                model_agent.executive_function_id}")

    def _create_kernel_plugin_compiled_instance(
        self,
        service_provider: ExtendedServiceProvider,
        plugin_type: Type,
        plugin_name: str
    ) -> 'KernelPlugin':

        def construct_plugin(sp: ExtendedServiceProvider) -> Any:
            # Get constructor parameters
            import inspect
            sig = inspect.signature(plugin_type.__init__)
            params = sig.parameters

            plugin_instance = plugin_type(**{
                name: sp.get_required_service(param.annotation)
                for name, param in params.items()
                if name != 'self'  # Skip 'self' parameter
            })

            return plugin_instance

        service_provider.services.add_singleton_factory(
            plugin_type,
            lambda _: construct_plugin(service_provider)
        )

        plugin_instance = service_provider.get_required_service(plugin_type)
        return KernelPlugin.from_object(plugin_name=plugin_name, plugin_instance=plugin_instance)

    # TODO: Fix this
    def _create_kernel_plugin_compiled(self, plugin_instance: Any, plugin_name: str) -> KernelPlugin:
        return KernelPlugin.from_object(plugin_name=plugin_name, plugin_instance=plugin_instance)

    def _create_kernel_plugin_prompt(self, plugin: Plugin) -> KernelPlugin:
        functions = [
            KernelFunction.from_prompt(
                function_name=func.name,
                plugin_name=plugin.name,
                description=func.description,
                prompt=func.instruction,
            ) for func in plugin.functions
        ]

        return KernelPlugin(name=plugin.name, description=plugin.description, functions=functions)

    def dispose_agent(self, agent_id: str):
        agent = next((a for a in self.agents if a.id == agent_id), None)
        if agent:
            self.agents.remove(agent)
            agent.disconnect()

    def dispose(self):
        for agent in self.agents[:]:
            self.dispose_agent(agent.id)
        self.agents.clear()
