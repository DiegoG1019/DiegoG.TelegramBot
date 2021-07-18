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
    public partial class TelegramBotCommandClient
    {
        public class CommandProcessorUpdateHandler : IUpdateHandler
        {
            public UpdateType[]? AllowedUpdates { get; init; }

            public virtual Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
                => botClient is not TelegramBotCommandClient bot
                    ? throw new InvalidOperationException($"Cannot handle the updates of bots clients that are not of type {typeof(TelegramBotCommandClient)}")
                    : update.Type switch
                    {
                        UpdateType.Unknown => bot.UnknownUpdateHandler(),
                        UpdateType.Message => bot.MessageHandler(update.Message),
                        UpdateType.InlineQuery => bot.InlineQueryHandler(update.InlineQuery),
                        UpdateType.ChosenInlineResult => bot.ChosenInlineResultHandler(update.ChosenInlineResult),
                        UpdateType.CallbackQuery => bot.CallbackQueryHandler(update.CallbackQuery),
                        UpdateType.EditedMessage => bot.EditedMessageHandler(update.Message),
                        UpdateType.ChannelPost => bot.ChannelPostHandler(update.ChannelPost),
                        UpdateType.EditedChannelPost => bot.EditedChannelPostHandler(update.EditedChannelPost),
                        UpdateType.ShippingQuery => bot.ShippingQueryHandler(update.ShippingQuery),
                        UpdateType.PreCheckoutQuery => bot.PreCheckoutHandler(update.PreCheckoutQuery),
                        UpdateType.Poll => bot.PollHandler(update.Poll),
                        UpdateType.PollAnswer => bot.PollAnswerHandler(update.PollAnswer),
                        UpdateType.MyChatMember => bot.MyChatMemberHandler(update.MyChatMember),
                        UpdateType.ChatMember => bot.ChatMemberHandler(update.ChatMember),
                        _ => throw new InvalidOperationException($"Invalid Update {update.Type}")
                    };

            public virtual Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                Log.Fatal($"Exception thrown: {exception}");
                return Task.CompletedTask;
            }
        }
    }
}
