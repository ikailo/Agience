using System.Collections.Concurrent;
using System.Threading;
using Agience.Core;
using Agience.Core.Interfaces;
using Agience.Core.Logging;
using Agience.Core.Models.Messages;
using Agience.Plugins.Core.Interaction;
using Microsoft.Extensions.Logging;
using Host = Agience.Core.Host;

namespace Agience.Hosts._Console
{
    public class InteractiveConsole : IInteractionService, IEventLogHandler
    {
        private readonly ILogger<InteractiveConsole> _logger;
        private readonly ConcurrentDictionary<string, Queue<MessageRequest>> _agentMessageQueues = new(); // Keyed by AgentId
        private readonly Queue<MessageRequest> _messageQueue = new();
        private string? _currentAgentId;
        private readonly Host _host;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public InteractiveConsole(ILogger<InteractiveConsole> logger, Host host)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _host = host ?? throw new ArgumentNullException(nameof(host));

            _host.AgentConnected += OnAgentConnected;

            // Start background message processing
            Task.Run(() => ProcessMessagesAsync(_cancellationTokenSource.Token));
        }

        public void OnLogEntryReceived(object sender, EventLogArgs e)
        {
            //var agentName = GetAgentNameById(e.AgentId) ?? e.AgentId;
            //var logMessage = FormatMessage("LOG", agentName, e.Formatter(e.State, e.Exception));
            //WriteMessage(logMessage, e.AgentId);
        }

        private async Task OnAgentConnected(Agent agent)
        {
            _logger.LogInformation($"Agent connected: {agent.Name} ({agent.Id})");

            _agentMessageQueues.TryAdd(agent.Id, new Queue<MessageRequest>());

            if (string.IsNullOrEmpty(_currentAgentId))
            {
                SwitchContextByName(agent.Name);
            }
        }
        

        private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_messageQueue.TryDequeue(out var request))
                {
                    if (_currentAgentId != null &&
                        _agentMessageQueues.TryGetValue(_currentAgentId, out var agentQueue) &&
                        agentQueue.Contains(request))
                    {
                        HandleMessage(request);
                    }
                    else
                    {
                        _messageQueue.Enqueue(request);
                    }
                }

                await Task.Delay(50); // Prevent tight loop
            }
        }

        private void HandleMessage(MessageRequest request)
        {
            var message = FormatMessage(request.Tag, request.AgentName, request.Message);

            if (request.IsInput)
            {
                Console.Write(message);
                request.Message = Console.ReadLine();
            }
            else
            {
                Console.WriteLine(message);
            }

            
            request.IsProcessed = true;
        }

        public async Task<string?> SendToAgent(Agent agent)
        {
            // Create a message request with the "TO" tag
            var messageRequest = new MessageRequest
            {
                AgentId = agent.Id,
                AgentName = agent.Name,
                Tag = "TO",
                IsInput = true,
                IsProcessed = false
            };

            // Enqueue the message for processing
            _messageQueue.Enqueue(messageRequest);
            _agentMessageQueues.GetOrAdd(agent.Id, _ => new Queue<MessageRequest>()).Enqueue(messageRequest);

            // Wait for the user's input
            while (!messageRequest.IsProcessed)
            {

                await Task.Delay(50);
                /*
                Console.Write($"[{DateTime.Now:HH:mm:ss}] {messageRequest.Tag} {agent.Name}> ");
                messageRequest.Message = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(messageRequest.Message))
                {
                    messageRequest.IsProcessed = true;
                }*/
            }

            // Return the user's input
            return messageRequest.Message;
        }


        public async Task ReceiveFromAgent(Agent agent, string message)
        {
            var messageRequest = new MessageRequest
            {
                AgentId = agent.Id,
                AgentName = agent.Name,
                Message = message,
                Tag = "FROM",
                IsInput = false,
                IsProcessed = false
            };

            _messageQueue.Enqueue(messageRequest);
            _agentMessageQueues.GetOrAdd(agent.Id, _ => new Queue<MessageRequest>()).Enqueue(messageRequest);

            //var formattedMessage = FormatMessage("SHOW", agent.Name, message);
            //WriteMessage(formattedMessage, agent.Id);
        }

        private void WriteMessage(string message, string agentId)
        {
            if (_currentAgentId == agentId)
            {
                Console.WriteLine(message);
            }
            else
            {
                // TODO: Queue up the message for later display
                //Console.WriteLine($"Message queued for agent {GetAgentNameById(agentId) ?? agentId}. Use `/switch` to view.");
            }
        }

        private string FormatMessage(string type, string agentName, string content)
        {
            return $"[{DateTime.Now:HH:mm:ss}] [{type}] {agentName}> {content}";
        }

        public async Task Start()
        {
            Console.WriteLine("Welcome to Agience Interactive Console!");
            Console.WriteLine("Type `/help` for a list of commands.");

            while (true)
            {
                // Display the input prompt only after handling all outputs
                var agentName = GetAgentNameById(_currentAgentId) ?? "NoAgent";
                Console.Write($"[{DateTime.Now:HH:mm:ss}] [IN] {agentName}> ");

                // Read user input
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine(); // Ensure clean spacing for the next prompt
                    continue;
                }

                // Process the input
                if (input == "/help")
                {
                    ShowHelp();
                }
                else if (input.StartsWith("/switch "))
                {
                    var agentNameToSwitch = input.Substring(8).Trim();
                    SwitchContextByName(agentNameToSwitch);
                }
                else if (input == "/list")
                {
                    DisplayAgentStatus();
                }
                else if (input == "/logs")
                {
                    DisplayLogs();
                }
                else
                {
                    await SendToCurrentAgent(input);
                }

                // Add a clean line after the response
                Console.WriteLine();
            }
        }



        private void ShowHelp()
        {
            Console.WriteLine("Available Commands:");
            Console.WriteLine("/help       - Show this help menu");
            Console.WriteLine("/list       - List all connected agents");
            Console.WriteLine("/switch <agentName> - Switch to a specific agent");
            Console.WriteLine("/logs       - Display recent logs");
        }

        private void SwitchContextByName(string agentName)
        {
            var agentId = GetAgentIdByName(agentName);
            if (agentId != null)
            {
                _currentAgentId = agentId;
                Console.WriteLine();
                Console.Write($"[{DateTime.Now:HH:mm:ss}] [IN] {agentName}> ");                
            }
            else
            {
                Console.WriteLine($"Error: Agent {agentName} not found.");
            }
        }

        private async Task SendToCurrentAgent(string input)
        {
            if (string.IsNullOrEmpty(_currentAgentId))
            {
                Console.WriteLine("No agent selected. Use `/list` to see available agents.");
                return;
            }

            if (_host.Agents.TryGetValue(_currentAgentId, out var agent))
            {
                // Get the response from the agent synchronously
                var response = await agent.PromptAsync(input);

                // Display the response after processing
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [OUT] {agent.Name}> {response}");
            }
            else
            {
                Console.WriteLine($"Error: Unable to send message to {_currentAgentId}.");
            }
        }



        private void DisplayAgentStatus()
        {
            Console.WriteLine("Connected Agents:");
            foreach (var kvp in _agentMessageQueues)
            {
                var agentName = GetAgentNameById(kvp.Key) ?? kvp.Key;
                var queue = kvp.Value;
                int inputs = queue.Count(req => req.IsInput);
                int outputs = queue.Count(req => !req.IsInput);
                Console.WriteLine($"{agentName}: {inputs} input(s), {outputs} output(s) pending");
            }
        }

        private void DisplayLogs()
        {
            Console.WriteLine("Recent Logs:");
            // Add functionality to retrieve and display logs if necessary
        }

        private string? GetAgentNameById(string? agentId)
        {
            if (string.IsNullOrEmpty(agentId)) return null;

            return _host.Agents.TryGetValue(agentId, out var agent) ? agent.Name : null;
        }

        private string? GetAgentIdByName(string agentName)
        {
            return _host.Agents.Values.FirstOrDefault(a => a.Name == agentName)?.Id;
        }

        public class MessageRequest
        {
            public string? Message { get; set; }
            public string? AgentId { get; set; }
            public string? AgentName { get; set; }
            public bool IsInput { get; set; }
            public bool IsProcessed { get; set; }
            public string? Tag { get; set; }
        }
    }

}
