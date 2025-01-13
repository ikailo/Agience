import ssl
from datetime import datetime
from typing import Dict, List, Optional, Callable, Any
import asyncio
from asyncio import Task
import logging
from ntplib import NTPClient
from pydantic import BaseModel
import paho.mqtt.client as mqtt
from urllib.parse import urlparse

from core.models.messages.broker_message import BrokerMessage, BrokerMessageType
from core.information import Information


class CallbackContainer(BaseModel):
    callback: Callable[[BrokerMessage], Task]


class Broker:
    MESSAGE_TYPE_KEY = "message.type"
    TIME_FORMAT = "%Y-%m-%dT%H:%M:%S.%f"

    def __init__(self, logger: logging.Logger, custom_ntp_host: Optional[str] = None):
        self._logger = logger
        self._custom_ntp_host = custom_ntp_host
        self._mqtt_client = mqtt.Client(protocol=mqtt.MQTTv5)
        self._callbacks: Dict[str, List[CallbackContainer]] = {}
        self._ntp_client: Optional[NTPClient] = None
        self._connected = False

        # NTP hosts
        self.ntp_hosts = [
            "pool.ntp.org",
            "north-america.pool.ntp.org",
            "europe.pool.ntp.org",
            "asia.pool.ntp.org",
            "south-america.pool.ntp.org",
            "africa.pool.ntp.org",
            "oceania.pool.ntp.org"
        ]

        # Set up MQTT callbacks

        self._mqtt_client.on_connect = self._on_connect
        self._mqtt_client.on_message = self._on_message
        self._mqtt_client.on_disconnect = self._on_disconnect

    @property
    def is_connected(self) -> bool:
        return self._connected

    @property
    def timestamp(self) -> str:
        if not self._ntp_client:
            raise RuntimeError("NTP client not initialized")
        return datetime.utcfromtimestamp(self._ntp_client.request('pool.ntp.org').tx_time).strftime(self.TIME_FORMAT)

    async def connect(self, token: str, broker_uri: str):
        await self._start_ntp_clock()
        self._logger.info(f"Broker Connected Status: {self.is_connected}")

        if not self.is_connected:
            self._logger.info(f"Connecting to {broker_uri}")

            # Parse the broker URI to get host and port
            # Expected format: "wss://example.com:8883"
            parsed_uri = urlparse(broker_uri)
            host = parsed_uri.hostname
            port = parsed_uri.port or 443  # Default to 443 if not specified

            # Configure MQTT client
            self._mqtt_client.username_pw_set(
                username=token, password="<no_password>")
            self._mqtt_client.transport = "websockets"
            self._mqtt_client.ws_set_options(path="/mqtt")

            # TODO: SSL temp fix
            # self._mqtt_client.tls_set()  # Enable TLS
            self._mqtt_client.tls_set(cert_reqs=ssl.CERT_NONE)
            self._mqtt_client.tls_insecure_set(True)

            try:
                self._mqtt_client.connect(host, port, keepalive=60)
                # self._mqtt_client.loop_start()  # Start network loop in separate thread

                # Start a background loop for the MQTT client
                loop = asyncio.get_running_loop()
                loop.run_in_executor(None, self._mqtt_client.loop_start)

                # Wait for connection or timeout
                timeout = 10
                start_time = datetime.now()
                while not self.is_connected and (datetime.now() - start_time).seconds < timeout:
                    await asyncio.sleep(0.1)

                if not self.is_connected:
                    raise TimeoutError("Connection timeout")

                self._logger.info("Broker Connected")
            except Exception as e:
                self._logger.error(f"Broker Connection Failed: {str(e)}")
                raise

    def _on_connect(self, client, userdata, flags, rc, properties=None):
        if rc == 0:
            self._connected = True
        else:
            self._logger.error(f"Connection failed with code {rc}")

    def _on_message(self, client, userdata, msg: mqtt.MQTTMessage):
        self._logger.info(f"Received Message: {msg.topic}")

        topic_parts = msg.topic.split('/')
        callback_topic = '/'.join(topic_parts[2:])  # Remove SenderId segment

        if callback_topic in self._callbacks:
            try:
                # Get message type from properties
                properties = msg.properties if hasattr(
                    msg, 'properties') else {}
                try:
                    message_type = BrokerMessageType(
                        properties.get(self.MESSAGE_TYPE_KEY, 'UNKNOWN'))
                except ValueError:
                    message_type = BrokerMessageType.UNKNOWN

                message = BrokerMessage(
                    type=message_type,
                    topic=msg.topic
                )

                if message_type == BrokerMessageType.EVENT:
                    message.data = msg.payload.decode()
                elif message_type == BrokerMessageType.INFORMATION:
                    message.information = Information.parse_raw(msg.payload)

                # Create event loop if not exists
                try:
                    loop = asyncio.get_event_loop()
                except RuntimeError:
                    loop = asyncio.new_event_loop()
                    asyncio.set_event_loop(loop)

                for container in self._callbacks[callback_topic]:
                    asyncio.create_task(container.callback(message))

            except Exception as e:
                self._logger.error(f"Message handling error: {str(e)}")
                raise

    def _on_disconnect(self, client, userdata, rc):
        self._connected = False
        self._logger.info(f"Broker disconnected with code: {rc}")

    async def subscribe(self, topic: str, callback: Callable[[BrokerMessage], Task]):
        if not self.is_connected:
            raise RuntimeError("Not Connected")

        topic_parts = topic.split('/')
        callback_topic = '/'.join(topic_parts[2:])

        container = CallbackContainer(callback=callback)

        if callback_topic not in self._callbacks:
            self._callbacks[callback_topic] = []

        self._callbacks[callback_topic].append(container)
        self._mqtt_client.subscribe(topic, qos=0)

    async def unsubscribe(self, topic: str):
        topic_parts = topic.split('/')
        callback_topic = '/'.join(topic_parts[2:])

        if callback_topic in self._callbacks:
            del self._callbacks[callback_topic]

        self._mqtt_client.unsubscribe(topic)

    async def disconnect(self):
        if self.is_connected:
            self._mqtt_client.disconnect()
            self._mqtt_client.loop_stop()
            self._connected = False

    def publish(self, message: BrokerMessage):
        asyncio.create_task(self.publish_async(message))

    async def publish_async(self, message: BrokerMessage):
        if not self.is_connected:
            return

        if not message.topic:
            raise ValueError("Topic cannot be None")

        payload = ""
        if message.type == BrokerMessageType.EVENT:
            payload = str(message.data) if message.data else ""
        elif message.type == BrokerMessageType.INFORMATION:
            payload = message.information.json() if message.information else ""

        properties = mqtt.Properties(
            user_property=(self.MESSAGE_TYPE_KEY, message.type.value)
        )

        self._mqtt_client.publish(
            message.topic,
            payload.encode(),
            qos=0,
            retain=False,
            properties=properties
        )

    async def _start_ntp_clock(self):
        await self._query_ntp_with_backoff()

        # Schedule daily NTP updates
        # while True:
        #     await asyncio.sleep(24 * 60 * 60)  # 24 hours
        #     await self._query_ntp_with_backoff()

    async def _query_ntp_with_backoff(self, max_delay_seconds: float = 32):
        if self._custom_ntp_host:
            if not self._custom_ntp_host.lower().endswith('pool.ntp.org'):
                raise ValueError(
                    "The CustomNtpHost must end with `pool.ntp.org`")
            self.ntp_hosts = [self._custom_ntp_host]

        delay = 1
        current_host_index = 0

        while True:
            ntp_host = self.ntp_hosts[current_host_index]
            try:
                self._ntp_client = NTPClient()
                self._logger.info(f"NTP Querying host {ntp_host}")
                self._ntp_client.request(ntp_host)
                self._logger.info(
                    f"Connected to {ntp_host}. NTP Time: {self.timestamp}")
                break
            except Exception as e:
                self._logger.error(f"NTP Query to host {ntp_host} failed")

                start_new_cycle = current_host_index == len(self.ntp_hosts) - 1
                current_host_index = 0 if start_new_cycle else current_host_index + 1

                if start_new_cycle:
                    self._logger.info(f"Trying again a NTP connection in {
                                      delay} seconds.\n{str(e)}")
                    await asyncio.sleep(delay)
                    delay = min(delay * 2, max_delay_seconds)
