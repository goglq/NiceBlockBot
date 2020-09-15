using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NiceBlockBot.Exceptions
{
    public class NBException : ApplicationException
    {
        public NBException() : base() { }
        public NBException(string message) : base(message) { }

        public virtual async Task SendException(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(Message);
        }
    }
}
