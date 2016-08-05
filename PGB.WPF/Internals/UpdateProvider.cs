namespace PGB.WPF.Internals
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;

    using Newtonsoft.Json;

    using NLog;

    internal static class UpdateProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static Task CheckForUpdates(string url)
        {
            return Task.Run(async () =>
            {
                try
                {
                    Logger.Info("Checking for updates...");
                    var response = await HttpProvider.Get(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var updateJson =
                            JsonConvert.DeserializeObject<UpdateJson>(await response.Content.ReadAsStringAsync());
                        var version = new Version(updateJson.Version);
                        if (version.CompareTo(Assembly.GetExecutingAssembly().GetName().Version) > 0)
                        {
                            if (
                                MethodProvider.DisplayMessage("Update",
                                    $"There is an application update available ({version}). Download now?", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                                MessageBoxResult.Yes)
                            {
                                Process.Start(updateJson.DownloadUrl);
                            }
                            else
                            {
                                Logger.Info("You are running an outdated version of this application!");
                            }
                        }
                        else
                        {
                            Logger.Info("You're running the latest application version!");
                        }
                    }
                    else
                    {
                        Logger.Error(
                            $"Failed checking for updates ({response.StatusCode} {response.StatusCode})");
                    }
                    response = null;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex,
                        $"Unhandled exception while checking for updates ({ex.InnerException.Message})");
                }
            });
        }
    }
}