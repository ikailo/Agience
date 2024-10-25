using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Agience.Authority.Manage.Web.Services;
using Agience.Authority.Models.Manage;

namespace Agience.Authority.Manage.Web.Pages
{
    public class PluginsModel : PageModel
    {
        public List<Plugin> Plugins { get; private set; } = new();

        private readonly ILogger<PluginsModel> _logger;
        private readonly AgienceAuthorityService _authorityService;

        public PluginsModel(AgienceAuthorityService authorityService, ILogger<PluginsModel> logger)
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
            var httpResponse = await _authorityService.GetHttpClient().GetAsync("manage/plugins");

            if (httpResponse.IsSuccessStatusCode)
            {
                Plugins = JsonSerializer.Deserialize<List<Plugin>>(await httpResponse.Content.ReadAsStringAsync()) ?? new();

                Plugins.Add(new Plugin() { Id = "new" });

                foreach (var plugin in Plugins)
                {
                    if (plugin.Id != "new")
                    {
                        plugin.Functions.Add(new Function() { Id = "new" });
                        plugin.Connections.Add(new PluginConnection() { Id = "new", PluginId = plugin.Id});

                    }
                }

                SetView(TempData["ActiveTab"] as string ?? "details", TempData["ActivePage"] as string ?? Plugins.FirstOrDefault()?.Id);
            }
        }

        public Task<IActionResult> OnPostAsync(string pluginsJson, string activeTab, string activePage)
        {
            // TODO: We shouldn't have to save the pluginsJson to the Plugins property

            Plugins = JsonSerializer.Deserialize<List<Plugin>>(pluginsJson) ?? new();

            SetView(activeTab, activePage);

            return Task.FromResult((IActionResult)Page());
        }

        public async Task<IActionResult> OnPostPluginDeleteAsync(Plugin plugin)
        {
            if (plugin.Id == null) { return Page(); }

            SetView("details", plugin.Id);

            var httpResponse = await _authorityService.GetHttpClient().DeleteAsync($"manage/plugin/{Uri.EscapeDataString(plugin.Id)}");

            if (httpResponse.IsSuccessStatusCode)
            {
                SetView("details", null);
                TempData["SuccessMessage"] = "Success!";
            }
            else
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Unsuccessful Delete Plugin {plugin.Id} => {httpResponse.StatusCode} => {errorContent}");
                TempData["ErrorMessage"] = "Error!";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostPluginSaveAsync(Plugin plugin)
        {
            if (!ModelState.IsValid || plugin?.Id == null) { return Page(); }

            SetView("details", plugin.Id);

            HttpResponseMessage? httpResponse = default;

            if (plugin.Id == "new") {

                plugin.Id = string.Empty;

                httpResponse = await _authorityService.GetHttpClient().PostAsJsonAsync("manage/plugin", plugin);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var id = JsonSerializer.Deserialize<Plugin>(await httpResponse.Content.ReadAsStringAsync())?.Id;
                    SetView("details", id);
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Save Plugin {plugin.Id} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }

            } else {

                httpResponse = await _authorityService.GetHttpClient().PutAsJsonAsync($"manage/plugin", plugin);

                if (httpResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Update Plugin {plugin.Id} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }
            }           
            
            return RedirectToPage();
        }

        // FUNCTIONS //

        public async Task<IActionResult> OnPostFunctionSaveAsync(Plugin plugin, int idx)
        {   
            var function = plugin.Functions[idx];

            _logger.LogInformation("Function: " + JsonSerializer.Serialize(function));

            if (!ModelState.IsValid || function?.Id == null) { 

                _logger.LogError("Invalid ModelState or Function Id");
                return Page(); 
            }

            SetView("functions", plugin.Id);

            HttpResponseMessage? httpResponse = default;

            if (function.Id == "new")
            {
                function.Id = string.Empty;

                httpResponse = await _authorityService.GetHttpClient().PostAsJsonAsync($"manage/plugin/{plugin.Id}/function", function);

                if (httpResponse.IsSuccessStatusCode)
                {   
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Save Function {function.Name} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }
            }
            else
            {
                httpResponse = await _authorityService.GetHttpClient().PutAsJsonAsync($"manage/function", function);

                if (httpResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Update Function {function.Id} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostFunctionDeleteAsync(Plugin plugin, int idx)
        {
            SetView("functions", plugin.Id);

            var function = plugin.Functions[idx];

            if (!ModelState.IsValid || function.Id == null) { return Page(); }

            var httpResponse = await _authorityService.GetHttpClient().DeleteAsync($"manage/function/{Uri.EscapeDataString(function.Id)}");

            if (httpResponse.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Success!";
            }
            else
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Unsuccessful Delete Function {function.Id} => {httpResponse.StatusCode} => {errorContent}");
                TempData["ErrorMessage"] = "Error!";
            }

            return RedirectToPage();
        }

        // CONNECTIONS //

        public async Task<IActionResult> OnPostConnectionSaveAsync(Plugin plugin, int idx)
        {
            var connection = plugin.Connections[idx];

            _logger.LogInformation("Connection: " + JsonSerializer.Serialize(connection));

            if (!ModelState.IsValid || connection?.Id == null)
            {

                _logger.LogError("Invalid ModelState or Connection Id");
                return Page();
            }

            SetView("connections", plugin.Id);

            HttpResponseMessage? httpResponse = default;

            if (connection.Id == "new")
            {
                connection.Id = string.Empty;

                httpResponse = await _authorityService.GetHttpClient().PostAsJsonAsync($"manage/plugin/{plugin.Id}/connection", connection);

                if (httpResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Save Connection {connection.Name} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }
            }
            else
            {
                httpResponse = await _authorityService.GetHttpClient().PutAsJsonAsync($"manage/connection", connection);

                if (httpResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Update Function {connection.Id} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostConnectionDeleteAsync(Plugin plugin, int idx)
        {
            SetView("connections", plugin.Id);

            var connection = plugin.Connections[idx];

            if (!ModelState.IsValid || connection.Id == null) { return Page(); }

            var httpResponse = await _authorityService.GetHttpClient().DeleteAsync($"manage/connection/{Uri.EscapeDataString(connection.Id)}");

            if (httpResponse.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Success!";
            }
            else
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Unsuccessful Delete Connection {connection.Id} => {httpResponse.StatusCode} => {errorContent}");
                TempData["ErrorMessage"] = "Error!";
            }

            return RedirectToPage();
        }
    }
}