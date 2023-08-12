using System.Collections.Generic;
using System.Threading.Tasks;
using DiegoG.TelegramBot.Types;
using Telegram.Bot.Types;

namespace DiegoG.TelegramBot;

public class Start : IBotCommand
{
    public string HelpExplanation => "Starts the bot";

    public string HelpUsage => "/start";

    public IEnumerable<OptionDescription>? HelpOptions => null;

    public string Trigger => "/start";

    public string? Alias => null;

    public TelegramBotCommandClient Processor { get; set; }

    public virtual Task<CommandResponse> Action(BotCommandArguments args) => Task.FromResult(new CommandResponse(args, false, "Hello! Welcome! Please type /help"));

    public virtual Task<CommandResponse> ActionReply(BotCommandArguments args) => Task.FromResult(new CommandResponse(args, false, ""));

    public virtual void Cancel(User user)
    {
        return;
    }
}
