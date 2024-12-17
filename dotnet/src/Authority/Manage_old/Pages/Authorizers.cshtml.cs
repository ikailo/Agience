using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Agience.Authority.Manage.Services;
using Agience.Authority.Models.Manage;

namespace Agience.Authority.Manage.Pages
{
    public class AuthorizerModel : PageModel
    {
        public List<Authorizer> Authorizers { get; private set; } = new();

        private readonly ILogger<AuthorizerModel> _logger;
        private readonly AgienceAuthorityService _authorityService;

        public string? AuthorityUri => _authorityService.AuthorityUri;

        public AuthorizerModel(AgienceAuthorityService agienceAuthorityService, ILogger<AuthorizerModel> logger)
        {
            _authorityService = agienceAuthorityService;
            _logger = logger;
        }
        public void SetView(string? activeTab, string? activePage)
        {
            TempData["ActiveTab"] = activeTab;
            TempData["ActivePage"] = activePage;
        }

        public async Task OnGetAsync()
        {
            var httpResponse = await _authorityService.GetHttpClient().GetAsync("manage/authorizers");

            if (httpResponse.IsSuccessStatusCode)
            {
                Authorizers = JsonSerializer.Deserialize<List<Authorizer>>(await httpResponse.Content.ReadAsStringAsync()) ?? new();

                Authorizers.Add(new Authorizer() { Id = "new" });

                SetView(TempData["ActiveTab"] as string ?? "details", TempData["ActivePage"] as string ?? Authorizers.FirstOrDefault()?.Id);
            }
        }

        public Task<IActionResult> OnPostAsync(string authorizersJson, string activeTab, string activePage)
        {
            Authorizers = JsonSerializer.Deserialize<List<Authorizer>>(authorizersJson) ?? new();

            SetView(activeTab, activePage);

            return Task.FromResult((IActionResult)Page());
        }

        public async Task<IActionResult> OnPostAuthorizerDeleteAsync(Authorizer authorizer)
        {
            if (authorizer.Id == null) { return Page(); }

            SetView("details", authorizer.Id);

            var httpResponse = await _authorityService.GetHttpClient().DeleteAsync($"manage/authorizer/{Uri.EscapeDataString(authorizer.Id)}");

            if (httpResponse.IsSuccessStatusCode)
            {
                SetView("details", null);
                TempData["SuccessMessage"] = "Success!";
            }
            else
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Unsuccessful Delete Authorizer {authorizer.Id} => {httpResponse.StatusCode} => {errorContent}");
                TempData["ErrorMessage"] = "Error!";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAuthorizerSaveAsync(Authorizer authorizer)
        {
            if (!ModelState.IsValid || authorizer?.Id == null) { return Page(); }

            SetView("details", authorizer.Id);

            HttpResponseMessage? httpResponse = default;

            if (authorizer.Id == "new") {

                authorizer.Id = string.Empty;

                httpResponse = await _authorityService.GetHttpClient().PostAsJsonAsync("manage/authorizer", authorizer);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var id = JsonSerializer.Deserialize<Authorizer>(await httpResponse.Content.ReadAsStringAsync())?.Id;
                    SetView("details", id);
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Save Authorizer {authorizer.Id} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }

            } else {

                httpResponse = await _authorityService.GetHttpClient().PutAsJsonAsync($"manage/authorizer", authorizer);

                if (httpResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Update Authorizer {authorizer.Id} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }
            }           
            
            return RedirectToPage();
        }
    }
}