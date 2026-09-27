using Dahlia.Leveling;
using Dahlia.Mentioning;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Dahlia.Events;

public class MessageCreateHandler(RestClient client) : IMessageCreateGatewayHandler
{
    public async ValueTask HandleAsync(Message message)
    {
        if(message.Author.IsBot) return;

        var levelingSystem = ServiceStore.GetService<ILevelingSystem>();
        await levelingSystem.ProcessUser(client, message.Author.Id);

        var cacheStore = ServiceStore.GetService<DiscordCacheService>();
        if (!message.Content.ToLower().Contains($"<@{cacheStore.SelfUser.Id}>")) return;

        var detectTone = ServiceStore.GetService<DetectTone>();
        await client.SendMessageAsync(message.ChannelId, detectTone.Predict(message.Content, message.Author.Id));   
        return;
    }
}
