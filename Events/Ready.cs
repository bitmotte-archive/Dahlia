using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Dahlia.Events;

public class ReadyHandler : IReadyGatewayHandler
{
    public async ValueTask HandleAsync(ReadyEventArgs ev)
    {
        var cacheService = ServiceStore.GetService<DiscordCacheService>();
        cacheService.SelfUser = ev.User;
        return;
    }
}
