using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace NiceBlockBot.Commads
{
    public class TeamCommands : BaseCommandModule
    {
        [Command("join")]
        [Description("Returns Pong")]
        public async Task Join(CommandContext ctx)
        {
            var joinEmbed = new DiscordEmbedBuilder()
            {
                Title = "Would you like to join?",
                ImageUrl = ctx.Client.CurrentUser.AvatarUrl,
                Color = DiscordColor.Green
            };

            var joinMessage = await ctx.Channel.SendMessageAsync(embed: joinEmbed);

            var thumbsUpEmoji = DiscordEmoji.FromName(ctx.Client, ":+1:");
            var thumbsDawnEmoji = DiscordEmoji.FromName(ctx.Client, ":-1:");

            await joinMessage.CreateReactionAsync(thumbsUpEmoji);
            await joinMessage.CreateReactionAsync(thumbsDawnEmoji);

            var interactivity = ctx.Client.GetInteractivity();

            var reactionResult = await interactivity.WaitForReactionAsync(
                x => x.Message == joinMessage && 
                x.User == ctx.User &&
                (x.Emoji == thumbsUpEmoji || x.Emoji == thumbsDawnEmoji));

            if(reactionResult.Result.Emoji == thumbsUpEmoji)
            {
                var role = ctx.Guild.GetRole(752962491570716692);
                await ctx.Member.GrantRoleAsync(role);
            }

            await joinMessage.DeleteAsync();
        }
    }
}
