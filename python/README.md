# Agience Application Flow

## Detailed Application Flow
The application is a distributed, event-driven system built around modular components, each designed to perform specific tasks and interact via a message broker using the MQTT protocol. The system is designed to manage hosts, agencies, agents, and plugins, creating a scalable and dynamic environment.

## Key Concepts
1. **Authority**: The central controller responsible for authentication, configuration, and broker topic management.
2. **Host**: Represents a physical or logical entity that connects to the system to manage agents and agencies.
3. **Agency**: A group that organizes agents, manages their lifecycle, and coordinates their activities.
4. **Agent**: A unit of execution capable of running tasks, interacting with plugins, and participating in agencies.
5. **Broker**: The communication layer facilitating messaging between components using MQTT.
6. **AgentFactory**: A dynamic agent creation and management system that assigns plugins to agents.
7. **Plugins**: Extend agent functionality dynamically, enabling task execution and enhanced capabilities.

## Phase-by-Phase Application Flow

### Phase 1: System Initialization

#### 1. Authority Initialization:
- The Authority is the starting point of the system. It initializes itself by:
  - Fetching its configuration from the OpenID endpoint.
  - Retrieving the Broker URI and Token Endpoint.
  - Preparing MQTT topics for hosts, agencies, and agents.

**Key Method:**
```python
async def initialize_with_backoff(self, max_delay_seconds=16):
    # Exponential backoff to initialize authority
```

#### 2. Broker Initialization:
- The Broker connects to the specified MQTT server, synchronizes its time using NTP servers, and prepares for subscriptions.

#### 3. Host Initialization:
- A Host connects to the authority and broker by:
  - Fetching an access token using client credentials (`host_id` and `host_secret`).
  - Subscribing to system-wide and host-specific topics via the broker.
  - Publishing a `host_connect` event to notify the system of its presence.

**Example Event:**
```json
{
    "type": "host_connect",
    "timestamp": "2024-01-01T12:00:00Z",
    "data": {
        "host": {
            "id": "host123",
            "name": "MyHost"
        }
    }
}
```
- Other components react to this event (e.g., Authority might respond with a `host_welcome`).

### Phase 2: Host Lifecycle Management

#### 1. Host Connect:
- The Host listens for lifecycle events (e.g., `host_welcome`) via the broker.

#### 2. Event Flow:
- Authority responds to `host_connect` by publishing `host_welcome`.
- The Host processes `host_welcome`, retrieves information about agents, plugins, and agencies, and initializes them.

#### 3. Host Disconnect:
- When the Host disconnects, it:
  - Unsubscribes from MQTT topics.
  - Disconnects all agents and agencies it manages.
  - Notifies the system of its shutdown.

### Phase 3: Agent and Agency Lifecycle

#### 1. Agent Joins:
- An Agent publishes a `join` event when it connects to an agency.
- The Agency processes the event, updates its internal state, and may broadcast a welcome message.

**Event Example:**
```json
{
    "type": "agent_connect",
    "timestamp": "2024-01-01T12:01:00Z",
    "data": {
        "agent": {
            "id": "agent001",
            "name": "MyAgent",
            "agency": { "id": "agency123" }
        }
    }
}
```

#### 2. Agent Role Claim:
- Agents may claim special roles, such as `representative_claim`, by publishing corresponding events.
- The Agency ensures only one agent can hold a role and updates its state accordingly.

#### 3. Agent Leaves:
- An agent publishes a `leave` event when it disconnects.
- The Agency processes the event, removes the agent, and updates its state.

### Phase 4: Dynamic Plugin Management

#### 1. Plugin Assignment:
- The AgentFactory dynamically assigns plugins to agents when they are created.
- Plugins may be:
  - Compiled: Preloaded as classes (e.g., imported Python modules).
  - Curated: Created dynamically from metadata (e.g., JSON-defined behaviors).

#### 2. Plugin Execution:
- Plugins extend agent functionality by enabling them to:
  - Execute tasks.
  - Respond to prompts.
  - Interact with external systems.

**Example Plugin Metadata:**
```json
{
    "name": "GreetPlugin",
    "functions": [
        {
            "name": "greet",
            "description": "Responds with a greeting",
            "prompt": "Say hello in a friendly tone."
        }
    ]
}
```

### Phase 5: Event Processing

#### 1. Broker-Driven Communication:
- All entities communicate through the broker.
- Events like `host_welcome`, `agent_connect`, and `agent_disconnect` are processed based on their type.

#### 2. Example Processing:
In the Host:
```python
async def _broker_receive_message(self, message: dict):
    if message["type"] == BrokerMessageType.EVENT:
        if message["data"]["type"] == "agent_connect":
            await self._receive_agent_connect(message["data"]["agent"])
```

#### 3. Callbacks:
- Events trigger callbacks, enabling application-level actions.

### Phase 6: Shutdown

#### 1. Host Shutdown:
- The Host:
  - Disconnects from the broker.
  - Terminates all agents and agencies.
  - Publishes a `host_disconnect` event.

#### 2. System Cleanup:
- Resources are cleaned up, ensuring the application shuts down gracefully.

## Detailed Class Interactions

| Class | Responsibilities | Interacts With |
|-------|-----------------|----------------|
| Authority | Central orchestrator; manages authentication, configuration, and topics. | Broker, Host, Agency |
| Broker | MQTT communication layer; facilitates message publishing/subscription. | Authority, Host, Agency, Agent |
| Host | Represents a central entity; manages agents, agencies, and events. | Authority, Broker, AgentFactory |
| Agency | Manages agent lifecycles; coordinates roles and activities. | Host, Broker, Agent |
| Agent | Executes tasks and interacts with plugins; participates in agency events. | Broker, Agency, Plugin |
| AgentFactory | Dynamically creates agents and assigns plugins. | Host, Plugin |
| Plugins | Extend agent functionality dynamically. | AgentFactory, Agent |