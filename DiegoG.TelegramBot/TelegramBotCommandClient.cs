using DiegoG.TelegramBot.Types;
using DiegoG.Utilities.Collections;
using DiegoG.Utilities.Reflection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;
using static DiegoG.TelegramBot.MessageQueue;
using System.Threading;
using Telegram.Bot.Exceptions;
using System.Net.Http;
using Telegram.Bot.Types.Payments;

namespace DiegoG.TelegramBot
{
    public record Config(bool ProcessNormalMessages = true, bool AddBotMeCommandInfo = true, bool AcceptMultiWordTriggers = false) { }
    public partial class TelegramBotCommandClient : TelegramBotClient, ITelegramBotClient
    {
        const string q = "\"";
        public const string DefaultName = "___default";


        public static string[] SeparateArgs(string input) => Regex.Split(input, $@"{q}([^{q}]*){q}|(\S+)").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        private readonly BotKey BotKey;
        private readonly Config Cfg;
        private readonly Func<Message, bool> MessageFilter;
        private readonly Dictionary<User, IBotCommand> HeldCommands = new();

        public event EventHandler<BotCommandArguments>? CommandCalled;
        public MessageQueue MessageQueue { get; private set; }
        public BotCommandList CommandList { get; private set; }
        public string BotHandle { get; private set; }

        /// <summary>
        /// Initializes the BotCommandProcessor
        /// </summary>
        /// <param name="apiSaturation">The maximum number of request the MessageQueue can send per minute</param>
        /// <param name="bots">A bot to subscribe onto, if you decide to leave blank, please make sure to manually subscribe <see cref="MessageHandler(object?, Telegram.Bot.Args.MessageEventArgs)"/> to your bots' OnMessage event </param>
        /// <param name="config"></param>
        public TelegramBotCommandClient(string token, int apiSaturation, BotKey key = BotKey.Any, Config? config = null, Func<Message, bool>? messageFilter = null, CommandProcessorUpdateHandler? updateHandler = null, HttpClient? client = null, string? baseUrl = null) : base(token, client, baseUrl)
        {
            Cfg = config ?? new();
            MessageFilter = messageFilter ?? (m => true);

            BotKey = key;

            CommandList = new(Cfg);

            foreach (var c in TypeLoader.InstanceTypesWithAttribute<IBotCommand>(typeof(BotCommandAttribute),
                ValidateCommandAttribute,
                AppDomain.CurrentDomain.GetAssemblies()))
            {
                c.Processor = this;
                CommandList.Add(c);
            }

            if (!CommandList.HasCommand("/help"))
                CommandList.Add(new Help() { Processor = this });
            if (!CommandList.HasCommand("/start"))
                CommandList.Add(new Start() { Processor = this });
            if (!CommandList.HasCommand(DefaultName))
                CommandList.Add(new Default_() { Processor = this });

            MessageQueue = new(this, apiSaturation);

            this.StartReceiving(updateHandler ?? new CommandProcessorUpdateHandler(), default);

            if (Cfg.AddBotMeCommandInfo)
                MessageQueue.EnqueueAction(async b => await b.SetMyCommandsAsync(CommandList.AvailableCommands));

            BotHandle = GetMeAsync().Result.Username;
        }

        public void EnqueueBotAction(BotAction action) => MessageQueue.EnqueueAction(action);
        public Task<TResult> EnqueueBotFunc<TResult>(BotFunc<TResult> func) => MessageQueue.EnqueueFunc(func);

        private bool ValidateCommandAttribute(Type type, Attribute[] attributes)
        {
            if (BotKey is BotKey.Any)
                return true;

            var b = ((BotCommandAttribute)attributes.First(y => y is BotCommandAttribute)).BotKey;
            return (BotKey & b) is not BotKey.Any; //x & x will always return x, and x & y will return whichever values are shared among them. So if we already know it's not 0 (Any), then if it returns anything other than that, we know it contains at least one of the desired flags
            //Also, if BotKey is Any, that means that this command is only meant only for certain bots, and not one marked as Any
        }

        /// <summary>
        /// Searches through the provided assemblies, or the executing assembly, in search of new commands to instantiate. Automatically excludes already registered command types, but does not extend exclusion for duplicated names
        /// </summary>
        /// <param name="assemblies">The assemblies to search. Leave null for none</param>
        /// <exception cref="InvalidBotCommandException">Thrown if one of the commands contains an invalid name</exception>
        /// <exception cref="InvalidOperationException">Thrown if one of the commands is found to be a duplicate</exception>
        public void LoadNewCommands(params Assembly[] assemblies)
        {
            foreach (var c in TypeLoader.InstanceTypesWithAttribute<IBotCommand>(typeof(BotCommandAttribute), ValidateCommandAttribute, CommandList.Select(d => d.GetType()), assemblies))
            {
                c.Processor = this;
                CommandList.Add(c);
            }
        }

        /// <summary>
        /// Currently this does nothing
        /// </summary>
        /// <returns></returns>
        protected virtual Task ChatMemberHandler(ChatMemberUpdated chatMember)
            => Task.CompletedTask;

        /// <summary>
        /// Currently this does nothing
        /// </summary>
        /// <returns></returns>
        protected virtual Task MyChatMemberHandler(ChatMemberUpdated chatMember)
            => Task.CompletedTask;

        /// <summary>
        /// Currently this does nothing
        /// </summary>
        /// <returns></returns>
        protected virtual Task PollAnswerHandler(PollAnswer pollAnswer)
            => Task.CompletedTask;

        /// <summary>
        /// Currently this does nothing
        /// </summary>
        /// <returns></returns>
        protected virtual Task PollHandler(Poll poll)
            => Task.CompletedTask;

        /// <summary>
        /// Currently this does nothing
        /// </summary>
        /// <returns></returns>
        protected virtual Task PreCheckoutHandler(PreCheckoutQuery query)
            => Task.CompletedTask;

        /// <summary>
        /// Currently this does nothing
        /// </summary>
        /// <returns></returns> 
        protected virtual Task ShippingQueryHandler(ShippingQuery query)
            => Task.CompletedTask;

        /// <summary>
        /// Currently this does nothing
        /// </summary>
        /// <returns></returns>
        protected virtual Task EditedChannelPostHandler(Message newPost)
            => Task.CompletedTask;

        /// <summary>
        /// Currently this does nothing
        /// </summary>
        /// <returns></returns>
        protected virtual Task ChannelPostHandler(Message post)
            => Task.CompletedTask;

        /// <summary>
        /// Currently this does nothing
        /// </summary>
        /// <returns></returns>
        protected virtual Task EditedMessageHandler(Message newMessage)
            => Task.CompletedTask;

        /// <summary>
        /// Currently this does nothing
        /// </summary>
        /// <returns></returns>
        protected virtual Task ChosenInlineResultHandler(ChosenInlineResult choice)
            => Task.CompletedTask;

        /// <summary>
        /// Currently this does nothing
        /// </summary>
        /// <returns></returns>
        protected virtual Task InlineQueryHandler(InlineQuery query)
            => Task.CompletedTask;

        /// <summary>
        /// Currently this does nothing
        /// </summary>
        /// <returns></returns>
        protected virtual Task UnknownUpdateHandler()
            => Task.CompletedTask;

        /// <summary>
        /// Currently this pipes CallbackQueries into commands using the CallbackData in them
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual Task CallbackQueryHandler(CallbackQuery query)
            => Task.Run(() =>
            {
                if (query.GetTriggerFromSignature(out var trigger, out var dat) && CommandList.HasCommand(trigger))
                {
                    Log.Verbose($"Piping CallbackQuery to {trigger} with \"{dat}\"");
                    query.Data = dat;
                    CommandList[trigger].AnswerCallbackQuery(query.From, query.Message?.Chat, query);
                    return;
                }
                Log.Verbose($"CallbackQuery has no trigger bound to it, ignoring");
            });

        /// <summary>
        /// Currently this is responsible for piping all user messages into the command execution engine
        /// </summary>
        /// <returns></returns>
        protected virtual async Task MessageHandler(Message msg)
        {
            var user = msg.From;
            Log.Debug($"Message from user {user}, processing");
            if (!MessageFilter(msg))
            {
                Log.Debug($"Message from user {user} filtered out");
                return;
            }

            var command = msg.Text;
            try
            {
                if (Cfg.ProcessNormalMessages || command.StartsWith("/"))
                {
                    command = command.Replace(BotHandle, "");
                    var cr = await Call(command, user, msg);

                    foreach (var act in cr)
                        MessageQueue.EnqueueAction(act);

                    Log.Debug($"Command {command} from user {user} succesfully processed.");
                }
            }
            catch (InvalidBotCommandException exc)
            {
                if (command.StartsWith("/"))
                {
                    MessageQueue.EnqueueAction(b => b.SendTextMessageAsync(msg.Chat.Id, $"Invalid Command: {exc.Message}", ParseMode.Default, null, false, false, msg.MessageId));
                    Log.Debug($"Invalid Command {command} from user {user}");
                }
            }
            catch (InvalidBotCommandArgumentsException exc)
            {
                if (command.StartsWith("/"))
                {
                    MessageQueue.EnqueueAction(b => b.SendTextMessageAsync(msg.Chat.Id, $"Invalid Command Argument: {exc.Message}", ParseMode.Default, null, false, false, msg.MessageId));
                    Log.Debug($"Invalid Command Arguments {command} from user {user}");
                }
            }
            catch (Exception exc)
            {
                Log.Fatal(exc, "Unhalded Exception thrown:");
            }
        }

        private async Task<IEnumerable<BotAction>> ReplyCall(BotCommandArguments args)
        {
            try
            {
                var (result, hold) = await HeldCommands[args.User].ActionReply(args);
                if (!hold)
                    HeldCommands.Remove(args.User);
                return result;
            }
            catch (InvalidBotCommandException e) { Log.Error(e, args.ToString()); throw; }
            catch (InvalidBotCommandArgumentsException e) { Log.Error(e, args.ToString()); throw; }
            catch (BotCommandProcessException e) { Log.Error(e, args.ToString()); throw; }
            catch (Exception e)
            {
                var ex = new InvalidBotCommandException(args.ToString(), "threw an unspecified exception", e);
                Log.Fatal(e, args.ToString());
                HeldCommands.Remove(args.User);
                throw ex;
            }
        }

        public Task<IEnumerable<BotAction>> Call(string input, User sender, Message message) 
            => Call(new(input, sender, message));

        public async Task<IEnumerable<BotAction>> Call(BotCommandArguments args)
        {
            try
            {
                if (HeldCommands.ContainsKey(args.User))
                    return await ReplyCall(args);

                IBotCommand cmd;
                {
                    var cst = Cfg.AcceptMultiWordTriggers ? args.ArgString : args.Arguments[0];
                    cmd = CommandList.HasCommand(cst, out var c) ? c : CommandList[DefaultName];
                }

                var t = cmd.Action(args);
                CommandCalled?.Invoke(this, args);

                var (result, hold) = await t;

                if (hold)
                    HeldCommands.Add(args.User, cmd);

                return result;
            }
            catch (InvalidBotCommandException e) { Log.Error(e, args.ToString()); throw; }
            catch (InvalidBotCommandArgumentsException e) { Log.Error(e, args.ToString()); throw; }
            catch (BotCommandProcessException e) { Log.Error(e, args.ToString()); throw; }
            catch (Exception e)
            {
                var ex = new InvalidBotCommandException(args.ToString(), "threw an unspecified exception", e);
                Log.Fatal(e, args.ToString());
                throw ex;
            }
        }
    }
}
