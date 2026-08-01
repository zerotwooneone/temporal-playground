using Microsoft.AspNetCore.SignalR;

namespace TemporalDDD.Api.Messaging;

public class ApplicationEventHub : Hub
{
    public async Task JoinProviderGroup(string providerPublicId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, providerPublicId);
    }

    public async Task LeaveProviderGroup(string providerPublicId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, providerPublicId);
    }
}
