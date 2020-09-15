using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using NiceBlockBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NiceBlockBot.Commads
{
    class LobbyCommands : BaseCommandModule
    {
        private const ulong ParentCategoryId = 754408581167710358;
        private const ulong WaitChannelId = 754427337692545125;
        private const ulong DefaultRole = 752941689638748332;

        private const string EmojiTeam1String = ":green_apple:";
        private const string EmojiTeam2String = ":apple:";

        [Command("cpl")]
        public async Task CreatePrivateLobby(CommandContext ctx, string lobbyName, params DiscordMember[] allowedUsers)
        {
            try
            {
                await ctx.Channel.DeleteMessageAsync(ctx.Message);
                DiscordChannel parentCategory = await ctx.Client.GetChannelAsync(ParentCategoryId);
                DiscordChannel waitChannel = await ctx.Client.GetChannelAsync(WaitChannelId);

                if (!waitChannel.Users.Contains(ctx.User))
                    throw new NBWaitRoomException();

                DiscordRole createdRole = await ctx.Guild.CreateRoleAsync(lobbyName);
                DiscordChannel createdLobby = await ctx.Guild.CreateVoiceChannelAsync(
                    lobbyName == "_" ? $"Lobby - {ctx.Member.DisplayName}" : lobbyName,
                    parent: parentCategory);

                await createdLobby.PlaceMemberAsync(ctx.Member);

                await createdLobby.AddOverwriteAsync(ctx.Guild.GetRole(DefaultRole), deny: Permissions.AccessChannels);
                await createdLobby.AddOverwriteAsync(createdRole, allow: Permissions.AccessChannels);
                await createdLobby.AddOverwriteAsync(ctx.Member, allow: Permissions.MuteMembers | Permissions.DeafenMembers);

                IReadOnlyCollection<DiscordMember> allMembers = await ctx.Guild.GetAllMembersAsync();
                await ctx.Member.GrantRoleAsync(createdRole);

                foreach (DiscordMember member in allowedUsers)
                    await member.GrantRoleAsync(createdRole);

                while (true)
                {
                    await Task.Delay(5000);
                    if (createdLobby.Users.Count() <= 0)
                    {
                        await createdLobby.DeleteAsync();
                        await createdRole.DeleteAsync();
                        return;
                    }
                }
            }
            catch (NBException ex)
            {
                await ex.SendException(ctx);
            }
        }

        [Command("cl")]
        public async Task CreateLobby(CommandContext ctx, string lobbyName, int userLimit = 20)
        {
            try
            {
                await ctx.Channel.DeleteMessageAsync(ctx.Message);
                DiscordChannel parentCategory = await ctx.Client.GetChannelAsync(ParentCategoryId);
                DiscordChannel waitChannel = await ctx.Client.GetChannelAsync(WaitChannelId);

                if (!waitChannel.Users.Contains(ctx.User))
                    throw new NBWaitRoomException();

                DiscordChannel createdLobby = await ctx.Guild.CreateChannelAsync(
                    lobbyName == "_" ? $"Lobby - {ctx.Member.DisplayName}" : lobbyName,
                    ChannelType.Voice,
                    parent: parentCategory,
                    userLimit: userLimit);

                await createdLobby.PlaceMemberAsync(ctx.Member);

                await createdLobby.AddOverwriteAsync(ctx.Member, allow: Permissions.MuteMembers | Permissions.DeafenMembers);

                while (true)
                {
                    await Task.Delay(5000);
                    if (createdLobby.Users.Count() <= 0)
                    {
                        await createdLobby.DeleteAsync();
                        return;
                    }
                }
            }
            catch(NBException ex)
            {
                await ex.SendException(ctx);
            }
        }

        [Command("tvt")]
        public async Task TeamVersusTeam(CommandContext ctx, 
            int size1, string team1, 
            int size2, string team2,
            TimeSpan duration)
        {
            try
            {
                await ctx.Channel.DeleteMessageAsync(ctx.Message);

                var interactivity = ctx.Client.GetInteractivity();
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = $"{team1} vs {team2}",
                    Color = DiscordColor.Cyan
                };

                DiscordMessage embedMessage = await ctx.Channel.SendMessageAsync(embed: embed);

                DiscordEmoji team1Emoji = DiscordEmoji.FromName(ctx.Client, EmojiTeam2String);
                DiscordEmoji team2Emoji = DiscordEmoji.FromName(ctx.Client, EmojiTeam2String);

                await embedMessage.CreateReactionAsync(team1Emoji);
                await embedMessage.CreateReactionAsync(team2Emoji);

                var reactions = await interactivity.CollectReactionsAsync(embedMessage, duration);

                List<DiscordUser> team1Users = new List<DiscordUser>();
                List<DiscordUser> team2Users = new List<DiscordUser>();
                SplitTeams(ctx, team1Emoji, reactions, team1Users, team2Users);

                Console.WriteLine("Finish placing users");

                foreach (DiscordUser user in team1Users)
                    Console.WriteLine($"{user.IsBot} {user.Username}");

                foreach (DiscordUser user in team2Users)
                    Console.WriteLine($"{user.IsBot} {user.Username}");

                team1Users.ForEach(user => team2Users.Remove(user));
                team2Users.ForEach(user => team1Users.Remove(user));

                DiscordRole team1Role = await ctx.Guild.CreateRoleAsync($"Team {team1}");
                DiscordRole team2Role = await ctx.Guild.CreateRoleAsync($"Team {team2}");

                team1Users
                    .Take(size1)
                    .ToList()
                    .ForEach(async user => await (await ctx.Guild.GetMemberAsync(user.Id)).GrantRoleAsync(team1Role));

                team2Users
                    .Take(size2)
                    .ToList()
                    .ForEach(async user => await (await ctx.Guild.GetMemberAsync(user.Id)).GrantRoleAsync(team2Role));

                DiscordChannel parentCategory = await ctx.Guild.CreateChannelCategoryAsync($"{team1} vs {team2}");

                await parentCategory.AddOverwriteAsync(ctx.Guild.GetRole(DefaultRole), deny: Permissions.AccessChannels);
                await parentCategory.AddOverwriteAsync(team1Role, allow: Permissions.AccessChannels);
                await parentCategory.AddOverwriteAsync(team2Role, allow: Permissions.AccessChannels);

                DiscordChannel commonChannel = await ctx.Guild.CreateVoiceChannelAsync(
                    "Common",
                    parent: parentCategory,
                    user_limit: size1 + size2
                    );

                DiscordChannel team1Lobby = await ctx.Guild.CreateVoiceChannelAsync(
                    team1,
                    parent: parentCategory,
                    user_limit: size1);

                DiscordChannel team2Lobby = await ctx.Guild.CreateVoiceChannelAsync(
                    team2,
                    parent: parentCategory,
                    user_limit: size2);

                await team1Lobby.AddOverwriteAsync(team2Role, deny: Permissions.AccessChannels);
                await team2Lobby.AddOverwriteAsync(team1Role, deny: Permissions.AccessChannels);
                await team1Lobby.AddOverwriteAsync(team1Role, allow: Permissions.AccessChannels);
                await team2Lobby.AddOverwriteAsync(team2Role, allow: Permissions.AccessChannels);


                DiscordChannel textChannel = await ctx.Guild.CreateTextChannelAsync(
                    "Common Chat",
                    parent: parentCategory
                    );

                DiscordChannel team1Chat = await ctx.Guild.CreateTextChannelAsync(
                    $"{team1} Chat",
                    parent: parentCategory
                    );

                DiscordChannel team2Chat = await ctx.Guild.CreateTextChannelAsync(
                    $"{team2} Chat",
                    parent: parentCategory
                    );

                await team1Chat.AddOverwriteAsync(team2Role, deny: Permissions.AccessChannels);
                await team2Chat.AddOverwriteAsync(team1Role, deny: Permissions.AccessChannels);
                await team1Chat.AddOverwriteAsync(team1Role, allow: Permissions.AccessChannels);
                await team2Chat.AddOverwriteAsync(team2Role, allow: Permissions.AccessChannels);

                while (true)
                {
                    await Task.Delay(10000);
                    if (team1Lobby.Users.Count() <= 0 && team2Lobby.Users.Count() <= 0 && commonChannel.Users.Count() <= 0)
                    {
                        await commonChannel.DeleteAsync();
                        await team1Lobby.DeleteAsync();
                        await team2Lobby.DeleteAsync();

                        await textChannel.DeleteAsync();
                        await team1Chat.DeleteAsync();
                        await team2Chat.DeleteAsync();

                        await team1Role.DeleteAsync();
                        await team2Role.DeleteAsync();

                        await parentCategory.DeleteAsync();
                        return;
                    }
                }
            }
            catch (NBException ex)
            {
                await ex.SendException(ctx);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SplitTeams(CommandContext ctx, DiscordEmoji team1Emoji, ReadOnlyCollection<Reaction> reactions, List<DiscordUser> team1Users, List<DiscordUser> team2Users)
        {
            foreach (Reaction reaction in reactions)
                if (reaction.Emoji == team1Emoji && !reaction.Users.Contains(ctx.Client.CurrentUser))
                {
                    Console.WriteLine($"{reaction.Users.First().IsBot} {reaction.Users.First().Username}");
                    team1Users.Add(reaction.Users.First());
                }
                else if (!reaction.Users.Contains(ctx.Client.CurrentUser))
                {
                    Console.WriteLine($"{reaction.Users.First().IsBot} {reaction.Users.First().Username}");
                    team2Users.Add(reaction.Users.First());
                }
        }
    }
}
