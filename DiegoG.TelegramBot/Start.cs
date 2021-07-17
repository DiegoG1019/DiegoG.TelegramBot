using DiegoG.TelegramBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DiegoG.TelegramBot
{
    public class Start : IBotCommand
    {
        public string HelpExplanation => "Starts the bot";

        public string HelpUsage => "/start";

        public IEnumerable<OptionDescription>? HelpOptions => null;

        public string Trigger => "/start";

        public string? Alias => null;

        public BotCommandProcessor Processor { get; set; }

        public virtual Task<CommandResponse> Action(BotCommandArguments args) => Task.FromResult(new CommandResponse(args, false, "Hello! Welcome! Please type /help"));

        public virtual Task<CommandResponse> ActionReply(BotCommandArguments args) => Task.FromResult(new CommandResponse(args, false, ""));

        public virtual void Cancel(User user)
        {
            return;
        }
    }
}
