using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DiegoG.TelegramBot.Types
{
    public sealed class BotCommandList : IEnumerable<IBotCommand>
    {
        private readonly SortedDictionary<string, IBotCommand> dict = new(new StringLenComp());
        private readonly List<BotCommand> BotCommands = new();
        private readonly Config Cfg;

        public IEnumerable<BotCommand> AvailableCommands => BotCommands;

        public int Count { get; private set; }
        public IBotCommand this[string commandName]
            => HasCommand(commandName)
               ? dict[commandName] ?? throw new InvalidBotCommandException(commandName, "does not exist")
               : throw new InvalidBotCommandException(commandName, "does not exist");

        internal void Add(IBotCommand cmd)
        {
            if (!cmd.Validate(out var msg))
                throw new InvalidOperationException($"Unable to load command {cmd.Trigger}: {msg}");
            Count++;
            var trigger = cmd.Trigger.Contains("/") ? cmd.Trigger.ToLower() : cmd.Trigger;
            ThrowIfDuplicateOrInvalid(trigger);
            dict.Add(trigger, cmd);

            if(trigger is not TelegramBotCommandClient.DefaultName && trigger.StartsWith("/"))
                BotCommands.Add(new()
                {
                    Command = trigger,
                    Description = @$"{(cmd.Alias is not null ? $"({cmd.Alias})" : "")} {cmd.HelpUsage} - {cmd.HelpExplanation}"
                });
        }

        public bool HasCommand(string cmd)
        {
            foreach (var x in dict.Keys.Reverse())
                if (cmd.StartsWith(x))
                    return true;
            return false;
        }

        internal bool HasCommand(string cmd, [NotNullWhen(true)] out IBotCommand? command)
        {
            var x = cmd.StartsWith('/');
            if ((x && Cfg.CommandCaseSensitive) || (!x && Cfg.CaseSensitive))
                return HasCommand_CaseSensitive(cmd, out command);
            return HasCommand_CaseInsensitive(cmd.ToLower(), out command);
        }

        private bool HasCommand_CaseSensitive(string cmd, [NotNullWhen(true)] out IBotCommand? command)
        {
            foreach (var x in dict.Keys.Reverse())
                if (cmd.StartsWith(x))
                {
                    command = dict[x];
                    return true;
                }
            command = null;
            return false;
        }

        private bool HasCommand_CaseInsensitive(string cmd, [NotNullWhen(true)] out IBotCommand? command)
        {
            foreach (var x in dict.Keys.Reverse())
                if (cmd.StartsWith(x.ToLower()))
                {
                    command = dict[x];
                    return true;
                }
            command = null;
            return false;
        }

        public IEnumerator<IBotCommand> GetEnumerator()
        {
            foreach (var cmd in dict.Values)
                yield return cmd;
        }

        private void ThrowIfDuplicateOrInvalid(string cmd)
        {
            if (dict.ContainsKey(cmd))
                throw new InvalidOperationException($"Duplicate command detected: {cmd}. Commands, Command Aliases, or General Aliases must be unique from one another and themselves");
            if (!Cfg.AcceptMultiWordTriggers && cmd.Any(char.IsWhiteSpace))
                throw new InvalidBotCommandException(cmd, "Command triggers or aliases cannot contain whitespace");
        }

        internal BotCommandList(Config config) { Cfg = config; }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class StringLenComp : IComparer<string>
        {
            public int Compare(string? x, string? y)
            {
                var r = x?.Length.CompareTo(y?.Length);
                return r is not null and not 0 ? (int)r : x?.CompareTo(y) ?? 0;
            }
        }
    }

}
