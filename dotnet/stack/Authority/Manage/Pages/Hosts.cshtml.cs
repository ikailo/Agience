using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using Host = Agience.Authority.Models.Manage.Host;
using Agience.Authority.Models.Manage;
using Agience.Authority.Manage.Web.Services;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Agience.Authority.Manage.Web.Pages
{
    public class HostsModel : PageModel
    {
        [BindProperty]
        public List<Host> Hosts { get; set; } = new();

        public List<Plugin> FindPluginResults { get; set; } = new();

        public KeyValuePair<string, string>? Credentials { get; private set; }

        private readonly ILogger<HostsModel> _logger;
        private readonly AgienceAuthorityService _authorityService;

        public HostsModel(AgienceAuthorityService authorityService, ILogger<HostsModel> logger)
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
            var httpResponse = await _authorityService.GetHttpClient().GetAsync("manage/hosts");

            if (httpResponse.IsSuccessStatusCode)
            {
                Hosts = JsonSerializer.Deserialize<List<Host>>(await httpResponse.Content.ReadAsStringAsync()) ?? new();

                Hosts.Add(new Host() { Id = "new" });

                foreach (var host in Hosts)
                {
                    if (host.Id != "new")
                    {
                        host.Keys.Add(new Key() { Id = "new" });
                    }
                }

                SetView(TempData["ActiveTab"] as string ?? "details", TempData["ActivePage"] as string ?? Hosts.FirstOrDefault()?.Id);
            }
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

        public async Task<IActionResult> OnPostAddPluginAsync(string hostsJson, string findPluginResultsJson, string hostId, string pluginId)
        {
            if (string.IsNullOrWhiteSpace(pluginId) || string.IsNullOrWhiteSpace(hostId) || string.IsNullOrWhiteSpace(hostsJson))
            {
                return BadRequest(new { ErrorMessage = "Invalid pluginId, hostsJson, or hostId." });
            }

            SetView("plugins", hostId);

            Hosts = JsonSerializer.Deserialize<List<Host>>(hostsJson) ?? new();

            var plugin = JsonSerializer.Deserialize<List<Plugin>>(findPluginResultsJson)?.Find(p => p.Id == pluginId);

            if (string.IsNullOrWhiteSpace(plugin?.Id))
            {
                return BadRequest(new { ErrorMessage = "Plugin not found." });
            }

            var httpResponse = await _authorityService.GetHttpClient().PutAsync($"manage/host/{hostId}/plugin/{plugin.Id}", null);

            if (httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Success adding plugin to host: {httpResponse.StatusCode}");

                Hosts.Find(h => h.Id == hostId)?.Plugins.Add(plugin);
                return Page();
            }

            _logger.LogError($"Error adding plugin to host: {httpResponse.StatusCode} => {await httpResponse.Content.ReadAsStringAsync()}");
            return Page();
        }


        public async Task<IActionResult> OnPostRemovePluginAsync(string hostId, string pluginId)
        {
            if (string.IsNullOrWhiteSpace(hostId) || string.IsNullOrWhiteSpace(pluginId))
            {
                return BadRequest(new { ErrorMessage = "Host ID and Plugin ID are required." });
            }

            SetView("plugins", hostId);

            var httpResponse = await _authorityService.GetHttpClient().DeleteAsync($"manage/host/{hostId}/plugin/{pluginId}");

            if (httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Success removing plugin to host: {httpResponse.StatusCode}");

                TempData["SuccessMessage"] = "Success!";
                return RedirectToPage();
            }

            _logger.LogError($"Error fetching plugins: {httpResponse.StatusCode} => {await httpResponse.Content.ReadAsStringAsync()}");

            return RedirectToPage();
        }

        public Task<IActionResult> OnPostAsync(string hostsJson, string activeTab, string activePage)
        {
            _logger.LogInformation("OnPostAsync");

            Hosts = JsonSerializer.Deserialize<List<Host>>(hostsJson) ?? new();

            SetView(activeTab, activePage);

            return Task.FromResult((IActionResult)Page());
        }

        public async Task<IActionResult> OnPostHostDeleteAsync(Host host)
        {
            if (host.Id == null) { return Page(); }

            SetView("details", host.Id);

            var httpResponse = await _authorityService.GetHttpClient().DeleteAsync($"manage/host/{Uri.EscapeDataString(host.Id)}");

            if (httpResponse.IsSuccessStatusCode)
            {
                SetView("details", null);
                TempData["SuccessMessage"] = "Success!";
            }
            else
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Unsuccessful Delete Host {host.Id} => {httpResponse.StatusCode} => {errorContent}");
                TempData["ErrorMessage"] = "Error!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostHostSaveAsync(Host host)
        {
            if (!ModelState.IsValid || host?.Id == null) { return Page(); }

            SetView("details", host.Id);

            HttpResponseMessage? httpResponse = default;

            if (host.Id == "new")
            {
                host.Id = string.Empty;

                httpResponse = await _authorityService.GetHttpClient().PostAsJsonAsync("manage/host", host);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var id = JsonSerializer.Deserialize<Host>(await httpResponse.Content.ReadAsStringAsync())?.Id;
                    SetView("details", id);
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Save Host {host.Id} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }
            }
            else
            {
                httpResponse = await _authorityService.GetHttpClient().PutAsJsonAsync($"manage/host", host);

                if (httpResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Update Host {host.Id} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }
            }

            return RedirectToPage();
        }

        private static JsonWebKey GenerateRsaJsonWebKey(out RSAParameters privateKeyParameters)
        {
            using (var rsa = RSA.Create(2048))
            {
                privateKeyParameters = rsa.ExportParameters(true);
                var publicKeyParameters = rsa.ExportParameters(false);

                var jwk = new JsonWebKey
                {
                    Kty = "RSA",
                    N = Base64UrlEncoder.Encode(publicKeyParameters.Modulus),
                    E = Base64UrlEncoder.Encode(publicKeyParameters.Exponent)
                };

                return jwk;
            }
        }

        private static string DecryptWithJWK(string encryptedValue, RSAParameters privateKeyParameters)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportParameters(privateKeyParameters);

                var encryptedBytes = Convert.FromBase64String(encryptedValue);
                var decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }

        /*
         * private void FixModelForNewRecords(Host host)
        {
            var newKeyIndex = host.Keys.FindIndex(k => k.Id == "new");

            if (newKeyIndex != -1)
            {
                ModelState.Remove($"host.Keys[{newKeyIndex}].Name");
            }
        }*/

        public async Task<IActionResult> OnPostKeySaveAsync(Host host, int idx)
        {
            if (host == null || idx  < 0 || idx >= host.Keys.Count)
            {
                return BadRequest(new { ErrorMessage = "Invalid hostId or index." });
            }

            var key = host.Keys[idx];

            if (!ModelState.IsValid || key?.Id == null)
            {
                return Page();
            }

            SetView("keys", host.Id);

            if (key.Id == "new")
            {
                if (string.IsNullOrEmpty(key.Name))
                {
                    TempData["ErrorMessage"] = "Error! Key name is required.";
                    return RedirectToPage();
                }

                var jsonWebKey = GenerateRsaJsonWebKey(out RSAParameters privateKeyParameters);

                var requestPayload = new
                {
                    name = key.Name,
                    json_web_key = jsonWebKey
                };

                var httpResponse = await _authorityService.GetHttpClient().PostAsJsonAsync($"manage/host/{host.Id}/key/generate", requestPayload);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Generate Key {key.Name} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = $"Failed to generate key.";

                    return RedirectToPage();
                }

                var result = await httpResponse.Content.ReadAsStringAsync();

                var generatedKey = JsonSerializer.Deserialize<Key>(result);

                if (generatedKey == null || string.IsNullOrEmpty(generatedKey.Value))
                {
                    TempData["ErrorMessage"] = $"Failed to retrieve the generated key";
                    return RedirectToPage();
                }

                // Expect encrypted if we provided JWK
                if (jsonWebKey != null && !generatedKey.IsEncrypted)
                {
                    _logger.LogWarning("Key was not encrypted. Expected encrypted key.");
                }

                var decryptedValue = generatedKey.IsEncrypted ? DecryptWithJWK(generatedKey.Value, privateKeyParameters) : generatedKey.Value;

                TempData["HostCredentials"] = JsonSerializer.Serialize(new { host_id = host.Id, host_secret = decryptedValue });

            }
            else
            {
                var httpResponse = await _authorityService.GetHttpClient().PutAsJsonAsync($"manage/key", key);

                if (httpResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Success!";
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Unsuccessful Update Key {host.Id} => {httpResponse.StatusCode} => {errorContent}");
                    TempData["ErrorMessage"] = "Error!";
                }
            }

            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostKeyDeleteAsync(Host host, int idx)
        {
            var key = host.Keys[idx];

            SetView("keys", host.Id);                        

            if (!ModelState.IsValid || key.Id == null) { return Page(); }

            var httpResponse = await _authorityService.GetHttpClient().DeleteAsync($"manage/key/{Uri.EscapeDataString(key.Id)}");

            if (httpResponse.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Success!";
            }
            else
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Unsuccessful Delete Key {host.Id} => {httpResponse.StatusCode} => {errorContent}");
                TempData["ErrorMessage"] = "Error!";
            }

            return RedirectToPage();
        }
    }
}