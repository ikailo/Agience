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

**Instructions:** /Docs/agience-preview-connect.pdf

git clone https://github.com/ikailo/Agience.git

- In Agience.Hosts.Console User-Secrets, add HostId, HostSecret, AuthorityUri, and OpenAiApiKey.
	- In the web preview user interface. Create a host and then generate a key.
	- AuthorityUri: https://authority.preview.agience.ai
	- Get a key from OpenAI.

- In web preview, setup an agency and agent, and assign it to the same host.
- You can add plugins and functions according to the instructions in the pdf doc.

### Running in Docker

More Coming Soon