namespace PGB.WPF.Internals
{
    using System;
    using System.IO;

    internal static class StringExtensions
    {
        #region Methods and other members

        public static string SanitizeFileName(this string source)
        {
            return string.Join("_", source.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion
    }
}