namespace PGB.WPF.Internals
{
    using System.Windows.Automation;
    using System.Windows.Automation.Peers;
    using System.Windows.Automation.Provider;
    using System.Windows.Controls;

    internal static class ListBoxExtensions
    {
        public static void ScrollToBottom(this ListBox source)
        {
            try
            {
                var scrollProvider =
                    (IScrollProvider)
                        UIElementAutomationPeer.CreatePeerForElement(source).GetPattern(PatternInterface.Scroll);
                var verticalAmount = ScrollAmount.LargeIncrement;
                var horizontalAmount = ScrollAmount.NoAmount;
                if (!scrollProvider.VerticallyScrollable)
                {
                    return;
                }

                scrollProvider.Scroll(horizontalAmount, verticalAmount);
            }
            catch
            {
            }
        }
    }
}