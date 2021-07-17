using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static DiegoG.TelegramBot.MessageQueue;

namespace DiegoG.TelegramBot.Types
{
    public interface IBotCommand
    {
        /// <summary>
        /// The Bot Command Processor this instance of the command is tied to
        /// </summary>
        BotCommandProcessor Processor { get; set; }

        /// <summary>
        /// The action to be taken when the command is invoked. Please be aware that the engine will try to remove the slash
        /// </summary>
        /// <returns>The return value of the command. If the method cannot be made async, consider returning Task.FromResult(YourResult)</returns>
        Task<CommandResponse> Action(BotCommandArguments args);

        /// <summary>
        /// While Hold is set to not null, the command processor will call ActionReply whenever another message is sent by the users
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<CommandResponse> ActionReply(BotCommandArguments args);

        /// <summary>
        /// Cancels the currently ongoing ActionReply executed by the command
        /// </summary>
        void Cancel(User user);

        /// <summary>
        /// Explains the purpose and effects of the command
        /// </summary>
        string HelpExplanation { get; }
        /// <summary>
        /// Explains the usage and syntax of the command - CommandName [Argument] (OptionalArgument)
        /// </summary>
        string HelpUsage { get; }
        /// <summary>
        /// Provides detailed information of each option setting. Set to null to ignore
        /// </summary>
        IEnumerable<OptionDescription>? HelpOptions { get; }
        /// <summary>
        /// Defines the trigger of the command (Case Insensitive)
        /// </summary>
        string Trigger { get; }
        /// <summary>
        /// An alternate, usually shortened way to call the command. Set to null to ignore, can not be duplicate with any of the aliases or triggers
        /// </summary>
        string? Alias { get; }
        /// <summary>
        /// Used to validate the command upon load
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Validate([NotNullWhen(false)]out string? message)
        {
            message = null;
            return true;
        }
    }

    public record OptionDescription(string OptionName, string Explanation) { }

    public record CommandResponse
    {
        public IEnumerable<BotAction> Actions { get; init; }
        public bool Hold { get; init; }

        public void Deconstruct(out IEnumerable<BotAction> actions, out bool hold)
        {
            hold = Hold;
            actions = Actions;
        }

        public CommandResponse(bool hold = false, params BotAction[] actions)
        {
            Hold = hold;
            Actions = actions;
        }

        public CommandResponse(Message msg, bool hold = false, params string[] messages)
        {
            Hold = hold;
            var act = new BotAction[messages.Length];
            for (int i = 0; i < messages.Length; i++)
            {
                var m = messages[i];
                act[i] = b => b.SendTextMessageAsync(msg.Chat.Id, m, replyToMessageId: msg.MessageId);
            }
            Actions = act;
        }

        public CommandResponse(BotCommandArguments args, bool hold = false, params string[] messages) : this(args.Message, hold, messages) { }
    }
}
