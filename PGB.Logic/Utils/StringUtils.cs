namespace PGB.Logic.Utils
{
    using System.Collections.Generic;
    using System.Linq;

    using POGOProtos.Inventory.Item;

    public static class StringUtils
    {
        #region Methods and other members

        public static string GetSummedFriendlyNameOfItemAwardList(IEnumerable<ItemAward> items)
        {
            var source = items as IList<ItemAward> ?? items.ToList();
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

        #endregion
    }
}