using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Agience.Authority.Models.Manage;
using Agience.Authority.Manage.Web.Services;
using Host = Agience.Authority.Models.Manage.Host;

namespace Agience.Authority.Manage.Web.Pages
{
    public class AgentsModel : PageModel
    {
        public List<Agent> Agents { get; private set; } = new();
        public List<Agency> Agencies { get; private set; } = new();
        public List<Host> Hosts { get; private set; } = new();
        public List<Function> Functions { get; private set; } = new();

        public List<Plugin> FindPluginResults { get; set; } = new();
        public List<Authorizer> SelectAuthorizerResults { get; set; } = new();

        private readonly ILogger<AgentsModel> _logger;
        private readonly AgienceAuthorityService _authorityService;

        public AgentsModel(AgienceAuthorityService authorityService, ILogger<AgentsModel> logger)
        {
            _authorityService = authorityService;
            _logger = logger;
        }

        public void SetView(string? activeTab, string? activePage)
        {
            TempData["ActiveTab"] = activeTab;
            TempData["ActivePage"] = activePage;
        }

        public async Task OnGetAsync()
        {
            var agentsResponse = await _authorityService.GetHttpClient().GetAsync("manage/agents");

            if (agentsResponse.IsSuccessStatusCode)
            {
                Agents = JsonSerializer.Deserialize<List<Agent>>(await agentsResponse.Content.ReadAsStringAsync()) ?? new();

                Agents.Add(new Agent() { Id = "new" });
            }

            var activeTabName = TempData["ActiveTab"] as string ?? "details";
            var activeAgentId = TempData["ActivePage"] as string ?? Agents.FirstOrDefault()?.Id;

            // Picklist Data

            var agenciesResponse = await _authorityService.GetHttpClient().GetAsync("manage/agencies");

            if (agenciesResponse.IsSuccessStatusCode)
            {
                Agencies = JsonSerializer.Deserialize<List<Agency>>(await agenciesResponse.Content.ReadAsStringAsync()) ?? new();
            }

            // TODO: Move Hosts to Agencies

            var hostsResponse = await _authorityService.GetHttpClient().GetAsync("manage/hosts?p=true");

            if (hostsResponse.IsSuccessStatusCode)
            {
                Hosts = JsonSerializer.Deserialize<List<Host>>(await hostsResponse.Content.ReadAsStringAsync()) ?? new();
            }
            
            SetView(activeTabName, activeAgentId);

            await EnsureAgentConnectionsExist();
        }

        private async Task EnsureAgentConnectionsExist()
        {
            if (TempData["ActiveTab"] as string == "connections" && TempData["ActivePage"] as string != null)
            {
                var agentId = (string)TempData["ActivePage"]!;

                var connectionsResponse = await _authorityService.GetHttpClient().GetAsync($"manage/agent/{agentId}/plugins/connections");

                if (connectionsResponse.IsSuccessStatusCode)
                {
                    var agent = Agents.Find(a => a.Id == agentId)!;

                    agent.Connections ??= new();

                    foreach (var pluginConnection in JsonSerializer.Deserialize<List<PluginConnection>>(await connectionsResponse.Content.ReadAsStringAsync()) ?? new())
                    {
                        var agentConnection = agent.Connections.Find(c => c.PluginConnectionId == pluginConnection.Id);

                        if (agentConnection == null)
                        {
                            agentConnection ??= new AgentConnection()
                            {
                                AgentId = agentId,
                                PluginConnection = pluginConnection,
                                PluginConnectionId = pluginConnection.Id
                            };

                            agent.Connections.Add(agentConnection);
                        }

                        // TODO: FIXME: This is a hack. We should be able to get the Plugin from the AgentConnection.
                        agentConnection.PluginConnection.Plugin ??= new Plugin() { Name = agent.Plugins.Find(p => p.Id == pluginConnection.PluginId)?.Name };
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostAsync(string agentsJson, string agenciesJson, string hostsJson, string activeTab, string activePage)
        {
            // TODO: We shouldn't have to save the Json to the properties. This is to workaround tab switching.

            Agents = JsonSerializer.Deserialize<List<Agent>>(agentsJson) ?? new();

            Agencies = JsonSerializer.Deserialize<List<Agency>>(agenciesJson) ?? new();

            Hosts = JsonSerializer.Deserialize<List<Host>>(hostsJson) ?? new();

            if (activePage != "new")
            {
                // TODO: Cache and only call as needed
                var functionsResponse = await _authorityService.GetHttpClient().GetAsync($"manage/agent/{activePage}/functions");

                if (functionsResponse.IsSuccessStatusCode)
                {
                    Functions = JsonSerializer.Deserialize<List<Function>>(await functionsResponse.Content.ReadAsStringAsync()) ?? new();
                }
            }            

            SetView(activeTab, activePage);

            await EnsureAgentConnectionsExist();

            return Page();
        }

        public async Task<IActionResult> OnPostFindPluginsAsync(string searchTerm, bool includePublic)
        {
            _logger.LogInformation($"Searching for plugins with term: {searchTerm} | {includePublic.ToString()}");

            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 3)
            {
                return BadRequest(new { ErrorMessage = "Search term must be at least 3 characters." });
            }

            var query = $"s={Uri.EscapeDataString(searchTerm)}";
            query += includePublic ? "&p=true" : string.Empty;

            _logger.LogInformation($"Query: {query}");

            var httpResponse = await _authorityService.GetHttpClient().GetAsync($"manage/plugins/find?{query}");

            if (httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Success fetching plugins: {httpResponse.StatusCode}");
                FindPluginResults = JsonSerializer.Deserialize<List<Plugin>>(await httpResponse.Content.ReadAsStringAsync()) ?? new();

                _logger.LogInformation($"Found {FindPluginResults.Count} plugins.");
                return Page();
            }

            _logger.LogError($"Error fetching plugins: {httpResponse.StatusCode} => {await httpResponse.Content.ReadAsStringAsync()}");

            return BadRequest(new { ErrorMessage = "Error fetching plugins." });
        }

        public async Task<IActionResult> OnPostAddPluginAsync(string agentsJson, string findPluginResultsJson, string agentId, string pluginId)
        {
            if (string.IsNullOrWhiteSpace(pluginId) || string.IsNullOrWhiteSpace(agentId) || string.IsNullOrWhiteSpace(agentsJson))
            {
                return BadRequest(new { ErrorMessage = "Invalid pluginId, agentsJson, or agentsId." });
            }

            var plugin = JsonSerializer.Deserialize<List<Plugin>>(findPluginResultsJson)?.Find(p => p.Id == pluginId);

            if (plugin == null)
            {
                return BadRequest(new { ErrorMessage = "Invalid plugin." });
            }

            SetView("plugins", agentId);

            Agents = JsonSerializer.Deserialize<List<Agent>>(agentsJson) ?? new();

            var httpResponse = await _authorityService.GetHttpClient().PutAsync($"manage/agent/{agentId}/plugin/{plugin.Id}", null);

            if (httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Success adding Plugin to Agent: {httpResponse.StatusCode}");
                Agents.Find(a => a.Id == agentId)?.Plugins.Add(plugin);
                return Page();
            }

            _logger.LogError($"Error adding Plugin to Agent: {httpResponse.StatusCode} => {await httpResponse.Content.ReadAsStringAsync()}");
            return Page();
        }

        public async Task<IActionResult> OnPostRemovePluginAsync(string agentId, string pluginId)
        {
            if (string.IsNullOrWhiteSpace(agentId) || string.IsNullOrWhiteSpace(pluginId))
            {
                return BadRequest(new { ErrorMessage = "Agent ID and Plugin ID are required." });
            }

            SetView("plugins", agentId);

            var httpResponse = await _authorityService.GetHttpClient().DeleteAsync($"manage/agent/{agentId}/plugin/{pluginId}");

            if (httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Success removing Plugin from Agent: {httpResponse.StatusCode}");

                TempData["SuccessMessage"] = "Success!";
                return RedirectToPage();
            }

            _logger.LogError($"Error fetching plugins: {httpResponse.StatusCode} => {await httpResponse.Content.ReadAsStringAsync()}");

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAgentDeleteAsync(Agent agent)
        {
            if (agent.Id == null) { return Page(); }

            SetView("details", agent.Id);

            var httpResponse = await _authorityService.GetHttpClient().DeleteAsync($"manage/agent/{Uri.EscapeDataString(agent.Id)}");

            if (httpResponse.IsSuccessStatusCode)
            {
                SetView("details", null);
                TempData["SuccessMessage"] = "Success!";
            }
            else
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Unsuccessful Delete Agent {agent.Id} => {httpResponse.StatusCode} => {errorContent}");
                TempData["ErrorMessage"] = "Error!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAgentSaveAsync(Agent agent)
        {
         //   if (ModelState["agent.Agency"]?.Errors.Any() ?? false)
         //   {
         //       ModelState.Remove("agent.Agency");
         //   }            

            if (!ModelState.IsValid || agent?.Id == null) { return Page(); }

            SetView("details", agent.Id);

            HttpResponseMessage? httpResponse = default;

            if (agent.Id == "new")
            {
                agent.Id = string.Empty;

                httpResponse = await _authorityService.GetHttpClient().PostAsJsonAsync("manage/agent", agent);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var id = JsonSerializer.Deserialize<Agent>(await httpResponse.Content.ReadAsStringAsync())?.Id;
                    SetView("details", id);
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Save Agent {agent.Id} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }
            }
            else
            {
                httpResponse = await _authorityService.GetHttpClient().PutAsJsonAsync($"manage/agent", agent);

                if (httpResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Update Agent {agent.Id} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostFindAuthorizersAsync()
        {
            _logger.LogInformation("Fetching all authorizers");

            var httpResponse = await _authorityService.GetHttpClient().GetAsync("manage/authorizers");

            if (httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Success fetching authorizers: {httpResponse.StatusCode}");
                SelectAuthorizerResults = JsonSerializer.Deserialize<List<Authorizer>>(await httpResponse.Content.ReadAsStringAsync()) ?? new();

                _logger.LogInformation($"Found {SelectAuthorizerResults.Count} authorizers.");
                return Page();
            }

            _logger.LogError($"Error fetching authorizers: {httpResponse.StatusCode} => {await httpResponse.Content.ReadAsStringAsync()}");

            return BadRequest(new { ErrorMessage = "Error fetching authorizers." });
        }


        public async Task<IActionResult> OnPostSelectAuthorizerAsync(string agentId, string authorizerId, string agentConnection)
        {
            if (string.IsNullOrWhiteSpace(agentId) || string.IsNullOrWhiteSpace(authorizerId) || string.IsNullOrWhiteSpace(agentConnection))
            {
                return BadRequest(new { ErrorMessage = "Invalid agentId, authorizerId, or agentConnection." });
            }

            var connection = JsonSerializer.Deserialize<AgentConnection>(agentConnection);
            if (connection == null)
            {
                return BadRequest(new { ErrorMessage = "Invalid agentConnection." });
            }

            _logger.LogInformation($"Updating authorizer for connection: {agentId} | {connection.PluginConnectionId} | {authorizerId}");

            var httpResponse = await _authorityService.GetHttpClient().PutAsync($"manage/agent/{agentId}/connection/{connection.PluginConnectionId}/authorizer/{authorizerId}", null);

            if (httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Success updating authorizer for connection: {httpResponse.StatusCode}");
                var authorizer = SelectAuthorizerResults.Find(a => a.Id == authorizerId);
                if (authorizer != null)
                {
                    connection.Authorizer = authorizer;
                }
                return Page();
            }

            _logger.LogError($"Error updating authorizer for connection: {httpResponse.StatusCode} => {await httpResponse.Content.ReadAsStringAsync()}");

            return BadRequest(new { ErrorMessage = "Error updating authorizer for connection." });
        }

        public async Task<IActionResult> OnPostActivateConnectionAsync(string agentId, string pluginConnectionId)
        {

            var httpResponse = await _authorityService.GetHttpClient().GetAsync($"manage/agent/{agentId}/connection/{pluginConnectionId}/activate");

            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Found && httpResponse.Headers.Location != null)
            {
                return Redirect(httpResponse.Headers.Location.ToString());
            }

            // TODO: Handle other status codes

            _logger.LogError($"Error activating connection: {httpResponse.StatusCode} => {httpResponse.Headers.Location?.ToString()} => {await httpResponse.Content.ReadAsStringAsync()}");

            return BadRequest(new { ErrorMessage = "Error activating connection." });
        }


        public async Task<IActionResult> OnPostDeactivateConnectionAsync(string agentId, string pluginConnectionId)
        {
            throw new NotImplementedException();
        }
    }
}