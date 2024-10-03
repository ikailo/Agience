//using Agience.SDK;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Agience.Plugins.Primary.Jira
{
    public class CreateComment
    {
        private const string USERNAME = "";
        private const string PASSWORD = "";

        //public override Data? Description => "Post Jira Comments";
        //public override string[] InputKeys => [ "domain", "issueID", "comment" ];
        //public override string[] OutputKeys => [ "content" ];
        /*
        [KernelFunction, Description("Post a comment to a Jira issue.")]
        public Task<Data?> NewComment(Runner runner, Data? input = null)
        {
            throw new NotImplementedException();
        */
            /*
            var domain = information.Input.Structured["domain"];
            var issueID = information.Input.Structured["issueID"];
            var textComment = information.Input.Structured["textComment"];
            var configs = new
            {
                body = new
                {
                    content = new[] {
                        new { content = new [] {
                        new { text = textComment, type = "text"} }, type = "paragraph"}
                    },
                    type = "doc",
                    version = 1
                }
            };
            var jsonData = JsonConvert.SerializeObject(configs);
            var byteArray = Encoding.ASCII.GetBytes($"{Username} : {Password}");

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
                return Data.Create("Error", $"Unable to Post Jira Comments Status Code: '{response.StatusCode}'");
            }
            catch (Exception e)
            {
                return Data.Create(e);
            }
           
        }*/
    }
}