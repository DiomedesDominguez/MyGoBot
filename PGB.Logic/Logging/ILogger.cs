namespace PGB.Logic.Logging
{
    using System;

    public interface ILogger
    {
        void Write(string message, LogLevel level = LogLevel.Info, ConsoleColor color = ConsoleColor.Black);
    }
}