import asyncio
import logging
import json
from typing import Callable, Dict, List, Optional
from paho.mqtt.client import Client, MQTTMessage
from ntplib import NTPClient, NTPException
from datetime import datetime, timedelta
from urllib.parse import urlparse
import threading
from models.messages.broker_message import BrokerMessage, BrokerMessageType

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)


class Broker:
    def __init__(self, custom_ntp_host: Optional[str] = None):
        self._ntp_client = NTPClient()
        self._custom_ntp_host = custom_ntp_host
        self.ntp_hosts = [
            "pool.ntp.org",
            "north-america.pool.ntp.org",
            "europe.pool.ntp.org",
            "asia.pool.ntp.org",
            "south-america.pool.ntp.org",
            "africa.pool.ntp.org",
            "oceania.pool.ntp.org",
        ]
        self._mqtt_client = Client()
        self._callbacks: Dict[str,
                              List[Callable[[BrokerMessage], asyncio.Task]]] = {}
        self._is_connected = False
        self._timestamp_format = "%Y-%m-%dT%H:%M:%S.%f"
        self._ntp_timer = threading.Timer(
            interval=86400, function=self._start_ntp_sync)
        self._lock = threading.Lock()

        # MQTT client event handlers
        self._mqtt_client.on_connect = self._on_connect
        self._mqtt_client.on_message = self._on_message
        self._mqtt_client.on_disconnect = self._on_disconnect

    async def connect(self, token: str, broker_uri: str):
        await self._start_ntp_sync()
        logger.info("Connecting to MQTT broker: %s", broker_uri)

        # Parse the broker URI to get host and port
        parsed_uri = urlparse(broker_uri)
        host = parsed_uri.hostname
        port = parsed_uri.port or 443  # Default to 443 if not specified

        # MQTT connection configuration
        self._mqtt_client.username_pw_set(
            username=token, password="<no_password>")
        self._mqtt_client.tls_set()

        # Configure for websockets
        self._mqtt_client.transport = "websockets"
        self._mqtt_client.ws_set_options(path="/mqtt")  # Adjust path if needed

        # Connect using the parsed host and port
        self._mqtt_client.connect(host, port)

        # Start a background loop for the MQTT client
        loop = asyncio.get_running_loop()
        loop.run_in_executor(None, self._mqtt_client.loop_forever)

    async def disconnect(self):
        if self._is_connected:
            self._mqtt_client.disconnect()
            self._is_connected = False
            logger.info("Disconnected from MQTT broker.")

    async def subscribe(self, topic: str, callback: Callable[[BrokerMessage], asyncio.Task]):
        if not self._is_connected:
            raise ConnectionError("MQTT client is not connected.")

        # Register the callback for the topic
        with self._lock:
            if topic not in self._callbacks:
                self._callbacks[topic] = []
            self._callbacks[topic].append(callback)

        # Subscribe to the topic
        self._mqtt_client.subscribe(topic)
        logger.info("Subscribed to topic: %s", topic)

    async def unsubscribe(self, topic: str):
        if not self._is_connected:
            raise ConnectionError("MQTT client is not connected.")

        # Remove callbacks and unsubscribe
        with self._lock:
            self._callbacks.pop(topic, None)
        self._mqtt_client.unsubscribe(topic)
        logger.info("Unsubscribed from topic: %s", topic)

    async def publish(self, message: BrokerMessage):
        if not self._is_connected:
            logger.error("Cannot publish, MQTT client is not connected")
            raise ConnectionError("MQTT client is not connected.")

        payload = ""
        if message.type == BrokerMessageType.EVENT:
            payload = str(message.data)
        elif message.type == BrokerMessageType.INFORMATION:
            payload = json.dumps(message.information)

        resp = self._mqtt_client.publish(
            topic=message.topic,
            payload=payload,
            qos=0,  # QoS Level 0 for at-most-once delivery
            retain=False
        )

        logger.info("Published message to topic %s", message.topic)
        print(f"Publised? - {resp.is_published()}")
        print(resp)

    def _on_connect(self, client, userdata, flags, rc):
        self._is_connected = rc == 0
        logger.info("Connected to MQTT broker with result code: %s", rc)

    def _on_disconnect(self, client, userdata, rc):
        self._is_connected = False
        logger.info("Disconnected from MQTT broker with result code: %s", rc)

    def _on_message(self, client, userdata, msg: MQTTMessage):
        topic = msg.topic
        payload = msg.payload.decode("utf-8")
        logger.info("Message received on topic %s: %s", topic, payload)

        # Handle registered callbacks
        with self._lock:
            if topic in self._callbacks:
                for callback in self._callbacks[topic]:
                    message_type = msg.properties.UserProperties.get(
                        "message.type", BrokerMessageType.UNKNOWN)
                    message = BrokerMessage(
                        topic=topic, message_type=message_type, data=payload)
                    asyncio.create_task(callback(message))

    async def _start_ntp_sync(self):
        await self._query_ntp_with_backoff()

    async def _query_ntp_with_backoff(self, max_delay_seconds: int = 32):
        hosts = [self._custom_ntp_host] if self._custom_ntp_host else self.ntp_hosts
        delay = 1
        for host in hosts:
            try:
                logger.info("Querying NTP server: %s", host)
                response = self._ntp_client.request(host)
                timestamp = datetime.utcfromtimestamp(response.tx_time)
                logger.info("NTP Time from %s: %s", host,
                            timestamp.strftime(self._timestamp_format))
                return
            except (NTPException, Exception) as e:
                logger.error("Failed to query NTP server %s: %s", host, e)
                delay = min(delay * 2, max_delay_seconds)
                await asyncio.sleep(delay)
