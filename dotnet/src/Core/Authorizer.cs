using System.Web;
using AutoMapper;

namespace Agience.Core
{
    [AutoMap(typeof(Models.Entities.Authorizer), ReverseMap = true)]
    public class Authorizer : Models.Entities.Authorizer
    {
        
        public Authorizer() { }

        public string? GetAuthorizationUri(string authorityUri, string state)
        {
            if (AuthType == Models.Entities.AuthorizationType.None)
            {
                return null;
            }
            else if (AuthType == Models.Entities.AuthorizationType.OAuth2)
            {
                var clientId = HttpUtility.UrlEncode(ClientId);
                var redirectUri = HttpUtility.UrlEncode($"{authorityUri}{RedirectUri}");
                var scope = HttpUtility.UrlEncode(Scope);                

                return $"{AuthUri}?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}&state={state}&prompt=consent&access_type=offline";
                
                // TODO: access_type and prompt are for Google only. Need to differentiate between providers.

            }
            else if (AuthType == Models.Entities.AuthorizationType.ApiKey)
            {
                return HttpUtility.UrlEncode($"{authorityUri}/manage/authorizer/{Id}/authorize?state={state}");
            }

            throw new InvalidOperationException("Unknown authorization type");            
        }
    }
}

