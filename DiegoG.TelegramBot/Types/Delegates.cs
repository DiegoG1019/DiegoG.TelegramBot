using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DiegoG.TelegramBot.Types
{
    public delegate Task SendTextMessageAsync(
            ChatId chatId,
            string text,
            Telegram.Bot.Types.Enums.ParseMode parseMode = Telegram.Bot.Types.Enums.ParseMode.Default,
            bool disableWebPreview = false,
            bool disableNotification = false,
            int replyToMessageId = 0,
            Telegram.Bot.Types.ReplyMarkups.IReplyMarkup? replyMarkup = null); 

    public delegate void SendTextMessage(
             ChatId chatId,
             string text,
             Telegram.Bot.Types.Enums.ParseMode parseMode = Telegram.Bot.Types.Enums.ParseMode.Default,
             bool disableWebPreview = false,
             bool disableNotification = false,
             int replyToMessageId = 0,
             Telegram.Bot.Types.ReplyMarkups.IReplyMarkup? replyMarkup = null);
}
