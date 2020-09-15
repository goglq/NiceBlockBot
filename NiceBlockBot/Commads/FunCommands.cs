using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;

namespace NiceBlockBot.Commads
{
    public class FunCommands : BaseCommandModule
    {
        [Command("ping")]
        [Description("Returns Pong")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync($"@{ctx.Member.Username} Pong!")
                .ConfigureAwait(false);
        }

        [Command("add")]
        [Description("Adds two numbers together")]
        [RequireRoles(RoleCheckMode.Any, "Moderator")]
        public async Task Add(CommandContext ctx, [Description("Number A")]int a, [Description("Number B")]int b)
        {
            await ctx.Channel.SendMessageAsync((a + b).ToString())
                .ConfigureAwait(false);
        }

        [Command("respondmessage")]
        public async Task RespondMessage(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel)
                .ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Content);
        }

        [Command("respondreaction")]
        public async Task RespondReaction(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForReactionAsync(x => x.Channel == ctx.Channel)
                .ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Emoji);

        }

        [Command("poll")]
        public async Task Poll(CommandContext ctx, TimeSpan duration, params DiscordEmoji[] emojiOptions)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var options = emojiOptions.Select(x => x.ToString());

            var embed = new DiscordEmbedBuilder
            {
                Title = "Poll",
                Description = string.Join(" ", options),
            };

            var pollMessage = await ctx.Channel.SendMessageAsync(embed: embed);

            foreach(var option in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(option);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration);

            var results = result.Distinct()
                                .Select(x => $"{x.Emoji}: {x.Total}");

            await ctx.Channel.SendMessageAsync(string.Join("\n", results));
        }
    }
}
