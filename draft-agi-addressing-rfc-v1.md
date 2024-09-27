# Internet-Draft: Address Resolution for the `agi://` Protocol Using Existing Protocols

### Status of this Memo

This document is an Internet-Draft submitted in full conformance with the provisions of BCP 78 and BCP 79. Internet-Drafts are working documents of the Internet Engineering Task Force (IETF) and its working groups. Other groups may also distribute working documents as Internet-Drafts. This Internet-Draft will expire six months after publication.

### Abstract

This document defines a method for resolving addresses in the `agi://` scheme by rerouting to existing protocols such as HTTPS. The resolution is driven by DNS requests and supports validation through DKIM and JWT signatures. The scheme supports hierarchical address resolution for hosts, agents, topics, and persons, allowing flexible and unambiguous entity identification within a system. The format intentionally resembles email addresses, enabling easy integration with existing infrastructure.

---

## Table of Contents

1. Introduction
2. Address Resolution Model
   - Address Structure
   - Resolution Process and Example
3. Schema Location and Protocol Mapping
4. Validation Using JWT and DKIM
5. Handling Ambiguities and Hierarchies
6. Security Considerations
7. IANA Considerations
8. References

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
- `{prefix}` (optional): Denotes the entity type (e.g., `p:` for person, `a:` for agent, `h:` for host, `t:` for topic).
- `{entity}`: The unique identifier for the person, agent, topic, or host.
- `{authority}`: The domain or authority responsible for resolving the entity.

### Resolution Process and Example

For an address such as `agi://host1@example.com`, the resolution follows these steps:

1. **DNS Lookup**:  
   The system performs a DNS request for the authority specified in the address. The DNS query is expected to return a `TXT` record that contains a resolution string. For example:
   
   ```
   "agi v1 https://{authority}/agi/[{entity-type}/]{entity-name}"
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

In cases where the `agi` protocol needs to reroute to protocols other than HTTPS, the authority can define an appropriate schema and protocol in the DNS response. This allows flexibility in supporting multiple protocols, depending on the type of entity or resource being addressed.

The DNS response can specify one or more protocol mappings, and the system will attempt to resolve the address by trying each protocol in sequence.

Example of a DNS response for alternative protocol mapping:

```
"agi v1 https://example.com/api/entity ftp://example.com/ftp/{entity-name}"
```

In this case, the system first attempts to resolve using HTTPS and, if unavailable, attempts to use FTP.

---

## 4. Validation Using JWT and DKIM

In this system, **JWT (JSON Web Tokens)** are used for entity authentication, allowing a person, agent, topic, or host to identify itself. Each entity obtains a JWT from the **authority** managing the system and presents the token in its requests, typically in the HTTP header as a Bearer token.

### JWT Issuance and Verification
- **JWT Issuance**: Each person, agent, topic, or host receives a JWT from the authority, signed by the authority’s private key.
- **JWT Usage**: The JWT is included in the request (e.g., in the Authorization header as a Bearer token) to authenticate the entity making the request.
- **JWT Validation**: The receiving system validates the JWT using the authority's public key to confirm the authenticity and integrity of the token.

### DKIM for JWT Signature Validation

**DKIM (DomainKeys Identified Mail)** is used to validate the signature of the JWT. The JWT is signed by the authority's private key, and the receiving party uses DKIM to verify the signature using the authority's public key. This guarantees that:
- The JWT was issued by the correct authority.
- The JWT has not been tampered with during transmission.

For example, if `agi://agent1@example.com` is making a request, it presents a JWT in the request header. The receiving party can validate the JWT using the authority’s public key via DKIM, ensuring the request comes from the authorized agent and that the agent’s identity is authentic.

---

## 5. Handling Ambiguities and Hierarchies

### Non-Typed Address Resolution

Non-typed addresses (e.g., `alice@example.com`) are treated flexibly, similar to email addresses. If the entity `alice` happens to be a person, it will resolve under the `person` hierarchy. If there are multiple possible entities (e.g., both an agent and a person named `alice`), the resolver will check entities in the following order:

```
{h}{t}{a}{p}@{o} 
```

This ensures that all possible entity types are checked, avoiding ambiguity.

#### Example:
Address: `host1:alice@example.com`

1. The system first looks for `alice` under the `p:` (person) prefix.
2. If found, it checks for `host1` under the remaining hierarchies (`a:`, `t:`, `h:`).
3. Once `host1` is found under `h:`, the address is resolved as:

```
host = host1, person = alice, authority = example.com
```

### Multiple Entity Addresses

Entities may have multiple addresses, similar to email aliases. It is up to the authority to define the mapping and resolution rules for these aliases. The system does not limit the number of addresses assigned to a single entity, providing flexibility.

---

## 6. Security Considerations

Security is critical in this proposal. The following measures ensure secure resolution:

1. **DKIM Validation**:  
   Authorities sign DNS responses using DKIM, ensuring the authenticity of the response.

2. **JWT Validation**:  
   Entities present JWTs that are validated against the authority’s public key, ensuring secure and authenticated access to resources.

3. **Path and Query Parameters**:  
   All path and query parameters should be sanitized and validated by the authority to prevent injection attacks or other forms of exploitation.

---

## 7. IANA Considerations

This document proposes the registration of the `agi` protocol scheme and updates to relevant DNS `TXT` record standards to support the `agi://` resolution model.

---

## 8. References

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