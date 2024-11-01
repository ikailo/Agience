using Microsoft.SemanticKernel;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace Agience.Plugins.Core.Google
{  

    public class GmailServiceHelper
    {
        private static readonly string[] Scopes = { GmailService.Scope.GmailReadonly };
        private const string ApplicationName = "Gmail API .NET Quickstart";

        private readonly GmailService _service;

        public GmailServiceHelper(string credentialsPath, string tokenPath)
        {
            UserCredential credential;

            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                string credPath = tokenPath;
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Gmail API service.
            _service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public async Task<IList<Message>> GetEmailsWithLabelAsync(string labelId)
        {
            var request = _service.Users.Messages.List("me");
            request.LabelIds = labelId;
            request.IncludeSpamTrash = false;

            var response = await request.ExecuteAsync();
            return response.Messages;
        }

        public async Task<IList<Message>> GetEmailsWithLabelDetailsAsync(string labelId)
        {
            var messages = await GetEmailsWithLabelAsync(labelId);
            var result = new List<Message>();

            foreach (var message in messages)
            {
                var msg = await _service.Users.Messages.Get("me", message.Id).ExecuteAsync();
                result.Add(msg);
            }

            return result;
        }
    }

}
