using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DiegoG.TelegramBot
{

    [Serializable]
    public class InvalidBotCommandException : Exception
    {
        public InvalidBotCommandException() { }
        public InvalidBotCommandException(string command, string message) : base($"Command \"{command}\" is Invalid: {message}") { }
        public InvalidBotCommandException(string command, string message, Exception inner) : base($"Command \"{command}\" is Invalid: {message}", inner) { }
        protected InvalidBotCommandException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class BotCommandProcessException : Exception
    {
        public BotCommandProcessException() { }
        public BotCommandProcessException(string command, string message) : base($"Command \"{command}\": {message}") { }
        public BotCommandProcessException(string command, string message, Exception inner) : base($"Command \"{command}\": {message}", inner) { }
        protected BotCommandProcessException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class InvalidBotCommandArgumentsException : Exception
    {
        public InvalidBotCommandArgumentsException() { }
        public InvalidBotCommandArgumentsException(string command, string message) : base($"Arguments for command \"{command}\" are Invalid: {message}") { }
        public InvalidBotCommandArgumentsException(string command, string message, Exception inner) : base($"Arguments for command \"{command}\" are Invalid: {message}", inner) { }
        protected InvalidBotCommandArgumentsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class InvalidBotCommandUserRightsException : Exception
    {
        public InvalidBotCommandUserRightsException() { }
        public InvalidBotCommandUserRightsException(User user, string command) : base($"User {user} does not have the rights to execute {command}") { }
        public InvalidBotCommandUserRightsException(User user, string command, Exception inner) : base($"User {user} does not have the rights to execute {command}", inner) { }
        public InvalidBotCommandUserRightsException(int user, string command) : base($"User of id {user} does not have the rights to execute {command}") { }
        public InvalidBotCommandUserRightsException(int user, string command, Exception inner) : base($"User of id {user} does not have the rights to execute {command}", inner) { }
        protected InvalidBotCommandUserRightsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
