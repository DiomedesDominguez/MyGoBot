namespace PGB.WPF.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Timers;

    using GMap.NET;
    using GMap.NET.MapProviders;

    using Newtonsoft.Json;

    using NLog;

    using PokemonGo.RocketAPI;
    using PokemonGo.RocketAPI.Enums;
    using PokemonGo.RocketAPI.GeneratedCode;
    using PokemonGo.RocketAPI.Logic.Utils;

    using PostSharp.Patterns.Model;

    using Logger = NLog.Logger;

    [NotifyPropertyChanged]
    internal class MainWindowModel : ISettings
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private double altitude = 10.0;
        private AuthType authType = AuthType.Ptc;
        private int breakAfterMinutes = 60;
        private int breakForMinutes = 20;
        private PerformanceCounter cpuCounter;
        private float cpuUsage;
        private int delayBetweenPokemonCatch = 5000;
        private bool enableBreaks;
        private double evolveAboveIVValue = 95.0;
        private bool evolveAllPokemonAboveIV;
        private bool evolvePokemon;
        private string googleRefreshToken = string.Empty;
        private string gpxFile = string.Empty;
        private int greatBallCount = 50;
        private int hyperPotionCount = 50;
        private bool isLoggedIn;
        private int keepMinCp = 1000;
        private double keepMinIvPercentage = 95.0;
        private double latitude = 34.07362;
        private string loginPassword = string.Empty;
        private string loginToken = string.Empty;
        private string loginUsername = string.Empty;
        private double longitude = -118.400352;
        private int masterBallCount = 50;
        private int maxPotionCount = 50;
        private int maxReviveCount = 50;
        private int maxTravelDistanceInMeters = 1000;
        private PerformanceCounter memoryCounter;
        private float memoryUsage;
        private bool onlyFarmPokestops;
        private int pokeBallCount = 50;

        private List<PokemonId> pokemonsToCatch =
            new List<PokemonId>(Enum.GetValues(typeof(PokemonId)).OfType<PokemonId>());

        private List<PokemonId> pokemonsToEvolve =
            new List<PokemonId>(Enum.GetValues(typeof(PokemonId)).OfType<PokemonId>());

        private List<PokemonId> pokemonsToTransfer =
            new List<PokemonId>(Enum.GetValues(typeof(PokemonId)).OfType<PokemonId>());

        private int potionCount = 50;
        private bool prioritizeIVOverCP;
        private int razzBerryCount = 50;
        private int reviveCount = 50;
        private int selectedTabIndex;
        private Statistics statistics;
        private string status;
        private int superPotionCount = 50;
        private bool transferDuplicatePokemon = true;
        private int ultraBallCount = 50;
        private bool useGpxPathing;
        private bool useLuckyEggsWhileEvolving;
        private User user;
        private double walkingSpeedKmHr = 15.0;

        public MainWindowModel()
        {
            Task.Run(() =>
            {
                var timer = new Timer();
                if (PerformanceCounterCategory.CounterExists("% Processor Time", "Process"))
                {
                    cpuCounter = new PerformanceCounter("Process", "% Processor Time",
                        Process.GetCurrentProcess().ProcessName);
                }
                else
                {
                    logger.Error("Cannot display CPU usage (counter does not exist on your system)");
                }
                if (PerformanceCounterCategory.CounterExists("Working Set", "Process"))
                {
                    memoryCounter = new PerformanceCounter("Process", "Working Set",
                        Process.GetCurrentProcess().ProcessName);
                }
                else
                {
                    logger.Error("Cannot display RAM usage (counter does not exist on your system)");
                }
                var num1 = 2000.0;
                timer.Interval = num1;
                ElapsedEventHandler elapsedEventHandler = (s, e) =>
                {
                    if (cpuCounter != null)
                    {
                        CpuUsage = cpuCounter.NextValue();
                        double num2 = cpuCounter.NextValue();
                    }
                    if (memoryCounter == null)
                    {
                        return;
                    }

                    MemoryUsage = memoryCounter.NextValue();
                    double num3 = memoryCounter.NextValue();
                };
                timer.Elapsed += elapsedEventHandler;
                timer.Start();
            });
            Singleton<GMaps>.Instance.Mode = AccessMode.ServerOnly;
        }

        public int BreakAfterMinutes { get; set; }

        public int BreakForMinutes { get; set; }

        [JsonIgnore]
        public float CpuUsage { get; set; }

        public bool EnableBreaks { get; set; }

        [JsonIgnore, IgnoreAutoChangeNotification]
        public GMapProvider GMapProvider => GoogleMapProvider.Instance as GMapProvider;

        [JsonIgnore]
        public string GoogleRefreshToken { get; set; }

        public int GreatBallCount { get; set; }

        public int HyperPotionCount { get; set; }

        [JsonIgnore]
        public bool IsLoggedIn { get; set; }

        public string LoginToken { get; set; }

        public int MasterBallCount { get; set; }

        public int MaxPotionCount { get; set; }

        public int MaxReviveCount { get; set; }

        [JsonIgnore]
        public float MemoryUsage { get; set; }

        public int PokeBallCount { get; set; }

        public int PotionCount { get; set; }

        public int RazzBerryCount { get; set; }

        public int ReviveCount { get; set; }

        [JsonIgnore]
        public int SelectedTabIndex { get; set; }

        [JsonIgnore]
        public Statistics Statistics { get; set; }


        [JsonIgnore]
        public string Status { get; set; }

        public int SuperPotionCount { get; set; }

        public int UltraBallCount { get; set; }

        [JsonIgnore]
        public bool UsePokemonToNotCatchFilter => true;

        [JsonIgnore]
        public User User { get; set; }

        #region ISettings Members

        public AuthType AuthType { get; set; }

        public double DefaultAltitude { get; set; }

        public double DefaultLatitude { get; set; }

        public double DefaultLongitude { get; set; }

        public int DelayBetweenPokemonCatch { get; set; }

        public double EvolveAboveIVValue { get; set; }

        public bool EvolveAllPokemonAboveIV { get; set; }

        public bool EvolveAllPokemonWithEnoughCandy { get; set; }

        public string GPXFile { get; set; }

        [JsonIgnore]
        public List<KeyValuePair<ItemId, int>> ItemRecycleFilter
        {
            get
            {
                return new List<KeyValuePair<ItemId, int>>(new KeyValuePair<ItemId, int>[30]
                {
                    new KeyValuePair<ItemId, int>(ItemId.ItemUnknown, 0),
                    new KeyValuePair<ItemId, int>(ItemId.ItemPokeBall, PokeBallCount),
                    new KeyValuePair<ItemId, int>(ItemId.ItemGreatBall, GreatBallCount),
                    new KeyValuePair<ItemId, int>(ItemId.ItemUltraBall, UltraBallCount),
                    new KeyValuePair<ItemId, int>(ItemId.ItemMasterBall, MasterBallCount),
                    new KeyValuePair<ItemId, int>(ItemId.ItemPotion, PotionCount),
                    new KeyValuePair<ItemId, int>(ItemId.ItemSuperPotion, SuperPotionCount),
                    new KeyValuePair<ItemId, int>(ItemId.ItemHyperPotion, HyperPotionCount),
                    new KeyValuePair<ItemId, int>(ItemId.ItemMaxPotion, MaxPotionCount),
                    new KeyValuePair<ItemId, int>(ItemId.ItemRevive, ReviveCount),
                    new KeyValuePair<ItemId, int>(ItemId.ItemMaxRevive, MaxReviveCount),
                    new KeyValuePair<ItemId, int>(ItemId.ItemLuckyEgg, 200),
                    new KeyValuePair<ItemId, int>(ItemId.ItemIncenseOrdinary, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemIncenseSpicy, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemIncenseCool, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemIncenseFloral, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemTroyDisk, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemXAttack, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemXDefense, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemXMiracle, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemRazzBerry, RazzBerryCount),
                    new KeyValuePair<ItemId, int>(ItemId.ItemBlukBerry, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemNanabBerry, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemWeparBerry, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemPinapBerry, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemSpecialCamera, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemIncubatorBasicUnlimited, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemIncubatorBasic, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemPokemonStorageUpgrade, 100),
                    new KeyValuePair<ItemId, int>(ItemId.ItemItemStorageUpgrade, 100)
                });
            }
        }

        public int KeepMinCP { get; set; }

        [JsonIgnore]
        public int KeepMinDuplicatePokemon => 1;

        public double KeepMinIVPercentage { get; set; }

        public string LoginPassword { get; set; }

        public string LoginUsername { get; set; }

        public int MaxTravelDistanceInMeters { get; set; }

        public bool OnlyFarmPokestops { get; set; }

        public List<PokemonId> PokemonsToCatch { get; set; }

        public List<PokemonId> PokemonsToEvolve { get; set; }

        public List<PokemonId> PokemonsToTransfer { get; set; }

        public bool PrioritizeIVOverCP { get; set; }

        public bool TransferDuplicatePokemon { get; set; }

        public bool UseGPXPathing { get; set; }

        public bool UseLuckyEggsWhileEvolving { get; set; }

        public double WalkingSpeedInKilometerPerHour { get; set; }

        #endregion
    }
}