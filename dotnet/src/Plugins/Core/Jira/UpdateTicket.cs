//using Agience.Core;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

namespace Agience.Plugins.Core.Jira
{
    public class UpdateTicket
    {
  /*      
        //public override string[] InputKeys => [ "domain", "issueID", "editObject" ];
        //public override string[] OutputKeys => [ "content" ];

        [KernelFunction, Description("Update Jira Ticket By Id")]
        public Task<Data?> Process(Runner runner, Data? input = null)
        {
            throw new NotImplementedException();

            /*
             var domain = (string)information.Input.Structured["domain"];
            var issueID = (string)information.Input.Structured["issueID"];
            var editObject = (string)information.Input.Structured["editObject"];

            var jsonData = JsonConvert.SerializeObject(editObject);
            var byteArray = Encoding.ASCII.GetBytes($"{Username}:{Password}");

            try
            {
                var url = new Uri($"https://{domain}.atlassian.net/rest/api/3/issue/{issueID}/comment");
                // Post comments
                using var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                using var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Data.Create(content);
                }
                return Data.Create("Error", $"Unable to Update Jira Tickets Status Code : '{response.StatusCode}'");
            }
            catch (Exception e)
            {
                return Data.Create(e);
            }
            
        }*/
    }
}