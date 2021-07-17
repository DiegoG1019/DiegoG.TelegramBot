using DiegoG.TelegramBot.Types.ChatSequenceTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DiegoG.TelegramBot.Types
{
    public abstract class ChatSequence
    {
        public abstract Task<string> EnterFirst();

        public abstract Task<Response> Respond(BotCommandArguments args);

        public abstract Task<Advancement> Advance();

        public abstract Task<Advancement> SetStep(string step);

        internal ChatSequence() { }
    }

    public class ChatSequence<TContext> : ChatSequence where TContext : IChatSequenceContext, new()
    {
        private readonly Dictionary<string, ChatSequenceStep<TContext>> Steps = new();

        public TContext Context { get; init; }
        private ChatSequenceStep<TContext> FirstStep_;
        public ChatSequenceStep<TContext> FirstStep
        {
            get => FirstStep_;
            init
            {
                FirstStep_ = value;
                CurrentStep = value;

                Steps.Add(FirstStep_.Name, FirstStep_);
                IEnumerable<ChatSequenceStep<TContext>>? steps = FirstStep.Children;
                while(steps is not null)
                {
                    IEnumerable<ChatSequenceStep<TContext>> steps_ = Array.Empty<ChatSequenceStep<TContext>>();
                    foreach(var s in steps)
                    {
                        if (!Steps.TryAdd(s.Name, s) && !ReferenceEquals(Steps[s.Name], s))
                            throw new InvalidOperationException("Cannot add two different steps under the same name. These names are identifiers only, and should be unique");
                        steps_ = steps_.Concat(s.Children ?? Array.Empty<ChatSequenceStep<TContext>>());
                    }
                    steps = steps_.FirstOrDefault() is not null ? steps_ : null;
                }
            }
        }
        public ChatSequenceStep<TContext> CurrentStep { get; private set; }

        public override Task<string> EnterFirst()
            => Task.Run(() => FirstStep.StepEntered(Context));

        public override Task<Response> Respond(BotCommandArguments args)
            => Task.Run(() => CurrentStep.Response(Context, args));

        public override Task<Advancement> Advance() => Task.Run<Advancement>(async () =>
        {
            if (CurrentStep?.Children is null)
                return new(null, Advancement.SuccessCode.EndOfSequence);

            foreach (var c in CurrentStep.Children)
                if (c.Condition(Context))
                {
                    CurrentStep = c;
                    return new(await CurrentStep.StepEntered(Context), Advancement.SuccessCode.Success);
                }
            return new(null, Advancement.SuccessCode.Failure);
        });

        public override Task<Advancement> SetStep(string step) => Task.Run<Advancement>(async () =>
        {
            if (Steps.TryGetValue(step, out var value))
            {
                CurrentStep = value;
                return new(await CurrentStep.StepEntered(Context), Advancement.SuccessCode.Success);
            }
            throw new ArgumentException($"No step named {step} found", nameof(step));
        });

        public ChatSequence(ChatSequenceStep<TContext> firstStep)
        {
            Context = new() { Sequence = this };
            FirstStep = firstStep;
        }
    }
}
