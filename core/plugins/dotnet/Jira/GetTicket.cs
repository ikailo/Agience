//using Agience.Core;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;

namespace Agience.Plugins.Core.Jira
{
    public class GetTicket
    {
        private const string USERNAME = "";
        private const string PASSWORD = "";
                
        //public override string[] InputKeys => ["domain", "issueID"];
        //public override string[] OutputKeys => ["deserializedData:JiraResModel"];
        /*
        [KernelFunction, Description("Get a Jira ticket by its ID.")]
        public Task<Data?> Process(Runner runner, Data? input = null)
        {
            throw new NotImplementedException();

            /*
            var domain = information.Input.Structured["domain"];
            var issueID = information.Input.Structured["issueID"];

            HttpClient client = new()
            {
                BaseAddress = new Uri($"https://{domain}.atlassian.net/rest/api/3/issue/{issueID}")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var byteArray = Encoding.ASCII.GetBytes($"{Username} : {Password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            try
            {
                // Get comments
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
                if (response.IsSuccessStatusCode)
                {
                    var responseResult = await response.Content.ReadAsStringAsync();
                    var deserializedData = JsonConvert.DeserializeObject<JiraResModel>(responseResult);
                    return Data.Create(deserializedData.ToString());
                }
                return Data.Create("Error", $"Unable to find the comments Status Code: '{response.StatusCode}'");
            }
            catch (Exception e)
            {
                return Data.Create(e);
            }
            
        }*/
    }
}