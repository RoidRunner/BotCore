﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace BotCoreNET.CommandHandling
{
    /// <summary>
    /// Collection of methods useful for parsing string command arguments to discord objects
    /// </summary>
    public static class ArgumentParsing
    {
        public const string GENERIC_PARSED_USER = "A Discord User, specified either by a mention, the Discord Snowflake Id, the keyword \"self\" or Username#Discriminator";
        public const string GENERIC_PARSED_ROLE = "A Guild Role, specified either by a mention, the Discord Snowflake Id or role name";
        public const string GENERIC_PARSED_CHANNEL = "A Guild Channel, specified either by a mention, the Discord Snowflake Id, the keyword \"this\" or channel name";

        /// <summary>
        /// Parses a user given a commandcontext. Because it works without guild context it needs to be asynchronous
        /// </summary>
        /// <param name="context">The commandcontext to parse the user from</param>
        /// <param name="argument">The argument string to parse the user from</param>
        /// <param name="allowMention">Wether mentioning is enabled for parsing user</param>
        /// <param name="allowSelf">Wether pointing to self is allowed</param>
        /// <param name="allowId">Wether the ulong id is enabled for parsing user</param>
        /// <returns>The parsed user if parsing succeeded, null instead</returns>
        public static async Task<SocketUser> ParseUser(IDMCommandContext context, string argument, bool allowMention = true, bool allowSelf = true, bool allowId = true)
        {
            SocketUser result = null;
            if (allowSelf && argument.Equals("self"))
            {
                result = context.User;
            }
            else if (allowMention && argument.StartsWith("<@") && argument.EndsWith('>') && argument.Length > 3)
            {
                if (ulong.TryParse(argument.Substring(2, argument.Length - 3), out ulong userId))
                {
                    result = await context.Channel.GetUserAsync(userId) as SocketUser;
                }
            }
            else if (allowMention && argument.StartsWith("<@!") && argument.EndsWith('>') && argument.Length > 3)
            {
                if (ulong.TryParse(argument.Substring(3, argument.Length - 3), out ulong userId))
                {
                    result = await context.Channel.GetUserAsync(userId) as SocketUser;
                }
            }
            else if (allowId && ulong.TryParse(argument, out ulong userId))
            {
                result = await context.Channel.GetUserAsync(userId) as SocketUser;
            }

            return result;
        }

        /// <summary>
        /// Attempts to parse a guild user
        /// </summary>
        /// <param name="context">The guild commandcontext to parse the user from</param>
        /// <param name="argument">The argument string to parse the user from</param>
        /// <param name="result">The resulting socketguild user</param>
        /// <param name="allowMention">Wether mentioning is enabled for parsing user</param>
        /// <param name="allowSelf">Wether pointing to self is allowed</param>
        /// <param name="allowId">Wether the ulong id is enabled for parsing user</param>
        /// <returns>True if parsing was successful</returns>
        public static bool TryParseGuildUser(IGuildCommandContext context, string argument, out SocketGuildUser result, bool allowMention = true, bool allowSelf = true, bool allowId = true, bool allowName = true)
        {
            result = null;
            if (allowSelf && argument.Equals("self"))
            {
                result = context.GuildUser;
                return true;
            }
            else if (allowMention && argument.StartsWith("<@") && argument.EndsWith('>') && argument.Length > 3)
            {
                if (argument.StartsWith("<@!") && argument.Length > 4)
                {
                    if (ulong.TryParse(argument.Substring(3, argument.Length - 4), out ulong userId2))
                    {
                        result = context.Guild.GetUser(userId2);
                        return result != null;
                    }
                }
                if (ulong.TryParse(argument.Substring(2, argument.Length - 3), out ulong userId))
                {
                    result = context.Guild.GetUser(userId);
                    return result != null;
                }
            }
            else if (allowMention && argument.StartsWith("<@!") && argument.EndsWith('>') && argument.Length > 3)
            {
                if (ulong.TryParse(argument.Substring(3, argument.Length - 3), out ulong userId))
                {
                    result = context.Guild.GetUser(userId);
                    return result != null;
                }
            }
            else if (allowId && ulong.TryParse(argument, out ulong userId))
            {
                result = context.Guild.GetUser(userId);
                return result != null;
            }
            else if (allowName)
            {
                foreach (SocketGuildUser user in context.Guild.Users)
                {
                    if (user.ToString() == argument)
                    {
                        result = user;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to parse a guild role
        /// </summary>
        /// <param name="context">The guild commandcontext to parse the role with</param>
        /// <param name="argument">The argument string to parse the role from</param>
        /// <param name="result">The resulting socketguild user</param>
        /// <param name="allowMention">Wether mentioning is enabled for parsing role</param>
        /// <param name="allowId">Wether the ulong id is enabled for parsing role</param>
        /// <returns>True if parsing was successful</returns>
        public static bool TryParseRole(IGuildCommandContext context, string argument, out SocketRole result, bool allowMention = true, bool allowId = true, bool allowName = true)
        {
            result = null;

            if (allowMention && argument.StartsWith("<@&") && argument.EndsWith('>') && argument.Length > 3)
            {
                if (ulong.TryParse(argument.Substring(3, argument.Length - 4), out ulong roleId))
                {
                    result = context.Guild.GetRole(roleId);
                    return result != null;
                }
            }
            else if (allowId && ulong.TryParse(argument, out ulong roleId))
            {
                result = context.Guild.GetRole(roleId);
                return result != null;
            }
            else if (allowName)
            {
                foreach (SocketRole role in context.Guild.Roles)
                {
                    if (role.Name == argument)
                    {
                        result = role;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to parse a guild channel
        /// </summary>
        /// <param name="context">The guild commandcontext to parse the channel with</param>
        /// <param name="argument">The argument string to parse the channel from</param>
        /// <param name="result">The socketguildchannel result</param>
        /// <param name="allowMention">Wether mentioning is enabled for parsing role</param>
        /// <param name="allowThis">Wether pointing to current channel is enabled</param>
        /// <param name="allowId">Wether the ulong id is enabled for parsing role</param>
        /// <returns>True, if parsing was successful</returns>
        public static bool TryParseGuildChannel(IGuildCommandContext context, string argument, out SocketGuildChannel result, bool allowMention = true, bool allowThis = true, bool allowId = true, bool allowName = true)
        {
            result = null;
            if (allowId && ulong.TryParse(argument, out ulong Id))
            {
                result = context.Guild.GetChannel(Id);
                return result != null;
            }
            if (allowMention && argument.StartsWith("<#") && argument.EndsWith('>') && argument.Length > 3)
            {
                if (ulong.TryParse(argument.Substring(2, argument.Length - 3), out ulong Id2))
                {
                    result = context.Guild.GetTextChannel(Id2);
                    return result != null;
                }
            }
            if (allowThis && argument.Equals("this"))
            {
                result = context.GuildChannel;
                return true;
            }
            if (allowName)
            {
                foreach (SocketGuildChannel channel in context.Guild.Channels)
                {
                    if (channel.Name == argument)
                    {
                        result = channel;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Attempts to parse a guild text channel without guild command context
        /// </summary>
        /// <param name="context">The commandcontext to parse the channel with</param>
        /// <param name="argument">The argument string to parse the channel from</param>
        /// <param name="result">The sockettextchannel result</param>
        /// <param name="allowMention">Wether mentioning is enabled for parsing role</param>
        /// <param name="allowThis">Wether pointing to current channel is enabled</param>
        /// <param name="allowId">Wether the ulong id is enabled for parsing role</param>
        /// <returns>True, if parsing was successful</returns>
        public static bool TryParseGuildTextChannel(IDMCommandContext context, string argument, out SocketTextChannel result, bool allowMention = true, bool allowThis = true, bool allowId = true)
        {
            result = null;
            if (allowId && ulong.TryParse(argument, out ulong Id))
            {
                result = BotCore.Client.GetChannel(Id) as SocketTextChannel;
                return result != null;
            }
            if (allowMention && argument.StartsWith("<#") && argument.EndsWith('>') && argument.Length > 3)
            {
                if (ulong.TryParse(argument.Substring(2, argument.Length - 3), out ulong Id2))
                {
                    result = BotCore.Client.GetChannel(Id2) as SocketTextChannel;
                    return result != null;
                }
            }
            if (allowThis && argument.Equals("this") && GuildCommandContext.TryConvert(context, out IGuildCommandContext guildContext))
            {
                result = guildContext.GuildChannel;
                return result != null;
            }
            return false;
        }

        public static bool TryParseGuild(IDMCommandContext context, string argument, out SocketGuild guild, bool allowthis = true, bool allowId = true)
        {
            if (allowthis && argument.ToLower() == "this" && GuildCommandContext.TryConvert(context, out IGuildCommandContext guildContext))
            {
                guild = guildContext.Guild;
                return guild != null;
            }
            else if (allowId && ulong.TryParse(argument, out ulong guildId))
            {
                guild = BotCore.Client.GetGuild(guildId);
                return guild != null;
            }
            guild = null;
            return false;
        }
    }
}