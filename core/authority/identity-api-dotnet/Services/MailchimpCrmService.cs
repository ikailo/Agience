using Agience.Authority.Identity.Services;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class MailchimpService : ICrmService
{
    private readonly HttpClient _httpClient;
    private readonly string _audienceId;
    private readonly string[] _tags;
    private readonly ILogger<MailchimpService> _logger;

    public MailchimpService(HttpClient httpClient, string apiKey, string audienceId, string[] tags, ILogger<MailchimpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _audienceId = audienceId;
        _tags = tags;
        
        var dataCenter = apiKey.Split('-')[1];

        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"anystring:{apiKey}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        _httpClient.BaseAddress = new Uri($"https://{dataCenter}.api.mailchimp.com/3.0/");
    }

    public async Task AddSubscriberAsync(string email, string? firstName, string? lastName)
    {
        _logger.BeginScope("MailchimpService.AddSubscriberAsync");

        _logger.LogInformation("Signing up new person");

        var payload = new
        {
            status = "subscribed",
            email_address = email,            
            merge_fields = new
            {
                FNAME = firstName,
                LNAME = lastName
            },
            tags = _tags
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"lists/{_audienceId}/members", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to add subscriber to Mailchimp: {error}", error);
            // TODO: We should have a backup service, but we can get the data from the People table if needed.
        }
        else
        {
            _logger.LogInformation("Successfully signed up new person");
        }
    }
}
