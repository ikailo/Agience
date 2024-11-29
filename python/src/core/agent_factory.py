import asyncio
from typing import Dict, List, Type, Optional
from dataclasses import dataclass, field
import logging

# Import dependencies (these must match your project structure)
from models.entities import Agent, Agency, KernelPlugin, KernelFunction
from broker import Broker
from authority import Authority

logger = logging.getLogger(__name__)


@dataclass
class AgentFactory:
    authority: Authority
    broker: Broker
    logger: logging.Logger
    host_openai_api_key: Optional[str] = None

    host_plugins_compiled: Dict[str, Type] = field(default_factory=dict)
    host_plugins_curated: Dict[str, KernelPlugin] = field(default_factory=dict)
    agencies: Dict[str, Agency] = field(default_factory=dict)
    agents: List[Agent] = field(default_factory=list)

    def add_host_plugin_from_type(self, plugin_name: str, plugin_type: Type):
        """Add a compiled host plugin."""
        if plugin_name in self.host_plugins_compiled:
            logger.warning(f"Plugin {plugin_name} is already added.")
        else:
            self.host_plugins_compiled[plugin_name] = plugin_type

    def add_host_plugin(self, plugin: dict):
        """Add a plugin by its definition."""
        plugin_name = plugin.get("name")
        plugin_type = plugin.get("type")
        plugin_functions = plugin.get("functions", [])

        if not plugin_name:
            logger.warning("Plugin name is empty. Plugin will not be loaded.")
            return

        if plugin_name in self.host_plugins_compiled or plugin_name in self.host_plugins_curated:
            logger.warning(f"Plugin {plugin_name} is already loaded.")
            return

        if plugin_type == "compiled":
            plugin_class = self._get_compiled_plugin_class(plugin_name)
            if plugin_class:
                self.host_plugins_compiled[plugin_name] = plugin_class
        elif plugin_type == "curated":
            kernel_plugin = self._create_kernel_plugin_curated(plugin_name, plugin_functions)
            self.host_plugins_curated[plugin_name] = kernel_plugin

    def _get_compiled_plugin_class(self, plugin_name: str) -> Optional[Type]:
        """Retrieve the compiled plugin class dynamically."""
        try:
            module_name, class_name = plugin_name.rsplit(".", 1)
            module = __import__(module_name, fromlist=[class_name])
            return getattr(module, class_name)
        except (ImportError, AttributeError) as e:
            logger.error(f"Failed to load plugin class {plugin_name}: {e}")
            return None

    def create_agent(self, model_agent: dict) -> Agent:
        """Creates an agent with associated plugins."""
        agent_plugins = []

        # Prepare plugins for the agent
        for plugin in model_agent.get("plugins", []):
            plugin_name = plugin.get("name")
            plugin_type = plugin.get("type")

            if not plugin_name:
                logger.warning("Plugin name is empty.")
                continue

            if plugin_type == "compiled" and plugin_name in self.host_plugins_compiled:
                plugin_class = self.host_plugins_compiled[plugin_name]
                agent_plugins.append(self._create_kernel_plugin_compiled(plugin_name, plugin_class))
            elif plugin_type == "curated" and plugin_name in self.host_plugins_curated:
                agent_plugins.append(self.host_plugins_curated[plugin_name])
            elif plugin_type == "curated":
                # Load curated plugin if it's not found in the preloaded list
                agent_plugins.append(self._create_kernel_plugin_curated(plugin_name, plugin.get("functions", [])))

        # Create the Agent instance
        agency = self.get_agency(model_agent["agency"])
        agent = Agent(
            id=model_agent["id"],
            name=model_agent["name"],
            authority=self.authority,
            broker=self.broker,
            agency=agency,
            persona=model_agent["persona"],
            plugins=agent_plugins,
        )

        agency.add_local_agent(agent)
        self.agents.append(agent)
        return agent

    def dispose_agent(self, agent_id: str):
        """Dispose of an agent by ID."""
        agent = next((a for a in self.agents if a.id == agent_id), None)
        if agent:
            self.agents.remove(agent)
            agent.dispose()

    def dispose(self):
        """Dispose of all agents."""
        agents_to_remove = list(self.agents)
        for agent in agents_to_remove:
            self.dispose_agent(agent.id)

    def _create_kernel_plugin_compiled(self, plugin_name: str, plugin_class: Type) -> KernelPlugin:
        """Creates a compiled kernel plugin."""
        plugin_instance = plugin_class()  # Assume the plugin class can be instantiated directly
        return KernelPlugin.from_object(plugin_instance, plugin_name)

    def _create_kernel_plugin_curated(self, plugin_name: str, functions: List[dict]) -> KernelPlugin:
        """Creates a curated kernel plugin."""
        kernel_functions = [
            KernelFunction.create_from_prompt(
                name=f["name"],
                description=f.get("description", ""),
                prompt=f["prompt"],
            )
            for f in functions
        ]
        return KernelPlugin.from_functions(plugin_name, kernel_functions)

    def get_agency(self, model_agency: dict) -> Agency:
        """Retrieve or create an agency."""
        agency_id = model_agency["id"]
        if agency_id not in self.agencies:
            agency = Agency(
                id=agency_id,
                name=model_agency["name"],
                authority=self.authority,
                broker=self.broker,
            )
            self.agencies[agency_id] = agency
        return self.agencies[agency_id]
