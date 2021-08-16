using System;
using System.Collections;
using System.Collections.Generic;
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

        public bool HasCommand(string cmd) => dict.ContainsKey(cmd);

        public IEnumerator<IBotCommand> GetEnumerator()
        {
            foreach (var cmd in dict.Values)
                yield return cmd;
        }

        private void ThrowIfDuplicateOrInvalid(string cmd)
        {
            if (HasCommand(cmd))
                throw new InvalidOperationException($"Duplicate command detected: {cmd}. Commands, Command Aliases, or General Aliases must be unique from one another and themselves");
            if (cmd.Any(char.IsWhiteSpace))
                throw new InvalidBotCommandException(cmd, "Command triggers or aliases cannot contain whitespace");
        }

        internal BotCommandList() { }

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
