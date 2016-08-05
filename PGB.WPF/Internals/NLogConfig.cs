namespace PGB.WPF.Internals
{
    using NLog;
    using NLog.Common;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;

    internal static class NLogConfig
    {
        public static string BaseLogDirectory { get; private set; }

        public static bool IsDebugConsoleOutputEnabled { get; private set; }

        public static bool IsErrorLoggingEnabled { get; private set; }

        public static bool IsOutputLoggingEnabled { get; private set; }

        public static bool IsWarningLoggingEnabled { get; private set; }

        public static void EnableDebugConsoleOutput()
        {
            if (IsDebugConsoleOutputEnabled)
            {
                return;
            }

            var configuration = LogManager.Configuration;
            var consoleTarget1 = new ConsoleTarget();
            var str = "consoleDebugOutput";
            consoleTarget1.Name = str;
            var consoleTarget2 = consoleTarget1;
            var loggingRule = new LoggingRule("*", consoleTarget2);
            loggingRule.EnableLoggingForLevel(LogLevel.Trace);
            loggingRule.EnableLoggingForLevel(LogLevel.Debug);
            var consoleTarget3 = consoleTarget2;
            configuration.AddTarget(consoleTarget3);
            configuration.LoggingRules.Add(loggingRule);
            IsDebugConsoleOutputEnabled = true;
        }

        public static void EnableErrorLogging()
        {
            if (IsErrorLoggingEnabled)
            {
                return;
            }

            var configuration = LogManager.Configuration;
            var fileTarget1 = new FileTarget();
            var str = "fileError";
            fileTarget1.Name = str;
            Layout layout1 =
                "-------------- ${level} (${longdate}) --------------${newline}${newline}Call Site: ${callsite}${newline}Type: ${exception:format=Type}${newline}Message: ${exception:format=Message}${newline}Stack Trace: ${exception:format=StackTrace}${newline}${newline}";
            fileTarget1.Layout = layout1;
            Layout layout2 = BaseLogDirectory + "\\Error\\error.txt";
            fileTarget1.FileName = layout2;
            Layout layout3 = BaseLogDirectory + "\\Error\\error.${shortdate}-{#}.txt";
            fileTarget1.ArchiveFileName = layout3;
            var num1 = 3;
            fileTarget1.ArchiveNumbering = (ArchiveNumberingMode) num1;
            long num2 = 10485760;
            fileTarget1.ArchiveAboveSize = num2;
            var num3 = 10;
            fileTarget1.MaxArchiveFiles = num3;
            var num4 = 1;
            fileTarget1.KeepFileOpen = num4 != 0;
            var num5 = 1;
            fileTarget1.ConcurrentWrites = num5 != 0;
            var fileTarget2 = fileTarget1;
            var fileTarget3 = fileTarget2;
            configuration.AddTarget(fileTarget3);
            configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, fileTarget2));
            IsErrorLoggingEnabled = true;
        }

        public static void EnableOutputLogging()
        {
            if (IsOutputLoggingEnabled)
            {
                return;
            }

            var configuration = LogManager.Configuration;
            var fileTarget1 = new FileTarget();
            var str1 = "fileOutput";
            fileTarget1.Name = str1;
            Layout layout1 = "${longdate} | ${level:uppercase=true} | ${logger} | ${message}";
            fileTarget1.Layout = layout1;
            Layout layout2 = BaseLogDirectory + "\\Output\\output.txt";
            fileTarget1.FileName = layout2;
            Layout layout3 = BaseLogDirectory + "\\Output\\output.${shortdate}-{#}.txt";
            fileTarget1.ArchiveFileName = layout3;
            var num1 = 3;
            fileTarget1.ArchiveNumbering = (ArchiveNumberingMode) num1;
            long num2 = 10485760;
            fileTarget1.ArchiveAboveSize = num2;
            var num3 = 10;
            fileTarget1.MaxArchiveFiles = num3;
            var num4 = 1;
            fileTarget1.KeepFileOpen = num4 != 0;
            var num5 = 1;
            fileTarget1.ConcurrentWrites = num5 != 0;
            var fileTarget2 = fileTarget1;
            var consoleTarget1 = new ConsoleTarget();
            var str2 = "consoleOutput";
            consoleTarget1.Name = str2;
            Layout layout4 = "${date:format=yyyy-MM-dd hh\\:mm\\:ss tt} | ${level:uppercase=true} | ${message}";
            consoleTarget1.Layout = layout4;
            var consoleTarget2 = consoleTarget1;
            var fileTarget3 = fileTarget2;
            configuration.AddTarget(fileTarget3);
            var consoleTarget3 = consoleTarget2;
            configuration.AddTarget(consoleTarget3);
            configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, fileTarget2));
            configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, consoleTarget2));
            IsOutputLoggingEnabled = true;
        }

        public static void EnableWarningLogging()
        {
            if (IsWarningLoggingEnabled)
            {
                return;
            }

            var configuration = LogManager.Configuration;
            var fileTarget1 = new FileTarget();
            var str = "fileWarnings";
            fileTarget1.Name = str;
            Layout layout1 = "${longdate} | ${level:uppercase=true} | ${logger} | ${message}";
            fileTarget1.Layout = layout1;
            Layout layout2 = BaseLogDirectory + "\\Warning\\warning.txt";
            fileTarget1.FileName = layout2;
            Layout layout3 = BaseLogDirectory + "\\Warning\\warning.${shortdate}-{#}.txt";
            fileTarget1.ArchiveFileName = layout3;
            var num1 = 3;
            fileTarget1.ArchiveNumbering = (ArchiveNumberingMode) num1;
            long num2 = 10485760;
            fileTarget1.ArchiveAboveSize = num2;
            var num3 = 10;
            fileTarget1.MaxArchiveFiles = num3;
            var num4 = 1;
            fileTarget1.KeepFileOpen = num4 != 0;
            var num5 = 1;
            fileTarget1.ConcurrentWrites = num5 != 0;
            var fileTarget2 = fileTarget1;
            var loggingRule = new LoggingRule("*", fileTarget2);
            loggingRule.EnableLoggingForLevel(LogLevel.Warn);
            var fileTarget3 = fileTarget2;
            configuration.AddTarget(fileTarget3);
            configuration.LoggingRules.Add(loggingRule);
            IsWarningLoggingEnabled = true;
        }

        public static void Init()
        {
            BaseLogDirectory = ApplicationEnvironment.LogsDirectory();
            LogManager.ThrowExceptions = false;
            InternalLogger.LogToConsole = false;
            InternalLogger.LogLevel = LogLevel.Error;
            LogManager.Configuration = new LoggingConfiguration();
            EnableOutputLogging();
            EnableWarningLogging();
            EnableErrorLogging();
            LogManager.ReconfigExistingLoggers();
        }
    }
}