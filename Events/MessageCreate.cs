using System.Net;
using Dahlia.Leveling;
using Dahlia.Mentioning;
using NetCord;
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

        if(message.Content.Contains("DahliaPingPongMessage>>>"))
        {
            if(message.Author.Username != "bitmotte") {await client.SendMessageAsync(message.ChannelId, "Only <@1414406914669805662> can use this command");
            return;
            
            /*h*/}
            string trimmed = message.Content;
            trimmed = trimmed.Replace("DahliaPingPongMessage>>>", "");

            MessageProperties pingPongMessage = new();

            pingPongMessage.Content = trimmed;
            AttachmentProperties[] pingPongAttachments = [];
            WebClient wc = new WebClient();
            foreach (Attachment attachment in message.Attachments)
            {
                pingPongAttachments = [.. pingPongAttachments, new(attachment.FileName, new MemoryStream(wc.DownloadData(attachment.Url)))];
            }
            pingPongMessage.Attachments = pingPongAttachments;
            
            //pingPongMessage.Attachments 

            await client.SendMessageAsync(message.ChannelId, pingPongMessage);
            await client.DeleteMessageAsync(message.ChannelId, message.Id);
        }

        var cacheStore = ServiceStore.GetService<DiscordCacheService>();
        if (!message.Content.ToLower().Contains($"<@{cacheStore.SelfUser.Id}>")) return;

        var detectTone = ServiceStore.GetService<DetectTone>();
        await client.SendMessageAsync(message.ChannelId, detectTone.Predict(message.Content, message.Author.Id));   
        return;
    }
}
