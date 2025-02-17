from typing import List, Optional, Callable, Awaitable
from pydantic import BaseModel
from functools import partial


class AclCheckRequest(BaseModel):
    acc: int
    clientid: str = ""
    topic: str = ""


class MessageAclChecker:
    EVENT_PREFIX: str = "event/"
    CONNECT_PREFIX: str = "connect/"

    # Constants for access control
    ALL: str = "0"
    NONE: str = "-"
    ANY_EXCLUSIVE: str = "*"
    ANY_INCLUSIVE: str = "+"
    QUERY: str = "?"

    # Access types
    READ: int = 1
    WRITE: int = 2
    READ_WRITE: int = 3
    SUBSCRIBE: int = 4

    def __init__(self, verify_host_source_target_relationships: Callable[[str, str, str], Awaitable[bool]]):
        self._verify_host_source_target_relationships = verify_host_source_target_relationships

    async def check_access_control(
        self,
        acl_request: Optional[AclCheckRequest],
        host_id: Optional[str],
        roles: List[str],
        authority_id: Optional[str]
    ) -> bool:
        # Handle connection prefixes
        if acl_request and acl_request.topic.startswith(self.CONNECT_PREFIX):
            return True

        # Remove event prefix if present
        if acl_request and acl_request.topic.startswith(self.EVENT_PREFIX):
            acl_request.topic = acl_request.topic[len(self.EVENT_PREFIX):]

        # Basic validation
        if (not acl_request or
            not acl_request.topic or
            acl_request.topic.isspace() or
                acl_request.acc == 0):
            return False

        if acl_request.acc == self.READ_WRITE:
            return False

        masks = self._get_user_masks(
            acl_request.acc, roles, authority_id, host_id)

        return await self._is_topic_allowed(acl_request.topic, masks, host_id, acl_request.acc)

    def _get_user_masks(
        self,
        access_type: int,
        roles: List[str],
        authority_id: Optional[str],
        host_id: Optional[str]
    ) -> List[str]:
        is_authority = "authority" in roles
        is_host = "host" in roles
        masks = []

        if is_authority and authority_id:
            if access_type in (self.READ, self.SUBSCRIBE):
                # Any -> Authority
                masks.append(
                    f"{self.ANY_EXCLUSIVE}/{authority_id}/{self.NONE}/{self.NONE}")

            if access_type == self.WRITE:
                # Authority -> Any Host or Agent
                masks.append(
                    f"{self.NONE}/{authority_id}/{self.ANY_INCLUSIVE}/{self.ANY_INCLUSIVE}")

        if is_host and host_id:
            if access_type in (self.READ, self.SUBSCRIBE):
                masks.extend([
                    # Authority -> All Hosts
                    f"{self.NONE}/{authority_id}/{self.ALL}/{self.NONE}",
                    # Authority -> Host
                    f"{self.NONE}/{authority_id}/{host_id}/{self.NONE}",
                    # Authority -> Agent
                    f"{self.NONE}/{authority_id}/{host_id}/{self.QUERY}",
                    # Any -> Agent
                    f"{self.ANY_EXCLUSIVE}/{authority_id}/{self.NONE}/{self.QUERY}"
                ])

            if access_type == self.WRITE:
                masks.extend([
                    # Host -> Authority
                    f"{host_id}/{authority_id}/{self.NONE}/{self.NONE}",
                    # Agent -> Authority
                    f"{self.QUERY}/{authority_id}/{self.NONE}/{self.NONE}",
                    # Agent -> Agent
                    f"{self.QUERY}/{authority_id}/{self.NONE}/{self.QUERY}"
                ])

        return masks

    async def _is_topic_allowed(
        self,
        topic: str,
        masks: List[str],
        host_id: Optional[str],
        access_type: int
    ) -> bool:
        for mask in masks:
            if self._check_mask(topic, mask, access_type):
                return True

            if self.QUERY in mask:  # TODO: Subject to "?" injection attack
                if not host_id:
                    raise ValueError(
                        "host_id cannot be None when processing query masks")

                if await self._check_query_mask_async(topic, mask, host_id, access_type):
                    return True

        return False

    async def _check_query_mask_async(
        self,
        topic: str,
        mask: str,
        host_id: str,
        access_type: int
    ) -> bool:
        topic_parts = topic.split('/')
        mask_parts = mask.split('/')

        if not self._is_valid_topic_and_mask(topic_parts, mask_parts):
            return False

        source_id = topic_parts[0] if mask_parts[0] == self.QUERY else None
        target_agent_id = topic_parts[3] if mask_parts[3] == self.QUERY else None

        if source_id == self.NONE or target_agent_id == self.NONE:
            return False

        if access_type == self.WRITE and source_id == target_agent_id:
            return False  # Can't send to self

        return await self._verify_host_source_target_relationships(host_id, source_id, target_agent_id)

    def _check_mask(self, topic: str, mask: str, access_type: int) -> bool:
        topic_parts = topic.split('/')
        mask_parts = mask.split('/')

        if not self._is_valid_topic_and_mask(topic_parts, mask_parts):
            return False

        if not topic_parts[0] or not mask_parts[0]:
            return False

        # First part is the sender id. Read from any sender. Otherwise, the sender id must match the claims.
        if access_type != self.READ:
            if access_type == self.SUBSCRIBE and topic_parts[0] != self.ANY_INCLUSIVE:
                return False
            if access_type == self.WRITE and topic_parts[0] != mask_parts[0]:
                return False

        for i in range(1, len(mask_parts)):
            mask_part = mask_parts[i]
            topic_part = topic_parts[i]

            if mask_part == self.ANY_INCLUSIVE:
                continue
            elif mask_part == self.ANY_EXCLUSIVE and topic_part != self.ALL:
                continue
            elif mask_part == self.ALL and topic_part == self.ALL:
                continue
            elif mask_part == self.NONE and topic_part == self.NONE:
                continue
            elif mask_part != topic_part:
                return False

        return True

    def _is_valid_topic_and_mask(self, topic_parts: List[str], mask_parts: List[str]) -> bool:
        return len(topic_parts) == 4 and len(mask_parts) == 4

    @staticmethod
    def _print_const(value: int) -> str:
        const_map = {
            0: "0",
            1: "READ",
            2: "WRITE",
            3: "READ_WRITE",
            4: "SUBSCRIBE"
        }
        return const_map.get(value, "UNKNOWN")
