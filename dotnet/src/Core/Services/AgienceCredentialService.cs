namespace Agience.Core.Services
{
    public class AgienceCredentialService
    {
        private readonly string _agentId;
        private readonly Authority _authority;
        private readonly Broker _broker;

        private readonly Dictionary<string, string> _credentials = new();

        public AgienceCredentialService(
            string agentId, 
            Authority authority, 
            Broker broker
            )
        {
            _agentId = agentId;
            _authority = authority;
            _broker = broker;
        }

        public string? GetCredential(string name)
        {
            if ( _credentials.ContainsKey(name)) {
                return _credentials[name];
            }

            // TODO: Get credential from Authority or Authorizer

            return null;
        }

        internal void AddCredential(string name, string credential)
        {
            _credentials[name] = credential;            
        }
    }
}