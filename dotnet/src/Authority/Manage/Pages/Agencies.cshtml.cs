using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Agience.Authority.Models.Manage;
using Agience.Authority.Manage.Services;

namespace Agience.Authority.Manage.Pages
{
    public class AgenciesModel : PageModel
    {
        public List<Agency> Agencies { get; private set; } = new();

        private readonly ILogger<AgenciesModel> _logger;
        private readonly AgienceAuthorityService _authorityService;

        public AgenciesModel(AgienceAuthorityService authorityService, ILogger<AgenciesModel> logger)
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
            var agenciesResponse = await _authorityService.GetHttpClient().GetAsync("manage/agencies");

            if (agenciesResponse.IsSuccessStatusCode)
            {
                Agencies = JsonSerializer.Deserialize<List<Agency>>(await agenciesResponse.Content.ReadAsStringAsync()) ?? new();

                Agencies.Add(new Agency() { Id = "new" });

                SetView(TempData["ActiveTab"] as string ?? "details", TempData["ActivePage"] as string ?? Agencies.FirstOrDefault()?.Id);

            }
        }

        public Task<IActionResult> OnPostAsync(string agenciesJson, string activeTab, string activePage)
        {
            // TODO: We shouldn't have to save the hostsJson to the Hosts property

            Agencies = JsonSerializer.Deserialize<List<Agency>>(agenciesJson) ?? new();

            SetView(activeTab, activePage);

            return Task.FromResult((IActionResult)Page());
        }

        public async Task<IActionResult> OnPostAgencyDeleteAsync(Agency agency)
        {
            if (agency.Id == null) { return Page(); }

            SetView("details", agency.Id);

            var httpResponse = await _authorityService.GetHttpClient().DeleteAsync($"manage/agency/{Uri.EscapeDataString(agency.Id)}");

            if (httpResponse.IsSuccessStatusCode)
            {
                SetView("details", null);
                TempData["SuccessMessage"] = "Success!";
            }
            else
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Unsuccessful Delete Agency {agency.Id} => {httpResponse.StatusCode} => {errorContent}");
                TempData["ErrorMessage"] = "Error!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAgencySaveAsync(Agency agency)
        {
            if (!ModelState.IsValid || agency.Id == null) { return Page(); }

            SetView("details", agency.Id);

            HttpResponseMessage? httpResponse = default;

            if (agency.Id == "new")
            {
                agency.Id = string.Empty;

                httpResponse = await _authorityService.GetHttpClient().PostAsJsonAsync("manage/agency", agency);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var id = JsonSerializer.Deserialize<Agency>(await httpResponse.Content.ReadAsStringAsync())?.Id;
                    SetView("details", id);
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Post Agency {agency.Id} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }
            }
            else
            {
                httpResponse = await _authorityService.GetHttpClient().PutAsJsonAsync($"manage/agency", agency);

                if (httpResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Update Agency {agency.Id} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAgentSaveAsync(Agency agency, int idx)
        {

            var agent = agency?.Agents[idx];

            if (!ModelState.IsValid || agency?.Id == null || agent?.Id == null) { return Page(); }

            SetView("agents", agency.Id);

            if (agent.Id == "new")
            {
                if (string.IsNullOrEmpty(agent.Name))
                {
                    TempData["ErrorMessage"] = "Error! Agent name is required.";
                    return RedirectToPage();
                }

                agent.Id = string.Empty;

                var httpResponse = await _authorityService.GetHttpClient().PostAsJsonAsync($"manage/agent", agent);

                if (!httpResponse.IsSuccessStatusCode)
                {   
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Post Agent {agent.Name} => {httpResponse.StatusCode} => {errorContent}");

                    TempData["ErrorMessage"] = $"Failed to save agent: {httpResponse.ReasonPhrase}";
                    return RedirectToPage();
                }
            }
            else
            {
                var httpResponse = await _authorityService.GetHttpClient().PutAsJsonAsync($"manage/agent/", agent);

                if (httpResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Save Agency {agency.Id} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error saving agent!";
                }
            }

            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostAgentDeleteAsync(Agency agency, int idx)
        {

            SetView("agents", agency.Id);

            var agent = agency.Agents[idx];

            if (!ModelState.IsValid || agent.Id == null) { return Page(); }

            var httpResponse = await _authorityService.GetHttpClient().DeleteAsync($"manage/agent/{Uri.EscapeDataString(agent.Id)}");

            if (httpResponse.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Success!";
            }
            else
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Unsuccessful Delete Agency {agency.Id} => {httpResponse.StatusCode} => {errorContent}");
                TempData["ErrorMessage"] = "Error deleting agent!";
            }

            return RedirectToPage();
        }
    }
}