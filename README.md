# Agience: Intelligent Agents for Everyone

**Agience** is an **open-source** intelligent agent platform that enables anyone to easily **build**, **deploy**, and **manage** intelligent agents capable of automating tasks, processing complex information, and facilitating communication between devices and systems with scalability and reliability.

## Highlights

- **Distributed Architecture**: Agience hosts can be deployed anywhere, ensuring data residency compliance for applications across personal, business, industrial, and enterprise levels of any size and scope.

- **Modular Agents**: Scalable, autonomous agents encapsulate their functions and data with the flexibility to move seamlessly between hosts.

- **Agent Agnostic**: Agience is designed to support any type of agent, with initial compatibility for Semantic Kernel and plans to expand to all agent frameworks.

- **Open-Source**: Released under the AGPL-3.0 license, Agience allows anyone to use, modify, and distribute the software. Build anything using Agience, with modifications to the framework shared for the collective benefit.

## Architecture

Agience is a distributed intelligent agent platform consisting of a network of authorities that provide essential services to their respective communities. These services include identity management, message brokering, streaming, databases, and management APIs, all supporting secure, coordinated interactions across a network of hosts, agents, and topics, each governed by its respective authority.

A host, deployed on devices or systems, offers computational resources and manages the flow of information between agents and topics under the guidance of its authority. Agents, acting as digital representatives of individuals, perform tasks and exchange information through topics while respecting the boundaries established by their authority.

Each authority governs the permissions and interactions of its agents, hosts, and topics, with JWT-based authorization managed by an identity provider (IDP). Authorities can establish trusted relationships, allowing agents from different authorities to interact across boundaries when authorized. This structure ensures that agents operate securely within defined scopes and adhere to permissions across the Agience ecosystem.

## Roadmap

Review the [Roadmap](ROADMAP.md) for upcoming features and improvements, and to see where you can contribute.

--- 

# Get Started

Agience is designed to be accessible to everyone, from developers and hobbyists to businesses and enterprises. You can explore the platform through the Preview Instance or set up a Development Instance to start building and deploying agents.

****NOTE: Agience is currently in the early stages of development with new features and improvements being added regularly. We welcome feedback and contributions from the community to help shape the future of the platform.****

### Preview Instance

Visit the [Agience Preview Instance](https://preview.agience.ai) to explore the platform and to start building and deploying agents.
 
### Development Instance

Prerequsites:
  - [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/)
  - [dotnet-ef](https://www.nuget.org/packages/dotnet-ef)
  - [GIT](https://www.git-scm.com/downloads)
  - [Docker](https://docs.docker.com/desktop/install/windows-install/)
  - [OpenSSL](https://slproweb.com/products/Win32OpenSSL.html)

1. **Clone the Repository**: Start by cloning the Agience repository to get access to the source code and configuration files.
   ```bash
   git clone https://github.com/ikailo/Agience.git
   ```

2. **Navigate to the Build Directory**: Change into the build directory within the Agience stack. This directory contains configuration and startup scripts for setting up Agience.
   ```bash
   cd Agience/dotnet/src/Authority/Build/development
   ```

3. **Run Initialization Script**: Execute the initialization batch file to set up essential dependencies and configurations.
   ```bash
   .\agience-init.bat
   ```
   Accept any prompts that appear during the initialization process.

#### Complete Configuration

After running the initialization script, you’ll need to complete a few outstanding configurations in the `.env.development` files. These files are now located within the `Agience.Authority.Manage` and `Agience.Authority.Identity` directories and control API keys and other credentials.

**NOTE**: The `agience-init.bat` script is succesful if the final output is:
```plaintext
  Updated HostId and HostSecret in the file successfully.
```
If it fails, you will need to correct the errors and run `agience-remove.bat` to reset the configuration. PLEASE reach out via [email](mailto:connect@agience.ai) or [Discord](https://discord.gg/fyWWqzeUKH) for real-time assistance.

1. **Edit Agience.Authority.Manage Environment File**

  - Obtain an [OpenAI API key](https://help.openai.com/en/articles/4936850-where-do-i-find-my-openai-api-key) and set it in the `.env.development` file located in `Agience.Authority.Manage`.
   ```plaintext   
   HostOpenAiApiKey=<Your_OpenAI_API_Key>
   ```

2. **Edit Agience.Authority.Identity Environment File**  

  - Obtain [Google OAuth2.0 Credentials](https://developers.google.com/identity/protocols/oauth2/web-server#creatingcred) for Server-side Web Apps.
	- Enter `https://localhost:5001` for Authorized JavaScript origins.
	- Enter `https://localhost:5001/signin-google` for Authorized redirect URIs.

	  Additional option for **local** builds (see below):
	- Enter `https://authority.local.agience.ai` for Authorized JavaScript origins.
	- Enter `https://authority.local.agience.ai/signin-google` for Authorized redirect URIs.

   In the `.env.development` file located in `Agience.Authority.Identity`, set the following:
   ```plaintext   
   GoogleClientId=<Your_Google_Client_ID>
   GoogleClientSecret=<Your_Google_Client_Secret>
   ```

   These credentials enable Google authentication for Agience.

#### Start Docker Services

Once the configuration is complete, you can start the necessary Docker services for Agience by running the following command:
```bash
.\docker-up.bat
```
This script will bring up the required Docker containers, such as the database, message broker, and any other necessary services specified in the configuration.

---

### Local Build

The development build accesses the Agience stack on the `https://localhost:<port>` domain. This is good for debugging and testing locally.

You can also run it locally using the `*.local.agience.ai` domains. This is an example of how it is intended to be hosted, however it does not support debugging directly.

Run the `.\agience-init.bat` from the `Build\local` directory.  Then update the `\.ent.local` configuration files as above. Finally, run `.\docker-up.bat` to start the services.


### Additional Notes

When in development build, you can debug a project by stopping that specific container and then choosing the `Debug` option in Visual Studio. For example, for the Manage project, you would stop the `manage` container and then choose `Debug` in Visual Studio. 

This command, run from the `..Build/development` directory, will stop the `manage` container:
```bash
.\docker-down.bat manage
```

Then run the `manage` project in debug mode from Visual Studio.

# How to Contribute

We welcome contributes and feedback from our community! Please feel free to:

- Submit issues for suggestions, bugs, or questions.
- Create pull requests for proposed changes or improvements.
- Join discussions on [Discord](https://discord.gg/fyWWqzeUKH).
- Spread the word about Agience to help grow the community.
- Share your projects to let us know how you're using or planning to use Agience.
- Provide feedback on the platform and how we can improve it.
- Suggest features and ideas for new improvements.
- Engage with the community by joining discussions and sharing your knowledge.