using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DiegoG.TelegramBot
{
    public class CommandProcessorUpdateHandler : IUpdateHandler
    {
        public UpdateType[]? AllowedUpdates { get; init; }

        public virtual Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            => botClient is not BotCommandProcessor bot
                ? throw new InvalidOperationException($"Cannot handle the updates of bots clients that are not of type {typeof(BotCommandProcessor)}")
                : update.Type switch
                {
                    UpdateType.Unknown => Task.CompletedTask,
                    UpdateType.Message => bot.MessageHandler(update.Message),
                    UpdateType.InlineQuery => Task.CompletedTask,
                    UpdateType.ChosenInlineResult => Task.CompletedTask,
                    UpdateType.CallbackQuery => Task.CompletedTask,
                    UpdateType.EditedMessage => Task.CompletedTask,
                    UpdateType.ChannelPost => Task.CompletedTask,
                    UpdateType.EditedChannelPost => Task.CompletedTask,
                    UpdateType.ShippingQuery => Task.CompletedTask,
                    UpdateType.PreCheckoutQuery => Task.CompletedTask,
                    UpdateType.Poll => Task.CompletedTask,
                    UpdateType.PollAnswer => Task.CompletedTask,
                    UpdateType.MyChatMember => Task.CompletedTask,
                    UpdateType.ChatMember => Task.CompletedTask,
                    _ => throw new InvalidOperationException($"Invalid Update {update.Type}")
                };

        public virtual Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Log.Fatal($"Exception thrown: {exception}");
            return Task.CompletedTask;
        }
    }
}
