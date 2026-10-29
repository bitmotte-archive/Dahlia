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

        if(message.Content.StartsWith("DahliaPingPongMessage>>>"))
        {
            if (message.Guild == null)
            {
                await client.SendMessageAsync(message.ChannelId, "twin you can only use this in servers");
                return;
            }

            var member = await message.Guild.GetUserAsync(message.Author.Id);
            var hasRole = member.RoleIds.Any((v) => v == 1559681998694649956);

            if(!hasRole) {
                await client.SendMessageAsync(message.ChannelId, "You do not have permission to run this command.");
                return;
            }

            string trimmed = message.Content;
            trimmed = trimmed.Replace("DahliaPingPongMessage>>>", "");

            MessageProperties pingPongMessage = new()
            {
                Content = trimmed
            };

            var attachments = message.Attachments;
            var attachmentCount = message.Attachments.Count;

            AttachmentProperties[] pingPongAttachments = new AttachmentProperties[attachmentCount];
            var httpClient = new HttpClient();
            await Parallel.ForAsync(0, attachmentCount, async (i, ct) =>
            {
                var attachment = attachments[i];
                var res = await httpClient.GetAsync(attachment.Url);
                var stream = await res.Content.ReadAsStreamAsync();
                pingPongAttachments[i] = new AttachmentProperties(attachment.FileName, stream);
            });

            pingPongMessage.Attachments = pingPongAttachments;

            await client.SendMessageAsync(message.ChannelId, pingPongMessage);
            try
            {
                await client.DeleteMessageAsync(message.ChannelId, message.Id);
            } catch (RestException e)
            {
                if (e.Error?.Code != 403) throw e;
                Console.Out.WriteLine($"No permission to delete message");
            }
        }

        if(message.Content.ToLower().StartsWith("dgenerate"))
        {
            MessageProperties messafe = new()
            {
                Content = "here you go"
            };

            AttachmentProperties[] attachments = new AttachmentProperties[1];
            var httpClient = new HttpClient();
            Random rng = new();

            HttpResponseMessage res = null; 
            if(rng.Next(1, 100) == 67)
            {
                res = await httpClient.GetAsync("");
            } else
            {
                res = await httpClient.GetAsync("https://raw.githubusercontent.com/bitmotte-dev/images/refs/heads/main/negative-bass.jpg");
            }
            var stream = await res.Content.ReadAsStreamAsync();
            attachments[0] = new AttachmentProperties("https://raw.githubusercontent.com/bitmotte-dev/images/refs/heads/main/low-quality-bass.jpg", stream);

            messafe.Attachments = attachments;

            await client.SendMessageAsync(message.ChannelId, messafe);
        }

        var cacheStore = ServiceStore.GetService<DiscordCacheService>();
        if (!message.Content.ToLower().Contains($"<@{cacheStore.SelfUser.Id}>")) return;

        var detectTone = ServiceStore.GetService<DetectTone>();
        await client.SendMessageAsync(message.ChannelId, detectTone.Predict(message.Content, message.Author.Id));   
        return;
    }
}
