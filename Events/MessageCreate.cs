using Dahlia.Leveling;
using Dahlia.Mentioning;
using Dahlia.Utilities;
using Microsoft.Extensions.Logging;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Dahlia.Events;

public class MessageCreateHandler(RestClient client, ILogger<MessageCreateHandler> logger) : IMessageCreateGatewayHandler
{
    public ValueTask HandleAsync(Message message)
    {
        logger.LogInformation("{}", message.Content);
        if(!message.Author.IsBot)
        {
            Leveller.ManageUser(client, message.Author.Id);

            bool dahliaConversation = false;
            if(message.Content.ToLower().Contains("<@1516464108092788950>")) dahliaConversation = true;
            if(dahliaConversation)
            {
                SendMessage.Send(client, message.ChannelId, DetectTone.Predict(message.Content));   
            }
        }
        return default;
    }
}
