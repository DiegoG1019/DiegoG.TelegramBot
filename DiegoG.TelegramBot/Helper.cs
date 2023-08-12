using System.Text.RegularExpressions;

namespace DiegoG.TelegramBot;

public static class BotHelper
{
    private static readonly Regex LegalizeStringRegex = new(@"((_|\*|\[|\]|\(|\)|~|`|>|#|\+|-|=|\||{|}|\.|!))", RegexOptions.Compiled);
    /// <summary>
    /// Makes sure the given string is legal for sending to telegram, using MarkupV2
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string TelegramLegalizeMarkupV2(this string s)
        => Regex.Replace(s, @"((\(|\)|~|`|>|#|\+|-|=|\||!|\.))", @"\$&", RegexOptions.Compiled);

    /// <summary>
    /// Makes sure the given string is legal for sending to telegram, using regular text
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string TelegramLegalizeRegular(this string s)
        => Regex.Replace(s, @"((_|\*|\[|\]|\(|\)|~|`|>|#|\+|=|\||{|}))", @"\$&", RegexOptions.Compiled);
    //'_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!'
}
