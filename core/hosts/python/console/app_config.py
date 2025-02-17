from typing import Optional

from core.host_config import HostConfig


class AppConfig(HostConfig):
    openai_api_key: Optional[str] = None
    openai_api_url: Optional[str] = None
    custom_ntp_host: Optional[str] = None
    workspace_path: Optional[str] = None
