using DiegoG.TelegramBot.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DiegoG.TelegramBot
{
    public static class CommandExtensions
    {
        private const string SignCbData_sep = @"||\";
        private static readonly int SignCbData_sep_count = SignCbData_sep.Length;

        /// <summary>
        /// Allows a CallbackQuery with this CallbackData to be piped properly to the respective BotCommand
        /// </summary>
        /// <param name="botCommand"></param>
        /// <param name="CallbackData"></param>
        /// <returns></returns>
        public static string SignCallbackData(this IBotCommand botCommand, string? CallbackData = null)
            => (CallbackData ?? string.Empty) + $"{SignCbData_sep}{botCommand.Trigger}";

        public static bool GetTriggerFromSignature(this CallbackQuery query, [NotNullWhen(true)]out string? trigger, [NotNullWhen(true)]out string? data)
        {
            trigger = data = null;
            var dat = query.Data;
            if (string.IsNullOrWhiteSpace(dat) || !dat.Contains(SignCbData_sep) || dat.EndsWith(SignCbData_sep))
                return false;

            var lind = dat.LastIndexOf(SignCbData_sep);
            trigger = dat[(lind + SignCbData_sep_count)..];
            data = dat[..lind];
            return true;
        }
    }
}
