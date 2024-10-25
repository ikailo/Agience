using Duende.IdentityServer.Services;
using Duende.IdentityServer.Extensions;

namespace Agience.Authority.Identity.Services
{
    public class AgienceServerUrls : DefaultServerUrls
    {
        public const string IdentityServerBasePath = "idsvr:IdentityServerBasePath";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AgienceServerUrls(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor) {

            _httpContextAccessor = httpContextAccessor;
        }

        public new string Origin
        {
            get
            {
                var request = _httpContextAccessor.HttpContext?.Request;

                if (request == null)
                {
                    return string.Empty;
                }

                return $"{request.Scheme}://{request.Host}"; // This keeps the port if present
            }
            set
            {
                var split = value.Split(new[] { "://" }, StringSplitOptions.RemoveEmptyEntries);

                var request = _httpContextAccessor.HttpContext?.Request;

                if (request == null)
                {
                    return;
                }

                request.Scheme = split.First();
                request.Host = new HostString(split.Last());
            }
        }

        public new string BasePath
        {
            get
            {
                return _httpContextAccessor.HttpContext.Items[IdentityServerBasePath] as string;
            }
            set
            {
                _httpContextAccessor.HttpContext.Items[IdentityServerBasePath] = value;//.RemoveTrailingSlash();
            }
        }

    }
}
