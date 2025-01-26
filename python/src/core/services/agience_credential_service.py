from typing import Dict, Optional
import uuid
import json
import asyncio
from typing import Any
from base64 import b64decode

from cryptography.hazmat.primitives.asymmetric import rsa
from cryptography.hazmat.primitives import serialization
from cryptography.hazmat.primitives.asymmetric import padding as asymmetric_padding
from cryptography.hazmat.primitives.serialization import load_pem_private_key

from core.authority import Authority
from core.broker import Broker
from core.topic_generator import TopicGenerator
from core.models.messages.broker_message import BrokerMessage, BrokerMessageType


class AgienceCredentialService:
    def __init__(self, agent_id: str, authority: Authority, broker: Broker):
        self._agent_id = agent_id
        self._authority = authority
        self._broker = broker
        self._credentials: Dict[str, str] = {}
        self._topic_generator = TopicGenerator(
            self._authority.id, self._agent_id)

        private_key = rsa.generate_private_key(
            public_exponent=65537,
            key_size=2048
        )

        self._decryption_key = private_key.private_bytes(
            encoding=serialization.Encoding.PEM,
            format=serialization.PrivateFormat.PKCS8,
            encryption_algorithm=serialization.NoEncryption()
        )

        self._encryption_key = private_key.public_key().public_bytes(
            encoding=serialization.Encoding.PEM,
            format=serialization.PublicFormat.SubjectPublicKeyInfo
        )

        self._key_id = str(uuid.uuid4())

    async def get_credential(self, name: str) -> Optional[str]:
        if name in self._credentials:
            return self._credentials[name]

        await self._send_credential_message(name)
        while name not in self._credentials:
            await asyncio.sleep(0.1)
        return self._credentials[name]

    def add_encrypted_credential(self, name: str, encrypted_credential: str) -> None:
        if not name or not encrypted_credential:
            raise ValueError("Invalid credential or name.")

        decrypted_credential = self._decrypt_with_jwk(encrypted_credential)
        self._credentials[name] = decrypted_credential

    async def _send_credential_message(self, credential_name: str) -> None:
        message = BrokerMessage(
            type=BrokerMessageType.EVENT,
            topic=self._topic_generator.publish_to_authority(),
            data={
                "type": "credential_request",
                "agent_id": self._agent_id,
                "credential_name": credential_name,
                "jwk": json.dumps({
                    "kid": self._key_id,
                    "key": self._encryption_key.decode()
                })
            }
        )
        await self._broker.publish(message)

    def _decrypt_with_jwk(self, encrypted_data: str) -> str:
        try:
            # Load the private key
            private_key = load_pem_private_key(
                self._decryption_key,
                password=None
            )

            # Decode the encrypted data
            encrypted_bytes = b64decode(encrypted_data)

            # Decrypt using PKCS1 v1.5 padding
            decrypted_data = private_key.decrypt(
                encrypted_bytes,
                asymmetric_padding.PKCS1v15()
            )

            return decrypted_data.decode('utf-8')

        except Exception as e:
            raise ValueError(f"Decryption failed: {str(e)}")

    # TODO: Python doesn't have direct stack trace attribute inspection
    # Consider alternative authorization approaches
    def _ensure_caller_has_access(self, connection_name: str) -> None:
        pass
