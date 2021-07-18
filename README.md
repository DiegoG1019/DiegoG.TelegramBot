# DiegoG.TelegramBot
A Library to simplify Bot Command building and Conversation definition

## Installation
[NuGet v.1.7.2](https://www.nuget.org/packages/DiegoG.TelegramBot/)
```nuget
Install-Package DiegoG.TelegramBot -Version 1.7.2
```
```powershell
dotnet add package DiegoG.TelegramBot --version 1.7.2
```

## Classes and Usage
### TelegramBotCommandClient
This is the center-piece of the entire library: The replacement for the standard `TelegramBotClient`

In order to make use of most of its qualities, two main points are required: Declaring a command (further explain below) and utilizing the `MessageQueue`
The simplest way to use the `MessageQueue` is to make use of the two methods in `TelegramBotCommandClient` that interact directly with it, namely `void QueueBotAction(BotAction action)` and `Task<TResult> QueueBotFunc(BotFunc func)`
Both queue a task to the MessageQueue to be issued to the telegram API when the Queue deems it fit. 

#### Syntax:
```C#
new TelegramBotCommandClient(string token, int apiSaturation, BotKey key = BotKey.Any, Config? config = null, Func<Message, bool>? messageFilter = null, CommandProcessorUpdateHandler? updateHandler = null, HttpClient? client = null, string? baseUrl = null) : base(token, client, baseUrl)
```

This class integrates reflection-based command loading, API Request Queueing, as well as a virtual default message handler and much more

- `string token` The Telegram Bot API Key, as required by `TelegramBotClient`

- `int apiSaturation` represents the amount of requests that can be fired at once. 

- `BotKey key` represents an enum to tell which clients should load which commands. A bot marked with the `Any` key will *only* instantiate commands marked with the same key. A bot marked with a specific key, or a combination of them, will load all the matching commands as well as those who belong to the `any` group. Defaults to `BotKey.Any`

- `Config? config` represents certain configuration parameters for the CommandClient. Defaults to null for the default values.
 
  - `ProcessNormalMessages` states whether the bot should process all messages, or if false, only those starting with a `/`.
  - `AddBotMeCommandInfo` states whether the bot should automatically upload info about its commands to telegram 
  
   ![AddBotMeCommandInfo Image Example](https://github.com/DiegoG1019/DiegoG.TelegramBot/blob/master/repo_assets/AddBotMeCommandInfo%20Image%20Example.png)
   
```C# 
new Config(bool ProcessNormalMessages = true, bool AddBotMeCommandInfo = true)
```

- `Func<Message, bool>? messageFilter` a function that ensures the messages passing through to the bot are of interest. Return `true` to let the message pass, `false` to ignore it. The default lets all message through.

- `CommandProcessorUpdateHandler? updateHandler` The object in charge of handling the updates, as required by `Telegram.Bot.Extensions.Polling`, adjusted to the library's requirements. All relevant methods are virtual, and can be thus overriden.

### MessageQueue
The MessageQueue is the object in charge of managing the transmission of API requests over to the telegram API, dedicated to ensuring that Telegram's Rate Limiting Standards are met
__(As of the time of writing, much work is still left to be put into the MessageQueue, but it should work for most scenarios)__

Using `QueueBotFunc` ensures that the Telegram API doesn't ban you for exceeding the rate limiting, but as a result, can take up quite some time to return a value, depending on the MessageQueue's congestion.

Each request is queued and executed in order, with a `100ms` delay between each request per burst. After each request is fired, a second list contains the times at which they were fired, where they remain for a minute. The total count of fired requests in the list is compared against `apiSaturation` to hopefully prevent  `Too Many API Requests` related exceptions

## Bot Commands
Bot Commands are the heart of this entire library. They simplify interaction code with the bot, and are the whole reason this whole library exists in the first place. Each command is declared as a class decorated with `[BotCommand]` and implementing `IBotCommand`. 

#### Runtime Startup Loading

Upon instantiating a `TelegramBotCommandClient`, the object will scan the assembly in search for classes decorated with `[BotCommand]` and, if they also implement `IBotCommand`, they will be loaded and added as commands for the bot.

#### Mid Runtime Loading

If a new assembly is loaded onto the application, or otherwise command data changes, `TelegramBotCommandClient` offers the 
```C#
void LoadNewCommands(params Assembly[] assemblies)
``` 
Method for this exact purpose. The same rules as above apply, except that commands that have already been loaded will be ignored.

### BotCommandAttribute
This attribute is used to decorate classes that are destined to become Bot Commands.
```C#
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class BotCommandAttribute : Attribute
```
```C#
[BotCommand(BotKey botKey = Types.BotKey.Any)]
```
Just as the rules regarding `BotKey` above specify, a command marked as `BotKey.Any` will be instantiated for each and every `TelegramBotCommandClient`, while those marked with a specific key, or combination of keys thereof, will only be instantiated for the respectively marked `TelegramBotCommandClient`s

`BotCommandAttribute` is only usable on classes, is **not** inheritable, and does not allow multiple decorations of itself.

The non-inheritable rule is exploitable, as you can create abstract Commands that can be inherited by other commands and only instantiate those.

### IBotCommand
```C#
public interface IBotCommand
{
	TelegramBotCommandClient Processor { get; set; }

	Task<CommandResponse> Action(BotCommandArguments args);

	Task<CommandResponse> ActionReply(BotCommandArguments args);

	void Cancel(User user);

	string HelpExplanation { get; }
	
	string HelpUsage { get; }
	
	IEnumerable<OptionDescription>? HelpOptions { get; }
	
	string Trigger { get; }
	
	string? Alias { get; }
	
	Task AnswerCallbackQuery(User user, CallbackQuery query); //Has a default implementation
	
	public bool Validate([NotNullWhen(false)]out string? message); //Has a default implementation
}
```

```C#
new CommandResponse(bool hold = false, params BotAction[] actions)
new CommandResponse(Message msg, bool hold = false, params string[] messages)
new CommandResponse(BotCommandArguments args, bool hold = false, params string[] messages)
```

```C#
public sealed record BotCommandArguments
{
    public Message Message { get; init; }
    public string ArgString { get; init; }
    public ChatId FromChat { get; init; }
    public User User { get; init; }
    public string[] Arguments { get; init; }
}
```

- `Processor` is the `TelegramBotCommandClient` this command's instance belongs to. It's automatically set by the Client upon loading, but due to limitations, it's freely settable. Please refrain from doing that.
- `Action` It's the response issued by the bot upon the command being executed.
  - `bool hold` if this is set to true, it specifies to the command execution engine that all subsequent messages from that user forwarded to `ActionReply` until `ActionReply` returns `hold = false`. This can be used for multi-message conversations with the user, and it's used extensively by the `ChatBot` class (explained below)
  - `params BotAction[] action` are the actions to be queued by the execution engine as soon as the command returns. They are executed in order.
  - `params string[] Message` each of the strings here passed are translated into an array of `b => b.SendTextMessageAsync(msg.Chat.Id, m, replyToMessageId: msg.MessageId);` and stored. __`Message msg` or `BotCommandArguments args` are required for this__
- `ActionReply` when `bool hold` is set to true by a previous `Action` or held true by `ActionReply` *all* messages by that specific user will be automatically forwarded to `ActionReply`. This is usually done to hold conversations with the user. The command is responsible for maintaining the conversation's state, and thus preferably, responsible for maintaining a way to obtain per-user state, such as the use of a `Dictionary<long, TContext>` using `User.Id` as the `long` key

- `Cancel` This method must be called manually. (for now) and is responsible for clearing any command-held data relating the user.

- `string Explanation` A string to explain the purpose and effects of the command

- `string HelpUsage` A string to explain the usage and syntax of the command, in format of `CommandName [Argument] (OptionalArgument)`

- `IEnumerable<OptionDescription>? HelpOptions` A collection of `OptionDescription` objects to name and describe the optional arguments of the command, if any. Set to null to ignore. Mainly used by the default `/help` command

- `string Trigger` The string to be matched when registering the command and when the user communicates with the bot. If the first word sent by the user matches this trigger, `Action` is called. **Cannot be set to `null`. Case Insensitive. `/` not included. If desired, must be added manually.**

- `string Alias` A secondary, optional string to be used as a secondary, usually shorter trigger for the command. Also Case Insensitive. Set to `null` to ignore.

- `bool Validate(out string? message)` A command that is called when the command is instantiated. Responsible for making sure everything is *just* right for the Command. `message` should only be used in case of failure (returning `false`), as hinted by the `[NotNullWhen(false)]` attribute

- `Task AnswerCallbackQuery(User user, CallbackQuery query)` When a CallbackQuery bound to this command is received, this method is called. Usually, a CallbackQuery is known to be bound to this command when it's signed by it using the `SignCallbackData` extension method (You'll perhaps have to write `this.SignCallbackData` for it to work)

### Callback Queries
Callback Query handling for commands is supported!
All you have to do is call the `SignCallbackData` extension method and put the result in the QueryData, and add your desired behaviour by overriding `IBotCommand`'s `AnswerCallbackQuery` method
Everything else will be done by the default implementations of TelegramBotCommandClient update handlers
```C#
SignCallbackData(this IBotCommand botCommand, string? CallbackData = null)
```

### Default Command 
```C#
public abstract class Default : IBotCommand
```
Represents the Default Response to an unknown command, or unrecognized normal message (when another command is not being held). This class cannot be instantiated. This class ___should___ be inherited and decorated with `[BotCommand]` in order to be recognized as a command.

### ChatBot Command
```C#
public abstract class ChatBot<TContext> : Default where TContext : IChatSequenceContext, new()
```

```C#
public interface IChatSequenceContext
{
    ChatSequence Sequence { get; set; }
    User User { get; }
}
```

This command is the base class for those who wish to hold more intricate conversations with their users. It provides a way of creating conversations, by dividing them into steps (`ChatSequenceStep`), where each one is a stand-alone object tied to its own context. This class can be inherited and used by any command, either `Default` to respond to any unknown commands or unrecognized messages, or to commands to trigger different conversations via keywords or commands.

#### ChatSequenceStep

```C#
public class ChatSequenceStep<TContext> where TContext : IChatSequenceContext, new()
{
    public string Name { get; init; };

    public Func<TContext, bool> Condition { get; init; };

    public Func<TContext, Task<string>> StepEntered { get; init; };

    public Func<TContext, BotCommandArguments, Task<Response>> Response { get; init; };

    public IEnumerable<ChatSequenceStep<TContext>>? Children;
}
```

```C#
new ChatSequenceStep()
//* Should only be used in cases where you plan to use an `ObjectInitializer` to otherwise fill the object's properties
``` 

```C#
new ChatSequenceStep(string name, Func<TContext, bool> condition, Func<TContext, Task<string>> stepEnter, Func<TContext, BotCommandArguments, Task<Response>> response, IOrderedEnumerable<ChatSequenceStep<TContext>> children)
```

The centerpiece of the conversation model.
Each step of the conversation is represented by an object such as this, and derived paths for the conversation are its children, in a tree collection structure. Refer to `DiegoG.TelegramBot.Tests` for examples.

- `string Name` Represents the non user friendly name of the step. It must be unique across all steps in the conversation tree.
- `Func<TContext, bool> Condition` Represents the conditions required for this step to be entered when its parent decides to `Advance` the tree. While advancing, the parent iterates through all its children and enters the first step whose condition returns `true`
- `Func<TContext, Task<string>> StepEntered` A method to be called when the step is entered. Should return a message continuing the conversation.
- `Response<TContext, BotCommandArguments, Task<Response>> Response` The action to be taken in response to the user's input. `BotCommandArguments` represent the user's input, and the function should return a `Response` based on that.
  - `ResponseValue` Represents the message to be sent to the user (currently no support for BotActions directly, but `Processor` should be accessible from here)
  - `Response.ResponseAction` Represents the action to be taken after this response is processed.
    - `Advance` Instructs the ChatSequence to attempt to advance the conversation.
    - `Continue` Instructs the ChatSequence to repeat the same step. (`StepEntered` is *NOT* triggered)
    - `End` Instructs the ChatSequence to end the conversation.

```C#
public record Response(string ResponseValue, Response.ResponseAction Action) 
{
    public enum ResponseAction
    { Advance, Continue, End }
}
```

### Deriving from TelegramBotCommandClient

Despite my attempts at increasingly making the library easier for general purpose work and making the entire thing play easier with the whole concept of individual uncoupled commands and interaction sequences, there will inevitably be a time where you simply would rather change certain aspects of the class, or replacing them altogether. Bearing this in mind, I made all the Update Handlers `virtual`, which means they can be safely overriden.

Furthermore, if this isn't enough, the `UpdateHandler` itself also has its method marked as `virtual`.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)
