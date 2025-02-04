import asyncio
import logging
from typing import Optional
from pathlib import Path
import sys
import os
from dotenv import load_dotenv

from core.host import Host
from core.broker import Broker
from core.authority import Authority
from core.host import Host
from core.agent_factory import AgentFactory
from core.interfaces.event_log_handler_interface import IEventLogHandler

from hosts.console.interactive_console import InteractiveConsole
from hosts.console.app_config import AppConfig

from utils.service_container import ServiceCollection, ServiceProvider

from plugins.core.openai.chat_completion_plugin import ChatCompletionPlugin
from plugins.core.interaction.interaction_plugin import InteractionPlugin
from plugins.core.interaction.interaction_service_interface import IInteractionService

from semantic_kernel.prompt_template.prompt_template_config import PromptTemplateConfig
from semantic_kernel.core_plugins import (
    ConversationSummaryPlugin,
    MathPlugin,
    TextPlugin,
    TimePlugin,
)


load_dotenv()


def load_app_config() -> AppConfig:
    required_vars = {
        'HOST_ID': os.getenv('HOST_ID'),
        'HOST_SECRET': os.getenv('HOST_SECRET'),
        'BROKER_URI': os.getenv('BROKER_URI'),
        'AUTHORITY_URI': os.getenv('AUTHORITY_URI'),
        'OPENAI_API_KEY': os.getenv('OPENAI_API_KEY')
    }

    # Check for missing environment variables
    missing_vars = [var_name for var_name, value in required_vars.items()
                    if not value or value.strip() == '']

    if missing_vars:
        raise ValueError(f"Missing required environment variables: {
                         ', '.join(missing_vars)}")

    return AppConfig(
        authority_uri=required_vars['AUTHORITY_URI'],
        host_id=required_vars['HOST_ID'],
        host_secret=required_vars['HOST_SECRET'],
        broker_uri_internal=required_vars['BROKER_URI'],
        openai_api_key=required_vars['OPENAI_API_KEY'],
        workspace_path=os.getenv('WORKSPACE_PATH'),
        custom_ntp_host=os.getenv('CUSTOM_NTP_HOST')
    )


def configure_core_services(services: ServiceCollection, app_config: AppConfig):
    # services.add_singleton(EventLoggerProvider)
    services.add_singleton(Broker)
    services.add_singleton(Authority)
    services.add_singleton(AgentFactory)
    services.add_singleton(Host)

    def broker_factory(sp: ServiceProvider) -> Broker:
        logger = logging.getLogger("broker")
        return Broker(logger, app_config.custom_ntp_host)
    services.add_singleton_factory(Broker, broker_factory)

    def authority_factory(sp: ServiceProvider) -> Authority:
        broker = sp.get_required_service(Broker)
        return Authority(
            authority_uri=app_config.authority_uri,
            broker=broker,
            logger=logging.getLogger("authority"),
            service_scope_factory=None,  # TODO: Fix
            authority_uri_internal=app_config.authority_uri_internal,
            broker_uri_internal=app_config.broker_uri_internal
        )
    services.add_singleton_factory(Authority, authority_factory)

    def agent_factory_factory(sp: ServiceProvider) -> AgentFactory:
        broker = sp.get_required_service(Broker)
        authority = sp.get_required_service(Authority)
        return AgentFactory(
            broker=broker,
            authority=authority,
            logger=logging.getLogger("agent_factory"),
            service_provider=sp
        )
    services.add_singleton_factory(AgentFactory, agent_factory_factory)

    def host_factory(sp: ServiceProvider) -> Host:
        broker = sp.get_required_service(Broker)
        authority = sp.get_required_service(Authority)
        agent_factory = sp.get_required_service(AgentFactory)
        return Host(
            host_id=app_config.host_id,
            host_secret=app_config.host_secret,
            broker=broker,
            authority=authority,
            agent_factory=agent_factory,
            logger=logging.getLogger("host")
        )
    services.add_singleton_factory(Host, host_factory)


def configure_console_services(services: ServiceCollection):

    def interactive_console_factory(sp: ServiceProvider) -> InteractiveConsole:
        logger = logging.getLogger("console")
        host = sp.get_required_service(Host)
        return InteractiveConsole(
            host=host,
            logger=logger
        )

    services.add_singleton_factory(
        InteractiveConsole, interactive_console_factory
    )
    services.add_singleton(
        IEventLogHandler,
        lambda sp: sp.get_required_service(InteractiveConsole)
    )
    services.add_singleton(
        IInteractionService,
        lambda sp: sp.get_required_service(InteractiveConsole)
    )


async def main() -> None:

    logging.basicConfig(level=logging.INFO)

    app_config = load_app_config()

    services = ServiceCollection()

    services.add_singleton(AppConfig, lambda _: app_config)
    configure_core_services(services, app_config)
    configure_console_services(services)

    container = services.build()

    host = container.get_required_service(Host)

    # Add custom plugins to host
    host.add_plugin_from_type(ChatCompletionPlugin)
    host.add_plugin_from_type(InteractionPlugin)

    # Add Semantic Kernel plugins
    host.add_plugin_from_type(MathPlugin)
    host.add_plugin_from_type(TextPlugin)
    host.add_plugin_from_type(TimePlugin)

    # Does not work with current implementation
    # host.add_plugin_from_type(ConversationSummaryPlugin)

    # Start the host
    await host.start()

    interaction_service = container.get_required_service(IInteractionService)
    await interaction_service.start()


if __name__ == "__main__":
    asyncio.run(main())
