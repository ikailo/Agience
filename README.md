# Agience: AI Agents Powered by You

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

### Would you like a one-on-one session to go through all this?  **Reach out via [email](mailto:connect@agience.ai) or via [Discord](https://discord.gg/fyWWqzeUKH) for real-time assistance.**

Visit the [Agience Preview Instance](https://preview.agience.ai) to explore the platform and to start configuring agents.

To see it in action, you will an Agience Host. The Host is a lightweight application that runs on your device and connects to the Authority. The vision of Agience is that there will be multiple Hosts available for different platforms and use cases.

**First, create a Host**
- Go to Hosts.
- Add a host name. 
- Fill in any value for "Redirect URIs" and "Post Logout Redirect URIs". (eg: foo, bar) There is a bug in the current version that requires these fields to be filled in every time.
- Select Scope: "connect"
- Click "SAVE".
- Go to the "KEYS" tab.
- Enter a key name and click "SAVE".
- Copy the Host ID and Host Secret. This is the only time you will see the Host Secret, so make sure to save it in a secure location.

Currently, there is only one Host app available [here](https://github.com/ikailo/Agience/tree/main/dotnet/src/Hosts/Console) and it can be created using Visual Studio by cloning and building the source code. In the future, we will provide a standalone installer for this Host.

**Host Build Instructions**

Prerequsites:
  - [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/)
  - An OpenAI API Key
	
1. **Clone the Repository**: Start by cloning the Agience repository.
   ```bash
   git clone https://github.com/ikailo/Agience.git
   ```

2. **Open the Solution**: Open the `/dotnet/Agience.sln` solution file in Visual Studio 2022 and find the `Agience.Hosts.Console` project (in Hosts folder).

3. **Configure the Host**: Update user secrets for the Host by right-clicking on the `Agience.Hosts.Console` project and selecting `Manage User Secrets`. Add the following configuration:
   ```plaintext
   {	 
	 "HostId": "<Your_Host_Id>",
	 "HostSecret": "<Your_Host_Secret>",
	 "AuthorityUri": "https://preview.agience.ai",
	 "WorkspacePath": "<Choose_A_Local_Directory_Outside_The_Agience_Source_Code>"
   }
   ```
You'll need to escape the backslashes in the `WorkspacePath` value, like this: `"C:\\Users\\YourName\\Documents\\AgienceWorkspace"`.
 
4. **Run the Host**: Start the Host by running the `Agience.Hosts.Console` project in Visual Studio. The Host will connect to the Agience Preview Instance. The first time you run the Host, it will update the Authority with its Plugin information.

5. **Create an Agent**: Go to Agents in the preview instance.
	- Add a Name
	- Select the Host you created
	- Select the Executive Function "GetChatMessageContentsAsync"
	- Click "SAVE"
	- Go to "PLUGINS" tab. Select and Add a plugin (TimePlugin is a good start) 
	 
**All of the below steps are currently needed but will be made much easier in the future.**

6. **Create an Authorizer**: Go to Authorizers in the preview instance.
	- Add a Name "OpenAI API"
	- Select Type: "API Key"
	- Click "SAVE"

7. **Create a Connection**: Go to Connections in the preview instance.
	- Add a Name "OpenAI"
	- Click "SAVE"
	- Go to "AUTHORIZERS" tab. Select and Add the Authorizer you created.

8. **Attach the Connection to the Plugin**
	- Go to "Plugins" page.
	- Choose "ChatCompletionPlugin"
	- Go to "CONNECTIONS" tab.
	- Select "GetChatMessageContentsAsync" and add the Connection you created. Click SAVE.

9. Authorize the Agent to use the Connection
	- Go to "Agents" page.
	- Select the Agent you created in step 5.
	- Go to "CONNECTIONS" tab.
	- Select the Authorizer you created. Click "ENTER"
	- Enter a valid API Key for OpenAI. Click "SAVE".

**IMPORTANT NOTE: The API Key is NOT currently stored encrypted at rest. Even though the preview instance is secure, I strongly recommend you delete (overwrite) and disable it after you test. This is a high priority bug.**

**All the above steps are a bit cumbersome and will be made easier in the future.**

### FINALLY

10. **Test the Agent**
 - Re-start the `Agience.Hosts.Console` project in Visual Studio. The Host will connect to the Agience Preview Instance and start running the Agent. You can now test the Agent by sending a message to the Agent in the Preview Instance.
 - Ask the agent what time it is and it should respond with the current time.

### Running into an issue?  **Reach out via [email](mailto:connect@agience.ai) or via [Discord](https://discord.gg/fyWWqzeUKH) for real-time assistance.**

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

**NOTE**: The `agience-init.bat` script is succesful if the final output is:
```plaintext
  Updated HostId and HostSecret in the file successfully.
```
If it fails, you will need to correct the errors and also run `.\agience-remove.bat` to reset the configuration before running the init script again.

### Running into an issue?  **Reach out via [email](mailto:connect@agience.ai) or via [Discord](https://discord.gg/fyWWqzeUKH) for real-time assistance.**

### Open the Solution

After running the initialization script, open the `/dotnet/Agience.sln` solution file in Visual Studio 2022.

## Complete the Configuration

**Edit Agience.Authority.Identity Environment File**  

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
