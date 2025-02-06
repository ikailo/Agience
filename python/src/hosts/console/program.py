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

from hosts.console.interactive_console import InteractiveConsole
from hosts.console.app_config import AppConfig

from semantic_kernel.prompt_template.prompt_template_config import PromptTemplateConfig
from semantic_kernel.core_plugins import (
    ConversationSummaryPlugin,
    MathPlugin,
    TextPlugin,
    TimePlugin,
)

from plugins.core.openai.chat_completion_plugin import ChatCompletionPlugin
# from plugins.core.interaction import InteractionPlugin
# from plugins.core.code import Git


load_dotenv()


def load_configuration() -> AppConfig:
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


async def main() -> None:

    logging.basicConfig(level=logging.INFO)
    logger = logging.getLogger(__name__)

    app_config = load_configuration()

    # TODO: Git Plugin not implemented yet
    # if app_config.workspace_path:
    #     host.add_plugin(Git(app_config.workspace_path))

    broker = Broker(logger=logging.getLogger("broker"))

    # TODO: service_scope_factory should not be null, implement
    authority = Authority(
        authority_uri=app_config.authority_uri,
        broker=broker,
        service_scope_factory=None,
        logger=logging.getLogger("authority"),
        authority_uri_internal=None,
        broker_uri_internal=None
    )

    agent_factory = AgentFactory(
        main_service_provider=None,
        broker=broker,
        authority=authority,
        logger=logging.getLogger("agent_factory")
    )

    host = Host(
        host_id=app_config.host_id,
        host_secret=app_config.host_secret,
        authority=authority,
        broker=broker,
        agent_factory=agent_factory,
        logger=logging.getLogger("host")
    )

    # Add custom plugins to host
    host.add_plugin_from_type(ChatCompletionPlugin)
    # host.add_plugin(InteractionPlugin())

    # Add Semantic Kernel plugins
    host.add_plugin_from_type(MathPlugin)
    host.add_plugin_from_type(TextPlugin)
    host.add_plugin_from_type(TimePlugin)

    # Does not work with current implementation
    # host.add_plugin_from_type(ConversationSummaryPlugin)

    # Start the host
    await host.start()

    # Start the interaction service
    interaction_service = InteractiveConsole(logger, host)
    await interaction_service.start()

if __name__ == "__main__":
    asyncio.run(main())
