namespace DiegoG.TelegramBot.Types.ChatSequenceTypes;

public record Advancement(string? EnterValue, Advancement.SuccessCode Success)
{
    public enum SuccessCode
    { Success, EndOfSequence, Failure }
}

public record Response(string ResponseValue, Response.ResponseAction Action)
{
    public enum ResponseAction
    { Advance, Continue, End }
}
