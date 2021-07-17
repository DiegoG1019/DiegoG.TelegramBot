using DiegoG.TelegramBot.Types;
using DiegoG.TelegramBot.Types.ChatSequenceTypes;
using DiegoG.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DiegoG.TelegramBot.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            var bot = new TelegramBotClient(CheapTacticGitIgnore.DGSandboxApiKey);
            var proc = new BotCommandProcessor(bot, 30);

            bot.StartReceiving();

            Log.Information($"Connected to {await bot.GetMeAsync()}");

            while(true)
                await Task.Delay(500);
        }
    }

    [BotCommand]
    class ChatTest : ChatBot<ChatTest.Context>
    {
        public override ChatSequenceStep<Context> FirstStep { get; } = new()
        {
            Name = "0",
            StepEntered = async c => "Step One! Great! Please Enter a Number!",
            Response = async (c, a) =>
            {
                try
                {
                    var r = await Task.Run(() => double.Parse(a.ArgString)).AwaitWithTimeout(500);
                    c.EnteredValue = r;
                    return new Response($"Great! You entered {r}", Response.ResponseAction.Advance);
                }
                catch
                {
                    return new Response("Please enter a decimal number, it can include decimal digits", Response.ResponseAction.Continue);
                }
            },
            Condition = c => true,
            Children = new ChatSequenceStep<Context>[]
            {
                new()
                {
                    Name = "1_1",
                    Condition = c => c.EnteredValue is <0,
                    StepEntered = async c => "You entered a negative value! Say the word and we'll delve into unknown lands...",
                    Response = async (c, a) => new("Unknown lands means testing for an expected exception scenario. Here you go.", Response.ResponseAction.Advance)
                },
                new()
                {
                    Name = "1_2",
                    Condition = c => c.EnteredValue is not null,
                    StepEntered = async c => "Great! Now tell me your name!",
                    Response = async (c, a) =>
                    {
                        if(a.ArgString.Contains(" "))
                            return new Response("Uh-Oh! No spaces allowed! You'll have to start over!", Response.ResponseAction.Advance);
                        c.Name = a.ArgString;
                        return new Response("Great! Thanks!", Response.ResponseAction.Advance);
                    },
                    Children = new ChatSequenceStep<Context>[]
                    {
                        new RepeatStep<Context>("Rep_0", c => c.Name is null, "0"),
                        new()
                        {
                            Name = "1_2_2",
                            Condition = c => c.Name is not null,
                            StepEntered = async c => "You're finally at the last step! Give the word, and we can end this, you and I! For Azeroth! For the Alliance!",
                            Response = async (c, a) =>
                            {
                                return new Response("Huzzah! Success!", Response.ResponseAction.End);
                            },
                        }
                    }
                }
            }
        };

        public class Context : IChatSequenceContext
        {
            public ChatSequence Sequence { get; set; }
            public User User { get; set; }
            public double? EnteredValue { get; set; }
            public string? Name { get; set; }
        }
    }

    [BotCommand]
    class AsyncGetTest : IBotCommand
    {
        public BotCommandProcessor Processor { get; set; }

        public string HelpExplanation => "Tests the Get queued requests";

        public string HelpUsage => Trigger;

        public IEnumerable<OptionDescription> HelpOptions => null;

        public string Trigger => "/testget";

        public string Alias => null;

        public Task<CommandResponse> Action(BotCommandArguments args)
        {
            Processor.MessageQueue.ApiSaturationLimit = 3;
            AsyncTaskManager tasks = new();
            Log.Verbose("Start");
            tasks.Add(Processor.MessageQueue.EnqueueFunc(async b => { Log.Verbose("A1"); await Task.Delay(600); Log.Verbose("A2"); return 0; }));
            tasks.Add(Processor.MessageQueue.EnqueueFunc(async b => { Log.Verbose("B1"); await Task.Delay(600); Log.Verbose("B2"); return 0; }));
            tasks.Add(Processor.MessageQueue.EnqueueFunc(async b => { Log.Verbose("C1"); await Task.Delay(600); Log.Verbose("C2"); return 0; }));
            tasks.Add(Processor.MessageQueue.EnqueueFunc(async b => { Log.Verbose("D1"); await Task.Delay(600); Log.Verbose("D2"); return 0; }));
            tasks.Add(Processor.MessageQueue.EnqueueFunc(async b => { Log.Verbose("E1"); await Task.Delay(600); Log.Verbose("E2"); return 0; }));
            Log.Verbose("End");
            return Task.FromResult(new CommandResponse(args, false, "Done, check the console"));
        }

        public Task<CommandResponse> ActionReply(BotCommandArguments args)
        {
            throw new NotImplementedException();
        }

        public void Cancel(User user)
        {
            throw new NotImplementedException();
        }
    }

    //[BotCommand]
    class DefaultTest : Default
    {
        public override Task<CommandResponse> Action(BotCommandArguments args)
        {
            return Task.FromResult(new CommandResponse(args, false, "a"));
        }

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
