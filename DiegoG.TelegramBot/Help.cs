using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DiegoG.Utilities.Collections;
using System.Linq;
using DiegoG.Utilities;
using DiegoG.TelegramBot.Types;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace DiegoG.TelegramBot
{
    public class Help : IBotCommand
    {
        public string Trigger => "/help";
        public string Alias => "/h";
        public string HelpExplanation => "Returns a string explaining the uses of a specific command.";
        public string HelpUsage => "[Command]";
        public IEnumerable<OptionDescription>? HelpOptions => null;
        public BotCommandProcessor Processor { get; set; }


        private static string GetAlias(IBotCommand cmd) => cmd.Alias is not null ? $" ({cmd.Alias})" : "";

        private const string HelpExplanationFormat = "\n\tAvailable Options:\n\t\t";
        private static string GetHelpExplanation(IBotCommand cmd)
        {
            if(cmd.HelpOptions is not null)
            {
                int padding = cmd.HelpOptions.Max(s => s.OptionName.Length);
                return HelpExplanationFormat + cmd.HelpOptions.Select(s => $"{s.OptionName.PadLeft(padding)}: {s.Explanation}").Flatten("\n\t\t");
            }
            return "";
        }

        //0 : trigger | 1 : alias | 2 : HelpExplanation | 3 : HelpUsage | 4 : HelpOptions (if available)
        private const string HelpFormat = " > {0}{1} - {2}\n >> {3}{4}";

        public virtual async Task<CommandResponse> Action(BotCommandArguments args) 
            => new(args, false, args.ArgString.Length is <= 1 ? await GetGlobalHelp() : await GetCommandHelp(args));

        public Task<string> GetCommandHelp(BotCommandArguments args) => Task.Run(() => 
        {
            var clist = Processor.CommandList;
            var cmd = args.Arguments[1];

            IBotCommand c;
            if (clist.HasCommand(cmd))
                c = clist[cmd];
            else if (clist.HasCommand("/" + cmd))
                c = clist["/" + cmd];
            else
                throw new InvalidBotCommandArgumentsException(args.ToString(), "Unknown Command");
            return string.Format(HelpFormat, c.Trigger, GetAlias(c), c.HelpExplanation, c.HelpUsage, GetHelpExplanation(c));
        });

        public Task<string> GetGlobalHelp() => Task.Run(() => Task.FromResult(string.Join("\n", Processor.CommandList.Where(s => s.Trigger is not BotCommandProcessor.DefaultName).Select(command => $"{command.Trigger}{GetAlias(command)} - {command.HelpUsage}"))));

        public virtual Task<CommandResponse> ActionReply(BotCommandArguments args) => Task.FromResult(new CommandResponse(false));

        public virtual void Cancel(User user) { return; }
    }
}