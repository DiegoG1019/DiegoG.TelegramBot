using DiegoG.TelegramBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DiegoG.TelegramBot
{
    /// <summary>
    /// Represents the Default Response to an unknown command, or a normal message (when another command is not being held). This class cannot be instantiated. This class should be inherited.
    /// </summary>
    public abstract class Default : IBotCommand
    {
        public string HelpExplanation => "The Default Response to an unknown command";

        public string HelpUsage => string.Empty;

        public IEnumerable<OptionDescription>? HelpOptions => null;

        public virtual string Trigger => BotCommandProcessor.DefaultName;

        public string? Alias => null;

        public BotCommandProcessor Processor { get; set; }

        public abstract Task<CommandResponse> Action(BotCommandArguments args);

        public abstract Task<CommandResponse> ActionReply(BotCommandArguments args);

        public abstract void Cancel(User user);
    }

    internal class Default_ : Default
    {
        public override Task<CommandResponse> Action(BotCommandArguments args) => Task.FromResult(new CommandResponse(args, false, "Unknown Command"));

        public override Task<CommandResponse> ActionReply(BotCommandArguments args)
        {
            throw new NotImplementedException();
        }

        public override void Cancel(User user)
        {
            throw new NotImplementedException();
        }
    }
}
