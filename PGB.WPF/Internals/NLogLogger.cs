namespace PGB.WPF.Internals
{
    using NLog;
    using System;

    internal class NLogLogger : PGB.Logic.Logging.ILogger
    {
        private static NLog.Logger logger = LogManager.GetLogger("API");

        public void Write(string message, PGB.Logic.Logging.LogLevel level = PGB.Logic.Logging.LogLevel.Info)
        {
            NLogLogger.logger.Info(message);
        }

        public void Write(string message, PGB.Logic.Logging.LogLevel level = PGB.Logic.Logging.LogLevel.Info, ConsoleColor color = ConsoleColor.Black)
        {
            this.Write(message, level);
        }
    }
}