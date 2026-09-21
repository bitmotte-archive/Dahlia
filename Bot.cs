using Dahlia.Leveling;
using Dahlia.Mentioning;
using dotenv.net;
using Microsoft.Extensions.Hosting;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Dahlia;

static class Bot
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        DotEnv.Load();

        builder.Services.AddDiscordGateway(options =>
        {
            options.Intents = GatewayIntents.GuildMessages | GatewayIntents.MessageContent;
            options.Token = Environment.GetEnvironmentVariable("WowTHisIsTheEnvironmentVariableForTheDahliaDiscordBotTokenForBluhenServerSoCool");
        }).AddGatewayHandlers(typeof(Bot).Assembly);
        
        var host = builder.Build();
        
        Leveller.Init();
        DetectTone.Init();

        await host.RunAsync();
    }
} 