namespace PGB.WPF.Internals
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using PokemonGo.RocketAPI.GeneratedCode;

    internal static class StringExtensions
    {
        public static string GetSummedFriendlyNameOfItemAwardList(IEnumerable<FortSearchResponse.Types.ItemAward> items)
        {
            var source = items as IList<FortSearchResponse.Types.ItemAward> ?? items.ToList();
            if (!source.Any())
            {
                return string.Empty;
            }

            return source.GroupBy(i => i.ItemId).Select(kvp => new
            {
                ItemName = kvp.Key.ToString(),
                Amount = kvp.Sum(x => x.ItemCount)
            })
                .Select(y => $"{y.Amount} x {y.ItemName}")
                .Aggregate((a, b) => $"{a}, {b}");
        }

        public static string SanitizeFileName(this string source)
        {
            return string.Join("_", source.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        }
    }
}