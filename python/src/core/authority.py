from typing import Optional, Dict, List, Any
from pydantic import BaseModel
from urllib.parse import urlparse, urlunparse
import json
import asyncio
import logging


from broker import Broker, BrokerMessage, BrokerMessageType
from models.entities.host import Host
from models.entities.agent import Agent
from models.entities.plugin import Plugin
from topic_generator import TopicGenerator


class Authority:
    BROKER_URI_KEY = "broker_uri"
    FILES_URI_KEY = "files_uri"
    OPENID_CONFIG_PATH = "/.well-known/openid-configuration"

    def __init__(
        self,
        authority_uri: str,
        broker: Broker,
        # TODO: Look for type of service_scope_factory
        service_scope_factory,
        logger: logging.Logger,
        authority_uri_internal: Optional[str] = None,
        broker_uri_internal: Optional[str] = None
    ):
        if not authority_uri:
            raise ValueError("authority_uri cannot be empty")

        self._authority_uri = urlparse(authority_uri)
        self._authority_uri_internal = urlparse(
            authority_uri_internal) if authority_uri_internal else None
        self._broker = broker or ValueError("broker cannot be None")
        self._service_scope_factory = service_scope_factory
        self._logger = logger or ValueError("logger cannot be None")
        self._broker_uri = broker_uri_internal

        self.token_endpoint: Optional[str] = None
        self.files_uri: Optional[str] = None
        self.is_connected: bool = False

        self._topic_generator = TopicGenerator(self.id, self.id)

    @property
    def id(self) -> str:
        return self._authority_uri.hostname

    @property
    def timestamp(self) -> str:
        return self._broker.timestamp

    def get_authority_records_repository(self):
        scope = self._service_scope_factory.create_scope()
        return scope.service_provider.get_required_service("IAuthorityRecordsRepository")

    async def initialize_with_backoff(self, max_delay_seconds: float = 16):
        if self._broker_uri and self.token_endpoint:
            self._logger.info("Authority already initialized.")
            return

        delay = 1.0
        while True:
            try:
                authority_uri = self._authority_uri_internal or self._authority_uri
                authority_url = urlunparse(authority_uri)

                self._logger.info(f"Initializing Authority: {authority_url}")

                # TODO: This is a simplified version. You'll need to implement
                # OpenID Connect configuration retrieval based on your needs
                config = await self._fetch_openid_config(f"{authority_url}{self.OPENID_CONFIG_PATH}")

                if not self._broker_uri:
                    self._broker_uri = config.get(self.BROKER_URI_KEY)
                if not self.files_uri:
                    self.files_uri = config.get(self.FILES_URI_KEY)

                if self._authority_uri_internal is None:
                    self.token_endpoint = config.get("token_endpoint")
                elif config.get("token_endpoint"):
                    # Replace host and port with override
                    token_endpoint_uri = urlparse(config["token_endpoint"])
                    new_token_endpoint = token_endpoint_uri._replace(
                        netloc=f"{self._authority_uri_internal.hostname}:{
                            self._authority_uri_internal.port}"
                    )
                    self.token_endpoint = urlunparse(new_token_endpoint)
                else:
                    raise Exception(
                        "TokenEndpoint not found in OpenIdConnectConfiguration")

                self._logger.info("Authority initialized.")
                break

            except Exception as ex:
                self._logger.debug(str(ex))
                self._logger.info(
                    f"Unable to initialize Authority. Retrying in {delay} seconds.")
                await asyncio.sleep(delay)
                delay = min(delay * 2, max_delay_seconds)

    async def connect(self, access_token: str):
        if not self.is_connected:
            if not self._broker_uri:
                await self.initialize_with_backoff()

            broker_uri = self._broker_uri
            if not broker_uri:
                raise ValueError("BrokerUri")

            await self._broker.connect(access_token, broker_uri)

            if self._broker.is_connected:
                await self._broker.subscribe(
                    self._topic_generator.subscribe_as_authority(),
                    self._broker_receive_message
                )
                self.is_connected = True

    async def disconnect(self):
        if self.is_connected:
            await self._broker.unsubscribe(self._topic_generator.subscribe_as_authority())
            await self._broker.disconnect()
            self.is_connected = False

    async def _broker_receive_message(self, message: BrokerMessage):
        self._logger.info(f"MessageReceived: sender:{
                          message.sender_id}, destination:{message.destination}")

        if not message.sender_id or not message.data:
            return

        if (message.type == BrokerMessageType.EVENT and
            message.data.get("type") == "host_connect" and
                message.data.get("host")):

            host = Host.parse_raw(message.data["host"])
            if host.id == message.sender_id:
                await self._on_host_connected(host)

        if (message.type == BrokerMessageType.EVENT and
            message.data.get("type") == "credential_request" and
            message.data.get("credential_name") and
            message.data.get("jwk") and
                message.data.get("agent_id") == message.sender_id):

            credential_name = message.data["credential_name"]
            agent_id = message.data["agent_id"]
            jwk = json.loads(message.data["jwk"])

            await self._handle_credential_request(agent_id, credential_name, jwk)

    async def _handle_credential_request(self, agent_id: str, credential_name: str, jwk: dict):
        authority_records_repository = self.get_authority_records_repository()

        # TODO: Implement this
        credential = await authority_records_repository.get_credential_for_agent_by_name(
            agent_id, credential_name
        )

        if not credential:
            print(f"Credential '{
                  credential_name}' not found for Agent '{agent_id}'.")
            return

        # Note: You'll need to implement the encryption method based on your needs
        encrypted_credential = self._encrypt_with_jwk(credential, jwk)

        await self._broker.publish_async(BrokerMessage(
            type=BrokerMessageType.EVENT,
            topic=self._topic_generator.publish_to_agent(agent_id),
            data={
                "type": "credential_response",
                "credential_name": credential_name,
                "encrypted_credential": encrypted_credential
            }
        ))

        print(f"Credential response sent for '{
              credential_name}' to Agent '{agent_id}'.")

    async def _on_host_connected(self, model_host: Host):
        # TODO: Implement this
        authority_records_repository = self.get_authority_records_repository()

        self._logger.info(f"Received host_connect from: {model_host.id}")

        host = await authority_records_repository.get_host_by_id(model_host.id)
        self._logger.info(f"Found Host {host.name}")
        self._logger.debug(f"Host: {json.dumps(host.dict())}")

        plugins = await authority_records_repository.sync_plugins_for_host_by_id(
            model_host.id, model_host.plugins
        )
        self._logger.info(f"Found {len(plugins)} Plugins")
        self._logger.debug(
            f"Plugins: {json.dumps([p.dict() for p in plugins])}")

        agents = await authority_records_repository.get_agents_for_host_by_id(model_host.id)
        self._logger.info(f"Found {len(agents)} Agents")
        self._logger.debug(f"Agents: {json.dumps([a.dict() for a in agents])}")

        self._send_host_welcome_event(host, plugins, agents)

    def _send_host_welcome_event(self, host: Host, plugins: List[Plugin], agents: List[Agent]):
        if not self.is_connected:
            raise RuntimeError("Not Connected")

        self._logger.info(f"Publishing Host Welcome Event: {host.name}")

        self._broker.publish(BrokerMessage(
            type=BrokerMessageType.EVENT,
            topic=self._topic_generator.publish_to_host(host.id),
            data={
                "type": "host_welcome",
                "timestamp": self._broker.timestamp,
                "host": json.dumps(host.dict()),
                "plugins": json.dumps([p.dict() for p in plugins]),
                "agents": json.dumps([a.dict() for a in agents])
            }
        ))

    async def _send_agent_connect_event(self, agent: Agent):
        if not self.is_connected:
            raise RuntimeError("Not Connected")

        authority_records_repository = self.get_authority_records_repository()

        # TODO: Implement this
        host_id = await authority_records_repository.get_host_id_for_agent_by_id(agent.id)

        self._logger.info(f"Sending Agent Connect Event: {agent.name}")
        self._logger.debug(f"Agent: {json.dumps(agent.dict())}")

        self._broker.publish(BrokerMessage(
            type=BrokerMessageType.EVENT,
            topic=self._topic_generator.publish_to_host(host_id),
            data={
                "type": "agent_connect",
                "timestamp": self._broker.timestamp,
                "agent": json.dumps(agent.dict())
            }
        ))

    async def _send_agent_disconnect_event(self, agent: Agent):
        if not self.is_connected:
            raise RuntimeError("Not Connected")

        # TODO: Implement this
        authority_records_repository = self.get_authority_records_repository()
        host_id = await authority_records_repository.get_host_id_for_agent_by_id(agent.id)

        self._broker.publish(BrokerMessage(
            type=BrokerMessageType.EVENT,
            topic=self._topic_generator.publish_to_host(host_id),
            data={
                "type": "agent_disconnect",
                "timestamp": self._broker.timestamp,
                "agent_id": agent.id
            }
        ))

    # TODO: Implement this
    async def _fetch_openid_config(self, config_url: str) -> dict:
        # Implement OpenID Connect configuration retrieval
        # This is a placeholder - you'll need to implement based on your needs
        pass

    # TODO: Implement this
    def _encrypt_with_jwk(self, credential: Any, jwk: dict) -> str:
        # Implement JWK encryption
        # This is a placeholder - you'll need to implement based on your needs
        pass
