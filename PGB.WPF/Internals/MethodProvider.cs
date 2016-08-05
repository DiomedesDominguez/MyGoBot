namespace PGB.WPF.Internals
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    internal class MethodProvider
    {
        public static void DisplayError(string message)
        {
            var num = (int) DisplayMessage("Error", message, MessageBoxButton.OK, MessageBoxImage.Hand);
        }

        public static void DisplayInformation(string message)
        {
            var num = (int) DisplayMessage("Information", message, MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        public static MessageBoxResult DisplayMessage(string title, string message, MessageBoxButton button,
            MessageBoxImage icon)
        {
            return MessageBox.Show(message, title, button, icon);
        }

        public static void DisplayWarning(string message)
        {
            var num = (int) DisplayMessage("Warning", message, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        public static void WithWaitCursor(Action action)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                action();
            }
            catch
            {
                throw;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }
}