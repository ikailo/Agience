# Draft: Address Resolution for the `agi://` Protocol Using Existing Protocols

### Status of this Memo

This document is a draft intended for community review. We are seeking feedback and technical input from the community to refine and improve its content. The draft is open for contributions, and we welcome suggestions from all interested parties.

--- 

### Abstract

This document defines a method for resolving addresses in the `agi://` scheme by rerouting to existing protocols. The resolution is driven by DNS requests and supports validation through DKIM and JWT signatures. The scheme supports hierarchical address resolution for hosts, agents, topics, and persons, allowing flexible and unambiguous entity identification within a system. The format intentionally resembles email addresses, enabling easy integration with existing infrastructure.

(NOTE: We need more information here about why we need this. Benefits, why 'agi', what it means, what it enables..)

---

## Table of Contents

1. Introduction
2. Address Resolution Model
   - Address Structure
   - Resolution Process and Example
3. Schema Location and Protocol Mapping
4. Validation Using JWT and DKIM
5. Handling Ambiguities and Hierarchies
6. IANA Considerations
7. References

---

## 1. Introduction

The `agi://` protocol is designed for agent-based systems to address and interact with various entities, including persons, agents, topics, and hosts. This document introduces a mechanism for resolving `agi://` addresses into existing protocols, primarily using DNS requests. The system supports hierarchical paths to differentiate between multiple entities sharing the same authority.

By utilizing email-like address structures and integrating validation mechanisms like DKIM and JWT, the `agi://` scheme ensures secure and efficient resolution while remaining compatible with existing technologies.

---

## 2. Address Resolution Model

### Address Structure

An `agi://` address follows the pattern:

```
agi://{[prefix:]}{entity}@{authority}
```

Where:
- `{prefix}` (optional): Denotes the entity type (e.g. `p:` for person, `a:` for agent, `t:` for topic, `h:` for host, `g:` for plugin, `f:` for function).
- `{entity}`: The identifier for the agent, topic, host, plugin, function.
- `{authority}`: The domain or authority responsible for resolving the entity.

### Resolution Process and Example

For an address such as `agi://host1@example.com`, the resolution follows these steps:

1. **DNS Lookup**:  
   The system performs a DNS request for the authority specified in the address. The DNS query is expected to return a `TXT` record that contains a resolution string. For example:
   
   ```
   "agi v1 https://{authority}/agi/[{entity-type}/]{entity-name}?{query}"
   ```
   
   Here, `{entity-type}` is included in the final URL if specified in the original address; otherwise, it can be inferred from context.

2. **Rewrite the Request**:  
   The address `agi://host1@example.com` is rewritten to:
   
   ```
   https://example.com/agi/host1
   ```

3. **Path and Query Parameters**:  
   If additional query parameters or a path are provided, they are appended to the resolved address. For instance:
   
   ```
   https://example.com/agi/host1?param=value
   ```

---

## 3. Schema Location and Protocol Mapping

In cases where the `agi` protocol needs to reroute to different protocols, the authority can define an appropriate schema and protocol in the DNS response. This allows flexibility in supporting multiple protocols, depending on the type of entity or resource being addressed.

The DNS response can specify one or more protocol mappings, and the system will attempt to resolve the address by trying each protocol in sequence.

Example of a DNS response for alternative protocol mapping:

```
"agi v1 wss://{authority}/[{entity-type}/]{entity-name} https://{authority}/agi/[{entity-type}/]{entity-name}?{query} "
```

In this case, the system first attempts to resolve using WSS and, if unavailable, attempts to use HTTPS.

---

## 4. Validation Using JWT and DKIM

In this system, **JWT (JSON Web Tokens)** are used for entity authentication, allowing an entity to identify itself. Each entity obtains a JWT from the **authority** managing the system and presents the token in its requests, typically in the HTTP header as a Bearer token. Requests are proxied to the entity by the **host**, so a JWT should never be inspected by any entity other than an **authority** or **host**. An entity should only present its JWT token to the **host** when it is connecting with another entity within the same **authority** as the JWT issuer.

### JWT Issuance and Verification
- **JWT Issuance**: Entities receive a JWT from the authority, signed by the authority’s private key.
- **JWT Usage**: The JWT is included in the request (e.g., in the Authorization header as a Bearer token) to authenticate the entity making the request.
- **JWT Validation**: The receiving host validates the JWT using the authority's public key to confirm the authenticity and integrity of the token.

### DKIM for JWT Signature Validation

**DKIM (DomainKeys Identified Mail)** is used to validate the signature of the JWT. The JWT is signed by the authority's private key, and the receiving party uses DKIM to verify the signature using the authority's public key. This guarantees that:
- The JWT was issued by the correct authority.
- The JWT has not been tampered with during transmission.

For example, if `agi://agent1@example.com` is making a request, it presents a JWT in the request header. The receiving host can validate the JWT using the authority’s public key via DKIM, ensuring the request comes from the authorized agent and that the agent’s identity is authentic.

---

## 5. Handling Ambiguities and Hierarchies

### Non-Typed Address Resolution

Non-typed addresses (e.g., `alice@example.com`) are treated flexibly, similar to email addresses. If the entity `alice` happens to be a person, it will resolve under the `person` hierarchy. The resolver will check entities in **right to left** order:

```
{function}:{plugin}:{host}:{topic}:{agent}:{person}@{authority} 
```

This ensures that all possible entity types are checked, avoiding ambiguity.

#### Example:
Address: `host1:alice@example.com`

1. The system first looks for `alice` under the `p:` (person) prefix.
2. If found, it checks for `host1` under the remaining hierarchies, in order: (`a:`, `t:`, `h:`, `g:`, `f:`).
3. Once `host1` is found under `h:`, the address is resolved as:

```
host = host1, person = alice, authority = example.com
```

### Multiple Entity Addresses

Entities may have multiple addresses, similar to email aliases. It is up to the authority to define the mapping and resolution rules for these aliases. The system does not limit the number of addresses assigned to a single entity, providing flexibility and anonymity.

---

## 6. IANA Considerations

This document proposes the registration of the `agi` protocol scheme and updates to relevant DNS `TXT` record standards to support the `agi://` resolution model.

---

## 7. References

- [RFC 5321] Simple Mail Transfer Protocol (SMTP)
- [RFC 6376] DomainKeys Identified Mail (DKIM)
- [RFC 7519] JSON Web Token (JWT)
- [RFC 8141] Uniform Resource Names (URNs)

---

## Author's Address

**Name:** John Sessford  
**Organization:** Ikailo Inc.  
**Email:** john@ikailo.com  
**GitHub:** [https://github.com/john-s4d](https://github.com/john-s4d)
