### Topic-Masking

---

### Status of this Memo

This document is a draft open for community review, seeking feedback and technical input to refine and enhance its content.

---

### 1. Introduction

This document specifies a protocol for managing access control over hierarchical topics using topic masks with predefined tokens and dynamic placeholders. TBACP provides a secure, flexible mechanism for **Authorities** to control topic access for **People**, with **Agents** acting on behalf of **People** to submit access requests.

In TBACP, **Authorities** manage topic masks for both **People** and themselves. Each topic is represented as a hierarchical path, using slashes (`/`) to separate levels (e.g., `topic1/topic2/topicN`). Masks allow for various access types (e.g., subscribe, read, write), with each level of a topic evaluated against its mask to grant or deny access.

### 1.1 Scope

- **Authority**: Manages access control by storing topic masks for **People** and itself, specifying permissions for each topic.
- **Person**: An individual with assigned masks that determine topic access.
- **Agent**: Submits access requests on behalf of a **Person** using a unified **Agent ID** that encapsulates **Authority** and **Person** identifiers.

---

### 2. Protocol Specification

#### 2.1 Topic Structure

Topics in TBACP are hierarchical, divided by slashes (`/`), with each segment representing a unique level.

Example topic path:
```
accessType:category/subcategory/item/attribute
```

#### 2.2 Topic Masks

A **topic mask** defines access control for hierarchical topics using tokens for static values, wildcards, and dynamic placeholders. Masks may be stored at the **Person** or **Authority** level, and are evaluated in that order of priority.

#### 2.3 Access Types

TBACP defines the following access types:

- **NONE (`0`)**: No access permissions.
- **SUBSCRIBE (`1`)**: Permission to subscribe to a topic.
- **READ (`2`)**: Permission to read messages or data on a topic.
- **WRITE (`4`)**: Permission to publish or modify data.

Access type identifiers can be combined:

- **3** = SUBSCRIBE + READ
- **5** = SUBSCRIBE + WRITE
- **6** = READ + WRITE
- **7** = SUBSCRIBE + READ + WRITE

---

### 3. Mask Tokens and Wildcards

TBACP uses tokens to define flexible access policies:

| Token            | Symbol | Description                                                                 |
|------------------|--------|-----------------------------------------------------------------------------|
| **ALL**          | `0`    | Matches all values at this level, allowing unrestricted access. |
| **NONE**         | `-`    | Denies access at this level.   |
| **ANY_EXCLUSIVE**| `*`    | Matches only if the request segment is a wildcard itself, not a specific value. For example, `*/topic1/topic2` will match `*/+/+` but not `topic0/topic1/topic2`. |
| **ANY_INCLUSIVE**| `+`    | Matches any specified value, allowing access to multiple specific identifiers.  |
| **DYNAMIC**      | `?`    | A placeholder for dynamic matching, resolved at runtime. |

#### 3.1 Mask Matching

Masks are matched against topic requests to determine access. Tokens provide flexibility in exact values, wildcards, and dynamic evaluation.

Examples:

- **`0/subcategory/0`**: Allows access to any values at the first and third levels, with `subcategory` fixed at the second level.
- **`*/+/–`**: Matches any wildcard at the first level, any value at the second, and denies access at the third.
- **`?/subcategory/?`**: Dynamically matches values at the first and third levels, while keeping `subcategory` fixed.

---

### 4. Protocol Workflow

#### 4.1 Access Control Request

An **Agent** submits an **Access Control Request (ACR)** on behalf of a **Person**. The request includes:

- **topic**: The hierarchical topic requested.
- **accessType**: Integer for the requested access type (e.g., READ = 1).
- **agentId**: Unified identifier string for the **Agent**, encapsulating **Authority** and **Person** identifiers (e.g., `agi://agent:person@authority`).

The **Authority** checks stored topic masks to evaluate permissions for the **Person** and the **Authority**.

#### 4.2 Mask Evaluation Process

1. **Retrieve Masks**: Based on `agentId` and `accessType`, retrieve applicable masks:
   - Check masks assigned to the **Person** first.
   - If no match, check masks assigned to the **Authority**.
2. **Match Topic**: Evaluate each mask pattern against the topic path:
   - Static values must match topic segments exactly.
   - Tokens (`0`, `*`, `+`, `?`) are evaluated according to Section 3.
3. **Dynamic Resolution (`?`)**: Resolve any `?` placeholder based on runtime values in the topic path.
4. **Access Decision**: If any mask grants access, return `ALLOW` (TRUE or `0`); otherwise, return `DENY` (FALSE or `1`).

#### 4.3 Example Access Control Request

**Request**:
```json
{
  "topic": "*/subcategory/item",
  "accessType": 1,
  "agentId": "agi://agent:person@authority"
}
```

**Example Masks**:
- **Person Level Mask**: `7:*/subcategory/0`
- **Authority Level Mask**: `5:+/subcategory/-`

If the request’s `accessType` is **READ**, the **Person Level Mask** `*/subcategory/0` grants access. 

Multiple records can merge into a cumulative access result.

---

### 5. Security Considerations

TBACP masks may contain sensitive information in dynamic queries (`?`). All queries must be securely handled to prevent injection attacks. Logs of access control decisions should be maintained for traceability.

---

### 6. Example Usage

Suppose a given **Authority** allows any **Person** to read topics associated with specific categories and items. However, specific **People** may be granted additional permissions, such as writing to particular topics.

| Record               | Mask                     | Description                                       |
|----------------------|--------------------------|---------------------------------------------------|
| **@authority**       | `2:categoryId/itemId/+`  | Any **Person** in the named **Authority** can read any topic under `categoryId/itemId`. |
| **person@authority** | `3:categoryId/itemId/topic2` | Only `person@authority` can write to topics under `categoryId/itemId/topic2`. |

### 6.1 Assigning and Storing Masks

The **Authority** maintains a table of topic masks associated with each **Person** and **Authority**. This table includes:

- **agentId**: Identifier for each **Agent**, encapsulating **Authority** and **Person** details.
- **Masks**: List of topic masks with associated access types (READ, WRITE, SUBSCRIBE).

Example:
```plaintext
+----------------------------------------+------------------+-----------------------------+
| agentId                                | AccessType       | Mask                        |
+----------------------------------------+------------------+-----------------------------+
| agi://agent:person@authority           | READ             | 0/subcategoryId/0             |
| agi://agent:person@authority           | WRITE            | categoryId/+/itemId  |
+----------------------------------------+------------------+-----------------------------+
```

#### 6.2 Mask Matching Logic

When an **Agent** submits an access request with an **agentId**, the **Authority** checks stored masks for both **Person** and **Authority** levels, retrieves all relevant masks, and applies the mask evaluation process in Section 4.2.

---

### 7. Error Codes

**Standard Errors**:
- **400 Bad Request**: Invalid access control request parameters.
- **401 Unauthorized**: Access denied due to insufficient permissions.
- **403 Forbidden**: Forbidden topic restriction.
- **500 Internal Server Error**: Processing error.

---