using Microsoft.AspNetCore.Authentication;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Agience.Authority.Manage.Models
{
    public class Diagnostic
    {
        public AuthenticateResult AuthenticateResult { get; }
        public IEnumerable<string> Clients { get; } = new string[0];

        public Diagnostic(AuthenticateResult result)
        {
            AuthenticateResult = result;

            if (result.Properties?.Items.ContainsKey("client_list") == true)
            {
                var encoded = result.Properties.Items["client_list"];                
                var value = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(encoded));

                Clients = JsonSerializer.Deserialize<string[]>(value) ?? new string[0];
            }
        }
    }
}