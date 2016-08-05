namespace PGB.WPF.Internals
{
    using PostSharp.Patterns.Model;

    [NotifyPropertyChanged]
    internal class CheckedListItem<T>
    {
        public CheckedListItem()
        {
        }

        public CheckedListItem(T item, bool isChecked = false)
        {
            Item = item;
            IsChecked = isChecked;
        }

        public T Item { get; set; }

        public bool IsChecked { get; set; }
    }
}