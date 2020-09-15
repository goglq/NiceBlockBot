using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NiceBlockBot.Commads
{
    public class ChatCommands : BaseCommandModule
    {
        [Command("clear"), Hidden()]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Clear(CommandContext ctx, int limit = 100)
        {
            await ctx.Channel.DeleteMessagesAsync(await ctx.Channel.GetMessagesAsync(limit));
        }
    }
}
