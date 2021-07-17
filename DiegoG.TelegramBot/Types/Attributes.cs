using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiegoG.TelegramBot.Types
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class BotCommandAttribute : Attribute
    {
        public BotKey BotKey { get; init; }

        /// <summary>
        /// Decorate the given class with this attribute
        /// </summary>
        /// <param name="botKey">An optional parameter, to specify certain commands only for specific bots</param>
        public BotCommandAttribute(BotKey botKey = Types.BotKey.Any)
        {
            BotKey = botKey;
        }
    }


    [Flags]
    public enum BotKey : uint
    {
        Any = 0,
        AA  = 1u << 0,
        AB  = 1u << 1,
        AC  = 1u << 2, 
        AD  = 1u << 3,
        AE  = 1u << 4, 
        AF  = 1u << 5,
        AG  = 1u << 6,
        AH  = 1u << 7,
        AI  = 1u << 8,
        AJ  = 1u << 9,
        AK  = 1u << 10,
        AL  = 1u << 11,
        AM  = 1u << 12,
        AN  = 1u << 13,
        AO  = 1u << 14,
        AP  = 1u << 15,
        AQ  = 1u << 16,
        AR  = 1u << 17,
        AS  = 1u << 18,
        AT  = 1u << 19,
        AU  = 1u << 20,
        AV  = 1u << 21,
        AW  = 1u << 22,
        AX  = 1u << 23,
        AY  = 1u << 24,
        AZ  = 1u << 25,
        BA  = 1u << 26,
        BB  = 1u << 27,
        BC  = 1u << 28,
        BD  = 1u << 29,
        BE  = 1u << 30,
        BF  = 1u << 31,
    };

}
