using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NiceBlockBot.Commads;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NiceBlockBot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }

        public async Task RunAsync()
        {
            var json = string.Empty;

            using (var file = File.OpenRead("config.json"))
            using (var streamReader = new StreamReader(file, new UTF8Encoding(false)))
                json = await streamReader.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };

            Client = new DiscordClient(config);
            Client.Ready += OnClientReady;
            Client.GuildMemberAdded += OnGuildMemberAdded;
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                DmHelp = true,
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            //Commands.RegisterCommands<TeamCommands>();
            Commands.RegisterCommands<FunCommands>();
            Commands.RegisterCommands<LobbyCommands>();
            Commands.RegisterCommands<ChatCommands>();

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private async Task OnGuildMemberAdded(GuildMemberAddEventArgs e)
        {
            DiscordChannel welcomeChannel = await e.Client.GetChannelAsync(752942323243155597);
            await welcomeChannel.SendMessageAsync($"Даров, {e.Member.Mention}!");
            await e.Member.GrantRoleAsync(e.Guild.GetRole(754494905199493150));
        }

        private Task OnClientReady(ReadyEventArgs e) {
            return null;
        }
    }
}
