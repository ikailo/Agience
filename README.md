# Agience: Intelligent Agents for Everyone

**Agience** is an **open-source** intelligent agent **platform** that intuitively and seamlessly **connects devices**, **systems**, and **data** to achieve new levels of **automation** and **efficiency**.

This **open-source** platform enables anyone to easily **build**, **deploy**, and **manage** intelligent agents capable of automating tasks, processing complex information, and facilitating communication between devices and systems with scalability and reliability.

## Highlights

- **Scalable and Reliable**: With its distributed architecture, Agience easily adapts to personal, business, industrial, and enterprise-level applications of any size and scope.

- **Agent Compute Units**: Deploy scalable, autonomous agents equipped with specialized functions to handle data, automate processes, and communicate easily across networks.
 
- **Seamless Integration**: Agience is adaptable to any addressable protocol, ensuring reliable connectivity across systems.
 
- **Open-Source**: Agience is released under the AGPL-3.0 license, allowing anyone to use, modify, and distribute the software. You can build anything using Agience, but any modifications to the framework or platform must be shared with the community for the benefit of humanity.

## Framework Architecture

Agience is a distributed intelligent agent platform comprized of a network of **authorities** that provides essential services such as an identity provider, message broker, streaming server, databases, and management APIs. These components enable secure, coordinated interactions across a network of hosts, agents, and topics, each managed by their respective authorities.

A host, deployed on devices or systems, provides computational resources and manages information flow between agents and topics while following instructions from its authority. Agents, acting as digital representatives of people, perform tasks and share information through topics. Agents adhere to the boundaries set by their authority, interacting directly to represent the individuals they serve.

Each authority governs the permissions and interactions of its agents, hosts, and topics, with JWT-based authorization managed by an identity provider (IDP). Authorities can establish trusted relationships, enabling agents from different authorities to interact across boundaries when permitted. This approach ensures agents operate securely within designated scopes and adhere to specified permissions across the Agience ecosystem.

## Configuration Options

### **Preview Instance**: A public web host using the Agience Authority.

### **Host Preview**: A local host connected to the Agience Preview Authority.


 
### **Developer**: A fully local setup with both a local host and local authority.

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

   ```powershell
   .\agience-init.bat
   ```

   Accept any prompts that appear during the initialization process.

#### Complete Configuration

After running the initialization script, you’ll need to complete a few outstanding configurations in the `.env.development` files. These files are now located within the `Agience.Authority.Manage` and `Agience.Authority.Identity` directories and control API keys and other credentials.

1. **Edit Agience.Authority.Manage Environment File**

  - Obtain an [OpenAI API key](https://help.openai.com/en/articles/4936850-where-do-i-find-my-openai-api-key) and set it in the `.env.development` file located in `Agience.Authority.Manage`.
   
   ```plaintext
   Agience.Authority.Manage/.env.development
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
   Agience.Authority.Identity/.env.development
   GoogleClientId=<Your_Google_Client_ID>
   GoogleClientSecret=<Your_Google_Client_Secret>
   ```

   These credentials enable Google authentication for Agience.

#### Start Docker Services

Once the configuration is complete, you can start the necessary Docker services for Agience by running the following command:

```powershell
.\docker-up
```

This script will bring up the required Docker containers, such as the database, message broker, and any other necessary services specified in the configuration.

---

### **Local Build**

The development build accesses the Agience stack on the `https://localhost:<port>` domain. This is good for debugging and testing locally.

You can also run it locally using the `*.local.agience.ai` domains. This is an example of how it is intended to be hosted, however it does not support debuggind directly.

Run the .\agience-init script from the Build\local directory.  Then run the .\docker-up script to start the services.


## Additional Notes:

When in development build, you can debug a project by stopping that specific container and then choosing the `Debug` option in Visual Studio. For example, for the Manage project, you would stop the `manage` container and then choose `Debug` in Visual Studio.

```powershell
.\docker-down manage
```

Then run the container in debug mode.


# How to Contribute

We welcome feedback from the community! Please feel free to:

- **Submit Issues**: For suggestions, bugs, or questions.
- **Create Pull Requests**: For proposed changes or improvements.
- **Join Discussions**: On [Discord](https://discord.gg/fyWWqzeUKH)
- **Spread the Word**: Tell others about Agience and help grow the community.
- **Share Your Projects**: Let us know how you're using or planning to use Agience.
- **Provide Feedback**: Share your thoughts on the platform and how we can improve it.
- **Suggest Features**: Share your ideas for new features and improvements.
- **Engage with the Community**: Join discussions and share your knowledge.