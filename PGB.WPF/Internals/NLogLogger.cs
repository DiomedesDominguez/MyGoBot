namespace PGB.WPF.Internals
{
    using NLog;
    using System;

    internal class NLogLogger : PokemonGo.RocketAPI.Logging.ILogger
    {
        private static readonly Logger Logger = LogManager.GetLogger("API");

        public void Write(string message, PokemonGo.RocketAPI.LogLevel level = PokemonGo.RocketAPI.LogLevel.Info)
        {
            NLogLogger.Logger.Info(message);
        }

        public void Write(string message, PokemonGo.RocketAPI.LogLevel level = PokemonGo.RocketAPI.LogLevel.Info, ConsoleColor color = ConsoleColor.Black)
        {
            this.Write(message, level);
        }
    }
}