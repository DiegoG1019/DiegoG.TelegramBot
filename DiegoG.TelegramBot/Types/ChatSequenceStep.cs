using DiegoG.TelegramBot.Types.ChatSequenceTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiegoG.TelegramBot.Types
{
    /// <summary>
    /// A Special class, whose purpose is to serve as a placeholder for another step. Use ONLY the constructor for this instance, do NOT use an object initializer or initialize any other properties
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public sealed class RepeatStep<TContext> : ChatSequenceStep<TContext> where TContext : IChatSequenceContext, new()
    {
        /// <summary>
        /// Instances a Placeholder value
        /// </summary>
        /// <param name="stepName">The name of the step. Unique, as all other step names</param>
        /// <param name="condition">The condition under which to fall into this step</param>
        /// <param name="toStep">The step to hop onto</param>
        public RepeatStep(string stepName, Func<TContext, bool> condition, string toStep) : base()
        {
            Name = stepName;
            Condition = condition;
            StepEntered = async c => 
            {
                var x = await c.Sequence.SetStep(toStep);
                return x.EnterValue!;
            };
        }
    }
    public class ChatSequenceStep<TContext> where TContext : IChatSequenceContext, new()
    {
        private ChatSequenceStep<TContext>? Parent;

        /// <summary>
        /// The name of the step
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// The condition to be met to enter into this step
        /// </summary>
        public Func<TContext, bool> Condition { get; init; } = t => throw new NotImplementedException("The Condition for this Step was not defined");

        /// <summary>
        /// The response issued as soon as this step is entered
        /// </summary>
        public Func<TContext, Task<string>> StepEntered { get; init; } = t => throw new NotImplementedException("The StepEntered for this Step was not defined");

        /// <summary>
        /// The response issued every time the user replies to the bot
        /// </summary>
        public Func<TContext, BotCommandArguments, Task<Response>> Response { get; init; } = (t, b) => throw new NotImplementedException("The Response for this Step was not defined");

        private IEnumerable<ChatSequenceStep<TContext>>? Children_ = Array.Empty<ChatSequenceStep<TContext>>();
        /// <summary>
        /// The Possible steps this step can advance into
        /// </summary>
        public IEnumerable<ChatSequenceStep<TContext>>? Children
        {
            get => Children_;
            init
            {
                Children_ = value;
                if (value is not null)
                    foreach (var step in value)
                        step.Parent = this;
            }
        }

        /// <summary>
        /// Instances an object of class ChatSequenceStep, ensure that all properties are initialized in an object initializer
        /// </summary>
        public ChatSequenceStep() { }

        /// <summary>
        /// Instances an object of class ChatSequenceStep
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="condition"><see cref="Condition"/></param>
        /// <param name="stepEnter"><see cref="StepEntered"/></param>
        /// <param name="response"><see cref="Response"/></param>
        /// <param name="children"><see cref="Children"/></param>
        public ChatSequenceStep(string name, Func<TContext, bool> condition, Func<TContext, Task<string>> stepEnter, Func<TContext, BotCommandArguments, Task<Response>> response, IOrderedEnumerable<ChatSequenceStep<TContext>> children)
        {
            Name = name;
            Condition = condition;
            StepEntered = stepEnter;
            Response = response;
            Children = children;
        }
    }
}
