using Telegram.Bot.Types;

namespace DiegoG.TelegramBot.Types
{
    public interface IChatSequenceContext
    {
        ChatSequence Sequence { get; set; }
        User User { get; }
    }
}
