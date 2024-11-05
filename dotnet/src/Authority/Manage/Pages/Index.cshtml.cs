using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Agience.Authority.Manage.Services;
using Agience.Authority.Models.Manage;
using Agience.Plugins.Core.Interaction;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
using Agience.Core.Logging;
using Microsoft.AspNetCore.SignalR;
using Agience.Core.Models.Messages;

namespace Agience.Authority.Manage.Pages
{
    public class IndexModel : PageModel
    {

        private readonly IHubContext<AgienceChatHub> _hubContext;

        public List<Agency> Agencies { get; private set; } = new();
        public Agency? ActiveAgency => Agencies.FirstOrDefault(a => a.Id == TempData["ActiveAgencyId"] as string);
        public Agent? ActiveAgent => Agencies.SelectMany(a => a.Agents).FirstOrDefault(a => a.Id == TempData["ActiveAgentId"] as string);
        public List<string> AgentLogs { get; private set; } = new List<string>();

        private readonly AgienceAuthorityService _authorityService;
        private readonly ILogger<IndexModel> _logger;
        private readonly IInteractionService _interactionService;

        public IndexModel(AgienceAuthorityService authority, AgienceWebInteractionService interactionService, ILogger<IndexModel> logger, IHubContext<AgienceChatHub> hubContext)
        {
            _authorityService = authority;
            _interactionService = interactionService;
            _logger = logger;
            _hubContext = hubContext;

            if (_interactionService.IsExistedAgencyHandler() != true)
            {
                _interactionService.AgencyLogEntryReceived += HandleAgencyLogEntryReceived;
            }
            if (_interactionService.IsExistedAgentHandler() != true) 
            {
                _interactionService.AgentLogEntryReceived += HandleAgentLogEntryReceived;
            }
        }

        private async Task HandleAgencyLogEntryReceived(EventLogArgs args)
        {
            if (Agencies.Any(a => a.Id == args.AgencyId))
            {
                await _hubContext.Clients.All.SendAsync("ReceiveLog", args.AgencyId, args.Formatter(args.State, args.Exception));
            }
        }

        private async Task HandleAgentLogEntryReceived(EventLogArgs args)
        {
            if (Agencies.SelectMany(a => a.Agents).Any(a => a.Id == args.AgentId))
            {
                if(!string.IsNullOrEmpty(args.AgentId) && args.Formatter != null && args.State != null)
                { 
                    //add agent log
                    await AddLog(args.AgentId, args.Formatter(args.State, args.Exception));
                }

                await _hubContext.Clients.All.SendAsync("ReceiveLog", args.AgentId, args.Formatter(args.State, args.Exception));
            }
        }


        public async Task OnGetAsync()
        {
            var httpResponse = await _authorityService.GetHttpClient().GetAsync("manage/agencies");

            if (httpResponse.IsSuccessStatusCode)
            {
                Agencies = JsonSerializer.Deserialize<List<Agency>>(await httpResponse.Content.ReadAsStringAsync()) ?? new();

                SetView(
                    TempData["ActiveTab"] as string ?? "interaction",
                    TempData["ActiveAgencyId"] as string ?? Agencies.FirstOrDefault()?.Id,
                    TempData["ActiveAgentId"] as string ?? ""
                    );

                foreach (var agency in Agencies)
                {
                    // TODO: Temp disabled, need to implement Agency interaction
                    //agency.IsConnected = await _interactionService.IsAgencyConnected(agency.Id);

                    foreach (var agent in agency.Agents)
                    {
                        agent.IsConnected = await _interactionService.IsAgentConnected(agent.Id);
                    }
                }

                // TODO: This should be a delegate so it can be filtered pre-delivery by the interaction service
                _interactionService.AgencyChatMessageReceived += this._interactionService_AgencyChatMessageReceived; 

                LoadHistory();
            }
        }

        private Task _interactionService_AgencyChatMessageReceived(AgienceChatMessageArgs arg)
        {
            throw new NotImplementedException();
        }

        public IActionResult OnPost(string agenciesJson, string activeTab, string activeAgencyId, string activeAgentId)
        {
            Agencies = JsonSerializer.Deserialize<List<Agency>>(agenciesJson) ?? new();
            SetView(activeTab, activeAgencyId, activeAgentId);

            LoadHistory();

            return Page();
        }


        public async Task<IActionResult> OnPostSendMessageAsync(string entityType, string entityId, string message)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId) || string.IsNullOrWhiteSpace(message))
            {
                return new JsonResult(new { success = false, response = "Invalid input" });
            }

            if (entityType == "agency")
            {
                await _interactionService.SendAgencyChatMessageAsync(entityId, message);

                return new JsonResult(new { success = true });

            }
            else if (entityType == "agent")
            {
                await _interactionService.SendAgentChatMessageAsync(entityId, message);

                string response = (await _interactionService.GetAgentChatHistoryAsync(entityId)).ToList().Last().ToString();

                response = response == message ? string.Empty : response;
                
                return new JsonResult(new { success = true, response = response });

            }

            return new JsonResult(new { success = false, response = "Invalid entity type" });
        }

        private void SetView(string? activeTab, string? activeAgencyId, string? activeAgentId)
        {
            TempData["ActiveTab"] = activeTab;
            TempData["ActiveAgencyId"] = activeAgencyId;
            TempData["ActiveAgentId"] = activeAgentId;
        }

        private async void LoadHistory()
        {

            if (!string.IsNullOrEmpty(ActiveAgency?.Id))
            {
                var history = await _interactionService.GetAgencyChatHistoryAsync(ActiveAgency.Id);

                if (history.Any())
                {
                    var chatHistoryBuilder = new StringBuilder();

                    foreach (var message in history)
                    {
                        var messageClass = message.Role == AuthorRole.User ? "chat-message-outgoing" : "chat-message-incoming";
                        chatHistoryBuilder.AppendLine($"<div class=\"chat-message {messageClass}\">{message.Content}</div>");
                    }
                    TempData["ChatHistory"] = chatHistoryBuilder.ToString();
                }

            }
            else if (!string.IsNullOrEmpty(ActiveAgent?.Id))
            {
                // TODO: Implement agent chat history               
            }
        }

        private async Task HandleAgencyChatHistoryUpdated(string agencyId, IEnumerable<ChatMessageContent> messages)
        {
            if (agencyId == ActiveAgency?.Id || ActiveAgent?.Agency?.Id == agencyId)
            {
                var chatHistoryBuilder = new StringBuilder(TempData["ChatHistory"] as string ?? string.Empty);

                foreach (var message in messages)
                {
                    var messageType = message.Role == AuthorRole.User ? "chat-message-outgoing" : "chat-message-incoming";
                    chatHistoryBuilder.AppendLine($"<div class=\"{messageType}\">{message.Content}</div>");
                }

                TempData["ChatHistory"] = chatHistoryBuilder.ToString();

                // Trigger a re-render or update the chat display in real-time if applicable.
            }
        }

        /// <summary>
        /// This method is to add the log
        /// </summary>
        /// <param name="agentId">agentId</param>
        /// <param name="log">log</param>
        /// <returns>Return the newly created log object</returns>
        private async Task AddLog(string agentId, string log)
        {
            try
            {
                var logObj = new Log
                {
                    AgentId = agentId,
                    LogText = log,
                    CreatedDate = DateTime.UtcNow
                };
                var json = JsonSerializer.Serialize(logObj);
                    HttpResponseMessage? httpResponse = default;
                    httpResponse = await _authorityService.GetHttpClient().PostAsJsonAsync("manage/log", logObj);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message?.ToString() ?? ex?.InnerException?.Message ?? "Failed to add log!");
            }
        }

        /// <summary>
        /// This method is to get the logs by agent id
        /// </summary>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetGetLogsByAgentId(string agentId)
        {
            HttpResponseMessage? httpResponse = default;
            httpResponse = await _authorityService.GetHttpClient().GetAsync($"manage/log/{agentId}");

            if (httpResponse.IsSuccessStatusCode)
            {
                var response = JsonSerializer.Deserialize<List<Log>>(await httpResponse.Content.ReadAsStringAsync()) ?? new();
                return new JsonResult(new { success = true, response = response });
            }
            else
            {
                return new JsonResult(new { success = false, response = string.Empty });
            }
        }
    }
}

