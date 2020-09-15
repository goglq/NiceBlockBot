using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NiceBlockBot.Exceptions
{
    public class NBWaitRoomException : NBException
    {
        public override async Task SendException(CommandContext ctx)
        {
            Console.WriteLine(ctx.User.Mention);
            await ctx.Channel.SendMessageAsync($"{ctx.User.Mention} Enter to Wait Room");
        }
    }
}
