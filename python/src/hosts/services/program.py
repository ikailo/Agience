import logging
import asyncio
import os
import sys
from dotenv import load_dotenv
from typing import List

from hosts.services.app_config import AppConfig
from hosts.services.worker import Worker

from core.broker import Broker
from core.authority import Authority
from core.host import Host
from core.agent_factory import AgentFactory


load_dotenv()


def load_configuration() -> AppConfig:
    required_vars = {
        'HOST_ID': os.getenv('HOST_ID'),
        'HOST_SECRET': os.getenv('HOST_SECRET'),
        'BROKER_URI': os.getenv('BROKER_URI'),
        'AUTHORITY_URI': os.getenv('AUTHORITY_URI')
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
        broker_uri_internal=required_vars['BROKER_URI']
    )


async def main(args: List[str]):
    logging.basicConfig(level=logging.INFO)
    logger = logging.getLogger(__name__)

    def handle_exception(exc_type, exc_value, exc_traceback):
        logger.error("Unhandled exception occurred:",
                     exc_info=(exc_type, exc_value, exc_traceback))

    sys.excepthook = handle_exception

    try:
        app_config = load_configuration()

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
            service_provider=None,
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

        worker = Worker(host, logger)

        if not worker:
            raise RuntimeError("Worker service not configured")

        # Run the worker
        await worker.execute_async()

    except Exception as ex:
        logger.error("An error occurred while running the application",
                     exc_info=ex)
        sys.exit(1)

if __name__ == "__main__":
    asyncio.run(main(sys.argv[1:]))
