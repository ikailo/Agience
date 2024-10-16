# Agience: Intelligent Agents for Everyone

**Agience** is an **open-source** intelligent **agent framework** and **platform** that intuitively and seamlessly **connects devices**, **systems**, and **data** to achieve new levels of **automation** and **efficiency**.

This open-source project enables anyone to create and deploy intelligent agents capable of automating tasks, processing complex information, and facilitating communication between devices and systems with scalability and reliability.

## Highlights

- **Scalable and Reliable**: With its distributed architecture, Agience easily adapts to personal, business, industrial, and enterprise-level applications of any size and scope.

- **Agent Compute Units**: Deploy scalable, autonomous agents equipped with specialized functions to handle data, automate processes, and communicate easily across networks.
 
- **Seamless Integration**: Agience is adaptable to any addressable protocol, ensuring reliable connectivity across systems.
 
- **Open-Source**: Agience is released under the LGPLv3 license, allowing anyone to use, modify, and distribute the software. You can build anything using Agience, but any modifications to the framework or platform must be shared with the community for the benefit of humanity.

## Framework Architecture

The Agience Framework standardizes communication between agents, hosts, and their authority, facilitating synchronization and efficient resource management across the network.

### Host
The **Host** application is deployed on devices or systems, enabling agents to interact with local services and resources.

### Agents and Agencies
**Agents** are autonomous units that perform specific tasks, gather data, and communicate with other agents. Connected agents form an **agency**, allowing for coordinated tasks and shared access to data and services.

### Functions and Plugins
**Functions** are predefined tasks or behaviors that agents perform, such as controlling external systems, analyzing data, or facilitating communications. **Plugins** extend agent capabilities by bundling related functions.

### Authority
The **authority** manages governance, resources, identity, and metadata for hosts and agents. It also facilitates and enforces sharing and ownership within its own domain and with other authorities.

## Getting Started

Sign-up: https://web.preview.agience.ai

### Prerequisites

Visual Studio 2022

### Installation

**Instructions:** /Build/Docs/agience-preview-connect.pdf

git clone https://github.com/ikailo/Agience.git

- In Agience.Hosts.Console User-Secrets, add HostId, HostSecret, AuthorityUri, and OpenAiApiKey.
	- In the web preview user interface. Create a host and then generate a key.
	- AuthorityUri: https://authority.preview.agience.ai
	- Get a key from OpenAI.

- In web preview, setup an agency and agent, and assign it to the same host.
- You can add plugins and functions according to the instructions in the pdf doc.

### Running in Docker

Coming Soon.

# AGI Addressing RFC Proposal

This repository hosts the draft of an IETF proposal for the `agi://` protocol, which enables the resolution of agent, host, person, and topic addresses by rerouting to existing protocols such as HTTPS, validated using DKIM and JWT.

## Purpose

The goal of this proposal is to provide a standardized method for resolving `agi://` addresses while integrating with existing email and web-based protocols.

## Current Draft

The current draft can be found in the `/drafts` folder:  
- [Draft Version 1](drafts/draft-agi-addressing-rfc-v1.md)

## How to Contribute

We welcome feedback from the community! Please feel free to:

- **Submit Issues**: For suggestions, bugs, or questions.
- **Create Pull Requests**: For proposed changes or improvements to the RFC or associated documentation.

## License

This repository contains various types of content, each covered by different licenses as described below. The **core software** is available under a **dual-license model**: a combination of the **GNU Affero General Public License v3.0 (AGPL-3.0)** and a **commercial license** for specific use cases.

## Core Software (Dual License: AGPL-3.0 and Commercial)

### AGPL-3.0 License

The core software in **a `/src` directory** is licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**. This license allows for:

- **Open-Source Use**: You may copy, modify, and distribute the software, provided any public use over a network ensures that the modified source code is also made available under the AGPL-3.0.
- **Private Modifications**: You may make modifications and use the software privately, without sharing the source code, as long as it is not made publicly accessible over a network.

A full copy of the AGPL-3.0 license can be found in the `LICENSE-AGPL-3.0.txt` file.

[Learn more about AGPL-3.0](https://www.gnu.org/licenses/agpl-3.0.html).

### Commercial License

For commercial entities, enterprises, or those seeking to make proprietary modifications without making the source code publicly available, a commercial license is required. The commercial license applies under the following conditions:

- **Revenue Cap**: If your organization exceeds **$1M USD in revenue**, you are required to obtain a commercial license.
- **Private Use for Corporations**: If you are making proprietary modifications to the software and do not intend to share the modifications publicly, you must obtain a commercial license.
- **Redistribution**: If you redistribute the software as part of a proprietary service or product, a commercial license is required.
- **Non-Profit and Educational Use**: Non-profit organizations and educational institutions may use the core software under the AGPL-3.0 without a commercial license.

To obtain a commercial license, please contact licensing@agience.ai.

A full copy of the commercial license can be found in the `LICENSE-COMMERCIAL.txt` file.

## Example Code (MIT License)

All example code in **a `/samples` directory** (e.g., `/dotnet/samples`, `/python/samples`) is licensed under the **MIT License**. This allows you to use, copy, modify, merge, publish, and distribute the example code freely, provided that the copyright notice and this permission notice are included in all copies or substantial portions of the software.

A full copy of the MIT license can be found in the `LICENSE-MIT.txt` file.

[Learn more about the MIT License](https://opensource.org/licenses/MIT).

## Documentation (Creative Commons Attribution 4.0 International)

All documentation in **a `/docs` directory**, as well as any supplementary materials, are licensed under the **Creative Commons Attribution 4.0 International (CC BY 4.0)** license. You are free to share and adapt the documentation for any purpose, even commercially, as long as appropriate credit is given.

A full copy of the CC BY 4.0 license can be found in the `LICENSE-CC-BY-4.0.txt` file.

[Learn more about CC BY 4.0](https://creativecommons.org/licenses/by/4.0/).

## RFC Proposal License

The IETF RFC proposal drafts in **a `/ietf-drafts` directory** are subject to the **IETF Trust's Legal Provisions Relating to IETF Documents**. The documents are submitted as Internet-Drafts and are part of the IETF's contribution process. You may freely copy and redistribute the draft for review and comment, but they are provided "as-is" without warranties.

For further details, refer to the [IETF Trust Legal Provisions](https://trustee.ietf.org/license-info/).

## Unspecified Code and Content

Any code or content outside the defined directories (e.g., `/src`, `/samples`, `/docs`, `/ietf-drafts`) defaults to the **GNU Affero General Public License v3.0 (AGPL-3.0)** unless otherwise explicitly stated. This ensures that any undefined components of the project remain under a default license that is consistent with the core.