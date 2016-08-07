namespace PGB.WPF.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;

    using GMap.NET;

    using Internals;

    using Logic;

    using Models;

    using NLog;

    using PokemonGo.RocketAPI.Rpc;

    using POGOProtos.Enums;

    using Xceed.Wpf.Toolkit.Primitives;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IComponentConnector
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            Settings.Load();
            Model = Settings.MainWindowModel;
            Model.IsLoggedIn = true;
            DataContext = Model;
            var num = 0;
            backgroundTask =
                new BackgroundLoopingTask(() => num = Task.Run(async () =>
                {
                    try
                    {
                        PGB.Logic.Logging.Logger.SetLogger(new NLogLogger());
                        await logic.Execute();
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                    await Task.Delay(5000);
                    return true;
                }).Result
                    ? 1
                    : 0);
            SetupUI();
        }

        #endregion

        #region Fields, properties, indexers and constants

        private static BackgroundLoopingTask backgroundTask;
        private static ListBoxWriter listBoxWriter;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly Random random = new Random();
        private Logic logic;

        public static MainWindow Instance { get; private set; }

        internal MainWindowModel Model { get; }

        #endregion

        #region Methods and other members

        private void btnOpenLogsDirectory_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", ApplicationEnvironment.LogsDirectory());
        }

        private async void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var start = btnStartStop.Content.ToString().ToLower().Contains("start");
                if (start)
                {
                    logic = new Logic(Model);
                    logic._client.Player.UpdatePositionEvent +=
                        (Player.UpdatePositionDelegate)
                            (async (lat, lng, alt) =>
                                await
                                    gMap.SafeAccessAsync(
                                        g => g.Position = new PointLatLng(lat, lng)));
                    Model.Statistics = logic._stats;
                    await backgroundTask.Start();
                    Model.Status = "Running";
                }
                else
                {
                    await backgroundTask.Stop();
                    Model.Status = null;
                }
                btnStartStop.IsEnabled = false;
                btnStartStop.Content = start ? "Close the program to stop" : "Start";
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void clbPokemonToCatch_ItemSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
        {
            var str = e.Item as string;
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            var pokemonId = PokemonIdFromString(str);
            if (e.IsSelected)
            {
                Model.PokemonsToCatch.Add(pokemonId);
            }
            else
            {
                Model.PokemonsToCatch.Remove(pokemonId);
            }
        }

        private void clbPokemonToEvolve_ItemSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
        {
            var str = e.Item as string;
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            var pokemonId = PokemonIdFromString(str);
            if (e.IsSelected)
            {
                Model.PokemonsToEvolve.Add(pokemonId);
            }
            else
            {
                Model.PokemonsToEvolve.Remove(pokemonId);
            }
        }

        private void clbPokemonToTransfer_ItemSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
        {
            var str = e.Item as string;
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            var pokemonId = PokemonIdFromString(str);
            if (e.IsSelected)
            {
                Model.PokemonsToTransfer.Add(pokemonId);
            }
            else
            {
                Model.PokemonsToTransfer.Remove(pokemonId);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private PokemonId PokemonIdFromString(string value)
        {
            return (PokemonId) Enum.Parse(typeof(PokemonId), value, true);
        }


        private void SetupUI()
        {
            var generalSettings = Settings.GeneralSettings;
            listBoxWriter = new ListBoxWriter(lbLog);
            Title = $"PGB v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)} {string.Empty}";
            Console.SetOut(listBoxWriter);
            Console.SetError(listBoxWriter);
            if (generalSettings == null || generalSettings.WindowLeft <= 0.0 || generalSettings.WindowTop <= 0.0)
            {
                return;
            }

            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = generalSettings.WindowLeft;
            Top = generalSettings.WindowTop;
            Width = generalSettings.WindowWidth;
            Height = generalSettings.WindowHeight;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var generalSettings = Settings.GeneralSettings;
            var left = Left;
            generalSettings.WindowLeft = left;
            var top = Top;
            generalSettings.WindowTop = top;
            var width = Width;
            generalSettings.WindowWidth = width;
            var height = Height;
            generalSettings.WindowHeight = height;
            Settings.Save();
            Environment.Exit(0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Model.PokemonsToCatch == null)
            {
                Model.PokemonsToEvolve = new List<PokemonId>();
                Model.PokemonsToTransfer = new List<PokemonId>();
                Model.PokemonsToCatch = new List<PokemonId>();

            }
            foreach (PokemonId pokemonId in Enum.GetValues(typeof(PokemonId)))
            {
                if (!Model.PokemonsToEvolve.Contains(pokemonId))
                {
                    Model.PokemonsToEvolve.Add(pokemonId);
                }
                if (!Model.PokemonsToTransfer.Contains(pokemonId))
                {
                    Model.PokemonsToTransfer.Add(pokemonId);
                }
                if (!Model.PokemonsToCatch.Contains(pokemonId))
                {
                    Model.PokemonsToCatch.Add(pokemonId);
                }

                if (Model.PokemonsToEvolve.Contains(pokemonId))
                {
                    clbPokemonToEvolve.SelectedItems.Add(pokemonId.ToString());
                }
                if (Model.PokemonsToTransfer.Contains(pokemonId))
                {
                    clbPokemonToTransfer.SelectedItems.Add(pokemonId.ToString());
                }
                if (Model.PokemonsToCatch.Contains(pokemonId))
                {
                    clbPokemonToCatch.SelectedItems.Add(pokemonId.ToString());
                }
            }

            clbPokemonToEvolve.ItemSelectionChanged +=
                clbPokemonToEvolve_ItemSelectionChanged;
            clbPokemonToTransfer.ItemSelectionChanged +=
                clbPokemonToTransfer_ItemSelectionChanged;
            clbPokemonToCatch.ItemSelectionChanged +=
                clbPokemonToCatch_ItemSelectionChanged;
            logger.Info("Welcome to PGB!");
            gMap.Position = new PointLatLng(Model.DefaultLatitude, Model.DefaultLongitude);
            gMap.DragButton = MouseButton.Left;
            gMap.CenterCrossPen =
                new Pen(
                    new ImageBrush(
                        new BitmapImage(
                            new Uri(@"pack://application:,,,/"
                                    + Assembly.GetExecutingAssembly().GetName().Name
                                    + ";component/Resources/pokemon_ball_16x16.png",
                                UriKind.Absolute))), 16.0);
        }

        #endregion
    }
}