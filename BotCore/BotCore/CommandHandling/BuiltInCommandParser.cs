﻿using BotCoreNET.BotVars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotCoreNET.CommandHandling
{
    class BuiltInCommandParser : ICommandParser
    {
        private string prefix = "/";

        public void OnBotVarSetup()
        {
            BotVarManager.SubscribeToBotVarUpdateEvent(OnBotVarUpdate, "prefix");
        }

        private void OnBotVarUpdate(BotVar var)
        {
            if (var.IsString && !string.IsNullOrEmpty(var.String))
            {
                prefix = var.String;
            }
        }

        public ICommandContext ParseCommand(IGuildMessageContext guildContext)
        {
            return ParseCommand(guildContext as IMessageContext);
        }

        public ICommandContext ParseCommand(IMessageContext dmContext)
        {
            string message = dmContext.Content.Substring(prefix.Length).Trim();
            string commandIdentifier;
            string argumentSection;
            IndexArray<string> arguments;
            Command interpretedCommand;
            CommandSearchResult commandSearch;

            int argStartPointer = message.IndexOf(':', prefix.Length);
            if (argStartPointer == -1 || argStartPointer == message.Length - 1)
            {
                argumentSection = string.Empty;
                arguments = new string[0];
                commandIdentifier = message;
            }
            else
            {
                argumentSection = message.Substring(argStartPointer + 1);
                commandIdentifier = message.Substring(0, argStartPointer);
                int argcnt = 1;
                for (int i = 0; i < argumentSection.Length; i++)
                {
                    bool isUnescapedComma = argumentSection[i] == ',';
                    if (i > 0 && isUnescapedComma)
                    {
                        isUnescapedComma = argumentSection[i - 1] != '\\';
                    }
                    if (isUnescapedComma)
                    {
                        argcnt++;
                    }
                }
                arguments = new string[argcnt];
                int argindex = 0;
                int lastindex = 0;
                for (int i = 0; i < argumentSection.Length; i++)
                {
                    bool isUnescapedComma = argumentSection[i] == ',';
                    if (i > 0 && isUnescapedComma)
                    {
                        isUnescapedComma = argumentSection[i - 1] != '\\';
                    }
                    if (isUnescapedComma)
                    {
                        if (lastindex < i)
                        {
                            arguments[argindex] = argumentSection.Substring(lastindex, i - lastindex).Trim().Replace("\\,", ",");
                        }
                        else
                        {
                            arguments[argindex] = string.Empty;
                        }
                        argindex++;
                        lastindex = i + 1;
                    }
                }
                if (lastindex <= argumentSection.Length - 1)
                {
                    arguments[argindex] = argumentSection.Substring(lastindex, argumentSection.Length - lastindex).Trim().Replace("\\,", ",");
                }
                else
                {
                    arguments[argindex] = string.Empty;
                }
            }

            commandSearch = CommandCollection.TryFindCommand(commandIdentifier, arguments.TotalCount, out interpretedCommand);

            return new CommandContext(interpretedCommand, commandSearch, argumentSection, arguments);
        }

        public bool IsPotentialCommand(string messageContent)
        {
            return messageContent.StartsWith(prefix) && messageContent.Length > prefix.Length;
        }

        public bool IsPotentialCommand(string messageContent, ulong guildId)
        {
            return messageContent.StartsWith(prefix) && messageContent.Length > prefix.Length;
        }

        public string RemoveArgumentsFront(int count, string argumentSection)
        {
            for (int i = 0; i < argumentSection.Length; i++)
            {
                bool isUnescapedComma = argumentSection[i] == ',';
                if (i > 0 && isUnescapedComma)
                {
                    isUnescapedComma = argumentSection[i - 1] != '\\';
                }
                if (isUnescapedComma)
                {
                    count--;
                    if (count == 0)
                    {
                        return argumentSection.Substring(i);
                    }
                }
            }
            return null;
        }

        public string CommandSyntax(string commandidentifier)
        {
            return CommandSyntax(commandidentifier, new Argument[0]);
        }

        public string CommandSyntax(string commandidentifier, Argument[] arguments)
        {
            if (arguments.Length == 0)
            {
                return $"{prefix}{commandidentifier}";
            }
            else
            {
                return $"{prefix}{commandidentifier}: {string.Join(", ", arguments, 0, arguments.Length)}";
            }
        }
    }
}
