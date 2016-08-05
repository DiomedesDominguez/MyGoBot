namespace PGB.WPF.Internals
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Threading;

    internal static class DispatcherObjectExtensions
    {
        public static Task SafeAccessAsync<T>(this T dispatcherObject, Action<T> action)
            where T : DispatcherObject
        {
            return Task.Run(async () =>
            {
                var dispatcher = dispatcherObject.Dispatcher;
                if (!dispatcher.CheckAccess())
                {
                    await dispatcher.BeginInvoke(action, DispatcherPriority.Normal, dispatcherObject);
                }
                else
                {
                    action(dispatcherObject);
                }
            });
        }
    }
}