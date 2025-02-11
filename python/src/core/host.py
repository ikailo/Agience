from typing import Dict, Optional, Callable, Any, TYPE_CHECKING
from pydantic import BaseModel, Field, field_serializer
import asyncio
import base64
import logging
import httpx
import json
import inspect

from semantic_kernel.functions.kernel_function import KernelFunction

from core.models.entities.host import Host as HostModel
from core.models.entities.agent import Agent as AgentModel
from core.models.entities.plugin import Plugin as PluginModel
from core.models.entities.function import Function as FunctionModel
from core.models.entities.parameter import Parameter as ParameterModel
from core.models.enums.enums import PluginProvider, PluginSource

from core.authority import Authority
from core.broker import Broker, BrokerMessage, BrokerMessageType
from core.agent import Agent
from core.topic_generator import TopicGenerator
from core.data import Data

if TYPE_CHECKING:
    from core.agent_factory import AgentFactory


class TokenResponse(BaseModel):
    access_token: Optional[str] = None
    token_type: Optional[str] = None
    expires_in: Optional[int] = None


class Host(HostModel):
    id: str
    name: Optional[str] = None
    description: Optional[str] = None
    properties: Dict[str, Any] = Field(default_factory=dict)

    # Non-model attributes
    is_connected: bool = False
    agents: Dict[str, Agent] = Field(default_factory=dict)
    plugin_instances: Dict[str, Any] = Field(default_factory=dict)

    # Event callbacks
    agent_connected: Optional[Callable[[Agent], None]] = Field(
        default=None, exclude=True)
    agent_disconnected: Optional[Callable[[str], None]] = Field(
        default=None, exclude=True)
    agent_log_entry_received: Optional[Callable[[str, str], None]] = Field(
        default=None, exclude=True)

    def __init__(self, host_id: str, host_secret: str, authority: Authority,
                 broker: Broker, agent_factory: 'AgentFactory', logger: Optional[logging.Logger] = None, **data):
        super().__init__(id=host_id, **data)

        if not host_id:
            raise ValueError("host_id cannot be empty")
        if not host_secret:
            raise ValueError("host_secret cannot be empty")

        self._host_secret = host_secret
        self._authority = authority
        self._broker = broker
        self._agent_factory = agent_factory
        self._logger = logger or logging.getLogger(__name__)
        self._topic_generator = TopicGenerator(authority.id, host_id)

    @field_serializer('agents', when_used='json')
    def serialize_agents(self, agents: Dict[str, Agent], _info):
        return list(agents.values())

    async def run(self):
        await self.start()

        while self.is_connected:
            await asyncio.sleep(0.1)

    async def stop(self):
        self._logger.info("Stopping Host")
        await self.disconnect()

    async def start(self):
        self._logger.info("Starting Host")

        while not self.is_connected:
            try:
                await self.connect()
            except Exception as ex:
                self._logger.error("Unable to Connect", exc_info=ex)
                self._logger.info("Retrying in 10 seconds")
                await asyncio.sleep(10)  # TODO: Implement backoff

    async def connect(self):
        self._logger.info("Connecting Host")

        await self._authority.initialize_with_backoff()

        if not self._authority._broker_uri:
            raise ValueError("BrokerUri not set")

        access_token = await self._get_access_token()

        if not access_token:
            raise ValueError("Failed to get access token")

        await self._broker.connect(access_token, self._authority._broker_uri)

        if self._broker.is_connected:
            await self._broker.subscribe(
                self._topic_generator.subscribe_as_host(),
                self._broker_receive_message
            )

            data = Data()
            data.add("type", "host_connect")
            data.add("timestamp", self._broker.timestamp)
            data.add("host", self.model_dump_json())

            broker_message = BrokerMessage(
                type=BrokerMessageType.EVENT,
                topic=self._topic_generator.publish_to_authority(),
                data=data
            )

            # await self._broker.subscribe(broker_message.topic, self._broker_receive_message)

            await self._broker.publish_async(broker_message)

            self.is_connected = True
            self._logger.info("Host Connected")
        else:
            raise Exception("Broker Connection Failed")

    async def disconnect(self):
        if self.is_connected:
            for agent in self.agents.values():
                await agent.disconnect()

            await self._broker.unsubscribe(self._topic_generator.subscribe_as_host())
            await self._broker.disconnect()

            self.is_connected = False

    async def _get_access_token(self) -> Optional[str]:
        if not self._authority.token_endpoint:
            raise ValueError("Token endpoint not set")

        auth_string = f"{self.id}:{self._host_secret}"
        basic_auth = base64.b64encode(auth_string.encode()).decode()

        # TODO: Verification turned off for development
        async with httpx.AsyncClient(verify=False) as client:
            headers = {"Authorization": f"Basic {basic_auth}"}
            data = {
                "grant_type": "client_credentials",
                "scope": "connect"
            }

            response = await client.post(
                self._authority.token_endpoint,
                headers=headers,
                data=data
            )

            if response.status_code == 200:
                token_response = TokenResponse.parse_raw(response.text)
                return token_response.access_token

            return None

    async def receive_host_welcome(self, host: 'HostModel', plugins: list['PluginModel'], agents: list['AgentModel']):
        # Reconcile plugins
        for plugin in plugins:
            # Find existing plugin by unique name or name
            existing_plugin = next(
                (p for p in self.plugins if p.unique_name == plugin.unique_name),
                next(
                    (p for p in self.plugins if p.name == plugin.name),
                    None
                )
            )

            if existing_plugin:
                # Update Plugin ID
                existing_plugin.id = plugin.id

                # Reconcile functions
                for function in plugin.functions:
                    existing_function = next(
                        (f for f in existing_plugin.functions if f.name == function.name),
                        None
                    )
                    if existing_function:
                        # Update Function ID
                        existing_function.id = function.id
                    # Note: Adding new functions is commented out in original
                    # else:
                    #     existing_plugin.functions.append(function)

                # Note: Removing extra functions is commented out in original
                # function_names = {f.name for f in plugin.functions}
                # existing_plugin.functions = [f for f in existing_plugin.functions if f.name in function_names]
            else:
                # Add the entire plugin
                self.plugins.append(plugin)

        # Connect all agents
        for agent in agents:
            await self.receive_agent_connect(agent)

    async def receive_agent_connect(self, model_agent: 'Agent'):
        # Create and configure agent
        agent = self._agent_factory.create_agent(model_agent)

        # Connect the agent
        self.agents[agent.id] = agent
        await agent.connect()

        self._logger.info(f"{agent.name} Connected")

        # Notify listeners if callback is set
        if self.agent_connected:
            await self.agent_connected(agent)

    async def receive_agent_disconnect(self, agent_id: str):
        agent = self.agents[agent_id]

        await agent.disconnect()

        self._logger.info(f"{agent.name} Disconnected")

        # Notify listeners if callback is set
        if self.agent_disconnected:
            await self.agent_disconnected(agent_id)

        self._agent_factory.dispose_agent(agent_id)

    async def _broker_receive_message(self, message: BrokerMessage):
        self._logger.info(f"Received message: {message.topic}")

        # self._logger.info(f"Received message: {message}")
        if not message.sender_id or not message.data:
            return

        # Incoming Host Welcome Message
        if (message.type == BrokerMessageType.EVENT and
            message.data.get("type") == "host_welcome" and
                message.data.get("host")):

            try:
                host_json = json.loads(message.data["host"])
                plugins_list_json = json.loads(
                    message.data.get("plugins", "[]")
                )
                agents_list_json = json.loads(message.data.get("agents", "[]"))

                host = HostModel.model_validate(host_json)
                plugins_list = [
                    PluginModel.model_validate(p) for p in plugins_list_json
                ]
                agents_list = [
                    AgentModel.model_validate(a) for a in agents_list_json
                ]

                if not host.id:
                    self._logger.error("Invalid Host")
                else:
                    self._logger.info(
                        f"Received Host Welcome Message for {host.name}")
                    await self.receive_host_welcome(host, plugins_list, agents_list)

            except json.JSONDecodeError as e:
                self._logger.error(
                    f"Failed to parse host welcome message: {e}")

        # Incoming Agent Connect Message
        elif (message.type == BrokerMessageType.EVENT and
              message.data.get("type") == "agent_connect" and
              message.data.get("agent")):

            try:
                agent_data = json.loads(message.data["agent"])
                agent = Agent.parse_obj(agent_data)

                self._logger.info(f"ReceiveAgentConnect for {agent.id}")

                if not agent.id:
                    self._logger.error("Invalid Agent")
                    return

                await self.receive_agent_connect(agent)

            except json.JSONDecodeError as e:
                self._logger.error(
                    f"Failed to parse agent connect message: {e}")

        # Incoming Agent Disconnect Message
        elif (message.type == BrokerMessageType.EVENT and
              message.data.get("type") == "agent_disconnect" and
              message.data.get("agent_id")):

            agent_id = message.data["agent_id"]
            await self.receive_agent_disconnect(agent_id)

    def add_plugin(self, instance: Any):
        if instance is None:
            raise ValueError("instance cannot be None")

        plugin_type = type(instance)
        plugin_name = plugin_type.__module__ + "." + plugin_type.__name__

        if plugin_name in self.plugin_instances:
            raise ValueError(f"A plugin with the name '{
                             plugin_name}' already exists.")

        self.plugin_instances[plugin_name] = instance

        plugin = PluginModel(
            name=plugin_type.__name__,
            unique_name=plugin_name,
            description="",
            plugin_provider=PluginProvider.SKPlugin,
            plugin_source=PluginSource.HostDefined,
            type=plugin_type
        )

        decorated_methods = [
            method for method in inspect.getmembers(plugin_type, predicate=inspect.isfunction)
            if getattr(method[1], '__kernel_function__', False)
        ]

        for _, method in decorated_methods:
            function = self._create_function_from_method(method)
            plugin.functions.append(function)

        self.plugins.append(plugin)

    def add_plugin_from_type(self, plugin_type: type):
        plugin = PluginModel(
            name=plugin_type.__name__,
            unique_name=plugin_type.__module__ + "." + plugin_type.__name__,
            description="",
            plugin_provider=PluginProvider.SKPlugin,
            plugin_source=PluginSource.HostDefined,
            type=plugin_type
        )

        decorated_methods = [
            method for method in inspect.getmembers(plugin_type, predicate=inspect.isfunction)
            if getattr(method[1], '__kernel_function__', False)
        ]

        for _, method in decorated_methods:
            function = self._create_function_from_method(method)
            plugin.functions.append(function)

        self.plugins.append(plugin)

    @staticmethod
    def _create_function_from_method(method):
        sig = inspect.signature(method)

        function = FunctionModel(
            name=method.__name__,
            description=method.__doc__ or "",
            inputs=[
                ParameterModel(
                    name=param.name,
                    description="",  # Python doesn't have built-in parameter descriptions
                    type=Host._get_friendly_type_name(param.annotation)
                )
                for param in sig.parameters.values()
                if param.name != 'self' and param.kind != inspect.Parameter.VAR_KEYWORD
            ],
            outputs=[
                ParameterModel(
                    name="result",
                    description="",
                    type=Host._get_friendly_type_name(sig.return_annotation)
                )
            ]
        )

        return function

    @staticmethod
    def _get_friendly_type_name(type_hint):
        if hasattr(type_hint, '__origin__'):  # For generic types
            origin = type_hint.__origin__.__name__
            args = ', '.join(arg.__name__ for arg in type_hint.__args__)
            return f"{origin}[{args}]"
        return getattr(type_hint, '__name__', str(type_hint))
