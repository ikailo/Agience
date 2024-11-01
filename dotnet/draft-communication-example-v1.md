### Example Scenario: Scheduling a Call between Alice and Bob

**Authorities:**  
Alice and Bob are people. Each are associated with a different authority.
 - Alice is in authority "aa"
 - Bob is in authority "bb"

**Topics:**
Both Alice and Bob have their main topics where they can communicate with their agents.
 - Alice's main topic is `"aa:alice/main"`
 - Bob's main topic is `"bb:bob/main"`
	
- Alice's agent, Astra, is assigned to the topic `"aa:alice/main"`.

Bob has published two topics related to his calendar:
  - `bob/calendar/events/add | Add events to Bob's calendar.`
  - `bob/calendar/events/view| View events in Bob's calendar.`

For privacy and access control, Bob has assigned different agents to each topic. This provides a separation of concerns and ensures that agents can only perform specific actions based on the topic they are assigned to.
 
 - **Bob’s Agent "Bolt"** is assigned to topic `"bob/calendar/events/add"` and can add events to Bob's calendar.
 - **Bob’s Agent "Cortex"** is assigned to topic `"bob/calendar/events/view"` and can retrieve events from Bob's calendar.
 
**Access Control:**  
Bob has previously granted Alice limited access to his calendar with the following topic masks:
  - `alice@ae.example - 7:bob/calendar/+/add` — Allows Alice to request additions of any type of event to Bob's calendar.
  - `alice@ae.example - 0:bob/calendar/+/view` — Denies Alice the ability to directly view any type of event on Bob’s calendar.

**Objective:**  
Alice wants to schedule a call with Bob.

**Agents Involved:**  
- **Alice’s Agent "Astra"** has full access to Alice's calendar and will assist with the scheduling request.
- **Bob’s Agents "Bolt" and "Cortex"** will handle the scheduling request and calendar access.
- **Bob’s Agent "Delta"** will notify Bob of the scheduled call.

### Communication Flow

The following communication channels are established among Alice, Bob, and their agents:

| Party 1   | Topic                          | Party 2   |
|-----------|--------------------------------|-----------|
| Alice     | `aa:alice/main`                | Astra     |
| Astra     | `bb:bob/calendar/events/add`   | Bolt      |
| Bolt      | `bb:bob/calendar/events/view`  | Cortex    |
| Bolt      | `bb:bob/main/incoming`         | Delta     |
| Delta     | `bb:bob/main`                  | Bob       |

**Dialogue and Information Exchange**

| Speaker | Topic                       | Message                                                                                            |
|---------|------------------------------|---------------------------------------------------------------------------------------------------|
| Alice   | `aa:alice/main`                 | "Please schedule a call with Bob for next week."                                               |
| Astra   | `aa:alice/main`                 | "I'm on it."                                                                                   |
| Astra   | `bb:bob/calendar/events/add`    | "I'd like to schedule a call between Bob and Alice for next week."                             |
| Bolt    | `bb:bob/calendar/events/add`    | "Sure, let me check Bob's availability."                                                       |
| Bolt    | `bb:bob/calendar/events/view`   | "Alice would like to schedule a call for next week. When is Bob available?"                    |
| Cortex  | `bb:bob/calendar/events/view`   | "Bob is available on Tuesday at 10:00 or Thursday at 14:00. Timezone is GMT-4."                |
| Bolt    | `bb:bob/calendar/events/add`    | "Bob is available on Tuesday at 10:00 or Thursday at 14:00. Timezone is GMT-4."                |
| Astra   | `bb:bob/calendar/events/add`    | "Great! Schedule it for Tuesday at 10:00, please."                                             |
| Bolt    | `bb:bob/calendar/events/add`    | "Confirmed: Tuesday at 10:00."                                                                 |
| Astra   | `aa:alice/main`                 | "I scheduled a call with Bob for 10:00 on Tuesday."                                            |
| Bolt    | `bb:bob/main/incoming`          | "Alice has scheduled a call with you for 10:00 on Tuesday."                                    |
| Delta   | `bb:bob/main`                   | "Alice has scheduled a call with you for 10:00 on Tuesday."                                    |


**Summary:**  
Alice initiates the scheduling request through her agent, Astra, who communicates with Bob's agent, Bolt, to inquire about Bob’s availability. Bolt then verifies availability through Cortex, who can access Bob’s calendar. Once confirmed, Astra relays the scheduled time back to Alice, and Bolt posts the confirmation to Bob's incoming topic, where Delta will consolidate messages and notify Bob of the important details.