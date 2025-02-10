from typing import Dict, Optional
import uuid
import asyncio
import base64


from cryptography.hazmat.primitives.serialization import load_pem_private_key
from cryptography.hazmat.primitives.asymmetric import padding
from cryptography.hazmat.primitives import hashes
from jwcrypto.jwk import JWK

from core.authority import Authority
from core.broker import Broker
from core.data import Data
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

        self._key_id = str(uuid.uuid4())
        key: JWK = JWK.generate(kty='RSA', size=2048, kid=self._key_id)

        private_key_dict = key.export_private(as_dict=True)
        self._decryption_key: JWK = JWK(**private_key_dict)

        public_key_dict = key.export_public(as_dict=True)
        self._encryption_key = JWK(**public_key_dict)

        self._encryption_jwk = self._encryption_key.export()

    async def get_credential(self, name: str) -> Optional[str]:
        # self._ensure_caller_has_access(name)

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
        data = Data()

        data.add("type", "credential_request")
        data.add("agent_id", self._agent_id)
        data.add("credential_name", credential_name)
        data.add("jwk", self._encryption_jwk)

        message = BrokerMessage(
            type=BrokerMessageType.EVENT,
            topic=self._topic_generator.publish_to_authority(),
            data=data,
        )

        await self._broker.publish(message)

    # TODO: Fix failing decryption (high priority)
    def _decrypt_with_jwk(self, encrypted_credential: str) -> str:
        try:
            pem_data = self._decryption_key.export_to_pem(
                private_key=True,
                password=None
            )
            private_key = load_pem_private_key(pem_data, password=None)

            encrypted_bytes = base64.b64decode(encrypted_credential)

            decrypted_bytes = private_key.decrypt(
                encrypted_bytes,
                padding.OAEP(
                    mgf=padding.MGF1(algorithm=hashes.SHA256()),
                    algorithm=hashes.SHA256(),
                    label=None
                )
            )

            return decrypted_bytes.decode('utf-8')

        except Exception as e:
            print(e)
            raise ValueError(f"Decryption failed: {str(e)}")

    # TODO: Python doesn't have direct stack trace attribute inspection
    # Consider alternative authorization approaches
    # Not used in the current implementation
    # Low priority
    def _ensure_caller_has_access(self, connection_name: str) -> None:
        import inspect

        frame = inspect.currentframe()
        if frame is None:
            raise Exception("Caller information could not be determined.")

        caller_frame = frame.f_back
        if caller_frame is None:
            raise Exception("Caller information could not be determined.")

        # Get the method that called this function
        method = caller_frame.f_code

        # Check for AgienceConnection attribute/decorator
        # Note: This would need to be implemented differently in Python
        # as Python handles attributes/decorators differently than C#
        if not hasattr(method, 'agience_connection') or \
           method.agience_connection != connection_name:
            raise Exception(f"Caller does not have access to the credential: {
                            connection_name}.")
