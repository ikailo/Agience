from typing import Optional, Dict, Any
from collections import deque
from concurrent.futures import CancelledError
import asyncio
import logging
from datetime import datetime
from pydantic import BaseModel
from typing import Dict, Optional, List
import threading
from queue import Queue


from core.host import Host
from core.agent import Agent

from plugins.core.interaction.interaction_service_interface import IInteractionService


class MessageRequest(BaseModel):
    message: Optional[str] = None
    agent_id: Optional[str] = None
    agent_name: Optional[str] = None
    is_input: bool = False
    is_processed: bool = False
    tag: Optional[str] = None


class InteractiveConsole(IInteractionService):
    def __init__(self, logger: logging.Logger, host: Host):
        self.logger = logger
        self.host = host

        self.agent_message_queues: Dict[str, deque[MessageRequest]] = {}
        self.message_queue: deque[MessageRequest] = deque()
        self.current_agent_id: Optional[str] = None
        self._stop_event = asyncio.Event()

        # Register agent connected callback
        self.host.agent_connected = self.on_agent_connected

        # Start background message processing
        asyncio.create_task(self.process_messages())

    def on_log_entry_received(self, sender: Any, args: Any) -> None:
        pass

    async def on_agent_connected(self, agent: Agent) -> None:
        self.logger.info(f"Agent connected: {agent.name} ({agent.id})")

        if agent.id not in self.agent_message_queues:
            self.agent_message_queues[agent.id] = deque()

        if not self.current_agent_id:
            self.switch_context_by_name(agent.name)

    async def process_messages(self) -> None:
        while not self._stop_event.is_set():
            if self.message_queue:
                request = self.message_queue.popleft()

                if (self.current_agent_id and
                    self.current_agent_id in self.agent_message_queues and
                        request in self.agent_message_queues[self.current_agent_id]):
                    self.handle_message(request)
                else:
                    self.message_queue.append(request)

            await asyncio.sleep(0.05)  # Prevent tight loop

    def handle_message(self, request: MessageRequest) -> None:
        message = self.format_message(
            request.tag, request.agent_name, request.message)

        if request.is_input:
            print(message, end='')
            request.message = input()
        else:
            print(message)

        request.is_processed = True

    async def send_to_agent(self, agent: Agent) -> Optional[str]:
        message_request = MessageRequest(
            agent_id=agent.id,
            agent_name=agent.name,
            tag="TO",
            is_input=True,
            is_processed=False
        )

        self.message_queue.append(message_request)
        if agent.id not in self.agent_message_queues:
            self.agent_message_queues[agent.id] = deque()
        self.agent_message_queues[agent.id].append(message_request)

        while not message_request.is_processed:
            await asyncio.sleep(0.05)

        return message_request.message

    async def receive_from_agent(self, agent: Agent, message: str) -> None:
        message_request = MessageRequest(
            agent_id=agent.id,
            agent_name=agent.name,
            message=message,
            tag="FROM",
            is_input=False,
            is_processed=False
        )

        self.message_queue.append(message_request)
        if agent.id not in self.agent_message_queues:
            self.agent_message_queues[agent.id] = deque()
        self.agent_message_queues[agent.id].append(message_request)

    def format_message(self, type_: str, agent_name: Optional[str], content: Optional[str]) -> str:
        return f"[{datetime.now().strftime('%H:%M:%S')}] [{type_}] {agent_name}> {content}"

    async def start(self) -> None:
        print("Welcome to Agience Interactive Console!")
        print("Type `/help` for a list of commands.")

        while True:
            agent_name = self.get_agent_name_by_id(
                self.current_agent_id) or "NoAgent"
            print(f"[{datetime.now().strftime('%H:%M:%S')}] [IN] {
                  agent_name}> ", end='')

            try:
                input_text = input()

                if not input_text.strip():
                    print()
                    continue

                if input_text == "/help":
                    self.show_help()
                elif input_text.startswith("/switch "):
                    agent_name_to_switch = input_text[8:].strip()
                    self.switch_context_by_name(agent_name_to_switch)
                elif input_text == "/list":
                    self.display_agent_status()
                elif input_text == "/logs":
                    self.display_logs()
                else:
                    await self.send_to_current_agent(input_text)

                print()

            except asyncio.CancelledError:
                break
            except Exception as e:
                self.logger.error(f"Error processing input: {e}")

    def show_help(self) -> None:
        print("Available Commands:")
        print("/help       - Show this help menu")
        print("/list       - List all connected agents")
        print("/switch <agentName> - Switch to a specific agent")
        print("/logs       - Display recent logs")

    def switch_context_by_name(self, agent_name: str) -> None:
        agent_id = self.get_agent_id_by_name(agent_name)
        if agent_id:
            self.current_agent_id = agent_id
            print()
            print(f"[{datetime.now().strftime('%H:%M:%S')}] [IN] {
                  agent_name}> ", end='')
        else:
            print(f"Error: Agent {agent_name} not found.")

    async def send_to_current_agent(self, input_text: str) -> None:
        if not self.current_agent_id:
            print("No agent selected. Use `/list` to see available agents.")
            return

        agent = self.host.agents.get(self.current_agent_id)
        if agent:
            response = await agent.prompt_async(input_text)
            print(f"[{datetime.now().strftime('%H:%M:%S')}] [OUT] {
                  agent.name}> {response}")
        else:
            print(f"Error: Unable to send message to {self.current_agent_id}.")

    def display_agent_status(self) -> None:
        print("Connected Agents:")
        for agent_id, queue in self.agent_message_queues.items():
            agent_name = self.get_agent_name_by_id(agent_id) or agent_id
            inputs = sum(1 for req in queue if req.is_input)
            outputs = sum(1 for req in queue if not req.is_input)
            print(f"{agent_name}: {inputs} input(s),"
                  f"{outputs} output(s) pending")

    def display_logs(self) -> None:
        print("Recent Logs:")
        # Add functionality to retrieve and display logs if necessary

    def get_agent_name_by_id(self, agent_id: Optional[str]) -> Optional[str]:
        if not agent_id:
            return None
        agent = self.host.agents.get(agent_id)
        return agent.name if agent else None

    def get_agent_id_by_name(self, agent_name: str) -> Optional[str]:
        for agent in self.host.agents.values():
            if agent.name == agent_name:
                return agent.id
        return None
