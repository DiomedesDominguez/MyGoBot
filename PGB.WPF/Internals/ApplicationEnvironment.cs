namespace PGB.WPF.Internals
{
    using System;
    using System.IO;

    internal static class ApplicationEnvironment
    {
        static ApplicationEnvironment()
        {
            EnsureExists(SettingsDirectory());
            EnsureExists(LogsDirectory());
        }

        public static void EnsureExists(string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);
        }

        public static string LogsDirectory()
        {
            return string.Concat(MainDirectory(), "\\Logs");
        }

        public static string MainDirectory()
        {
            return string.Concat(UserProfile(), "\\GO Bot");
        }

        public static string SettingsDirectory()
        {
            return string.Concat(MainDirectory(), "\\Settings");
        }

        public static string UserProfile()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
    }
}