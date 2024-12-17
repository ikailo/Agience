using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using AutoMapper;
using Agience.Core.Mappings;
using Agience.Core.Models.Messages;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Agience.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Agience.Core.Models.Entities;

namespace Agience.Core
{
    [AutoMap(typeof(Models.Entities.Agent), ReverseMap = true)]
    public class Agent : Models.Entities.Agent, IDisposable
    {
        public bool IsConnected { get; private set; }
        public ChatHistory ChatHistory => _chatHistory;

        private readonly OpenAIPromptExecutionSettings _promptExecutionSettings;
        private readonly ChatHistory _chatHistory = new();        

        private readonly Authority _authority;
        private readonly Broker _broker;
        private readonly ILogger _logger;
        private readonly Kernel _kernel;

        private readonly TopicGenerator _topicGenerator;


        private bool _disposed;

        internal Agent(
            string id,
            string name,
            Authority authority,
            Broker broker,
            string persona,
            Kernel kernel,
            ILogger<Agent> logger
            ) 

        {
            Id = id;
            Name = name;
            Persona = persona;
            _authority = authority;
            _broker = broker;            
            _kernel = kernel;
            _logger = logger;

            _topicGenerator = new TopicGenerator(_authority.Id, Id);

            _promptExecutionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            

        }

        internal async Task Connect()
        {
            if (!IsEnabled) { return; } // TODO: Log or handle this

            if (!IsConnected)
            {
                await _broker.Subscribe(_topicGenerator.SubscribeAsAgent(), _broker_ReceiveMessage);

                // Subscribe to the Agent's topic
                foreach (Topic topic in Topics)
                {
                    await _broker.Subscribe(_topicGenerator.ConnectTo(topic.Name), _broker_ReceiveMessage);
                }

                IsConnected = true;
            }
        }

        private async Task _broker_ReceiveMessage(BrokerMessage message)
        {
            // Incoming Credential
            if (message.Type == BrokerMessageType.EVENT &&
                message.Data?["type"] == "credential_response" &&
                message.Data?["credential_name"] != null &&
                message.Data?["encrypted_credential"] != null
                )
            {
                var name = message.Data?["credential_name"];
                var credential = message.Data["encrypted_credential"];

                var credentialService = _kernel.Services.GetRequiredService<AgienceCredentialService>();
                credentialService.AddEncryptedCredential(name, credential);
            }
        }

        internal async Task Disconnect()
        {
            // TODO: Auto-disconnect via MQTT Will message
            // TODO: Abstract the MQTT Connection to a Messaging Service

            if (IsConnected)
            {

                await _broker.Unsubscribe(_topicGenerator.SubscribeAsAgent());
                IsConnected = false;
            }
        }


        public async Task<string?> PromptAsync(string userMessage, CancellationToken cancellationToken = default)
        {
            // Add the user's message to the chat history
            _chatHistory.AddUserMessage(userMessage);

            // Get the response from the chat completion service
            var chatMessageContent = await _kernel.GetRequiredService<IChatCompletionService>()
                .GetChatMessageContentAsync(_chatHistory, _promptExecutionSettings, _kernel, cancellationToken);

            if (chatMessageContent?.Items.Last().ToString() is string assistantMessage)
            {
                // Add the assistant's message to the chat history
                _chatHistory.AddAssistantMessage(assistantMessage);

                // Return the assistant's message directly
                return assistantMessage;
            }

            // Return null if no assistant message is generated
            return null;
        }


        public void Dispose()
        {
            if (!_disposed)
            {
                if (_logger is IDisposable disposableLogger)
                {
                    disposableLogger.Dispose();
                }
                _disposed = true;
            }
        }
    }
}