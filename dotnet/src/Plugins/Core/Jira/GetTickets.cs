//using Agience.Core;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;

namespace Agience.Plugins.Core.Jira
{
    public class GetTickets 
    {
        private const string USERNAME = "";
        private const string PASSWORD = "";

        //public override string[] InputKeys => ["domain"];
        //public override string[] OutputKeys => ["domain"];
        /*
        [KernelFunction, Description("Get all Jira tickets.")]
        public Task<Data?> Process(Runner runner, Data? input = null)
        {
            throw new NotImplementedException();

            /*
            var domain = information.Input.Structured["domain"];

            HttpClient client = new()
            {
                BaseAddress = new Uri($"https://{domain}.atlassian.net/rest/api/3/search")
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
                    var content = await response.Content.ReadAsStringAsync();
                    return Data.Create(content);
                }
                return Data.Create("Error", $"Unable to find the Jira Ticket Status Code:'{response.StatusCode}'");
            }
            catch (Exception e)
            {
                return Data.Create(e);
            }
            
        } */
    }
}