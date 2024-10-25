using Microsoft.AspNetCore.SignalR;

namespace Agience.Authority.Manage.Web.Services
{
    public class AgienceChatHub : Hub
    {
        public async Task AddToGroup(string agencyId)
        {
            // TODO: SECURITY: Ensure the User is director of the Agency.
            await Groups.AddToGroupAsync(Context.ConnectionId, agencyId);
        }
    }
}
