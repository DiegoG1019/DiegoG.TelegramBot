using Telegram.Bot.Types;

namespace DiegoG.TelegramBot.Types;

public sealed record BotCommandArguments
{
    public Message Message { get; init; }
    public string ArgString { get; init; }
    public ChatId FromChat { get; init; }
    public User? User { get; init; }
    public string[] Arguments { get; init; }

    public BotCommandArguments(string argString, User? user, Message message)
    {
        ArgString = argString;
        User = user;
        Arguments = TelegramBotCommandClient.SeparateArgs(argString);
        Message = message;
        FromChat = message.Chat.Id;
    }

    public override string ToString()
        => $"Command {ArgString} sent by User {User}";
}
