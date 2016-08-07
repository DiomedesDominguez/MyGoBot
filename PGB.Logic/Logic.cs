namespace PGB.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Extensions;

    using Google.Protobuf.Collections;

    using Logging;

    using PokemonGo.RocketAPI;
    using PokemonGo.RocketAPI.Exceptions;
    using PokemonGo.RocketAPI.Extensions;

    using POGOProtos.Data;
    using POGOProtos.Enums;
    using POGOProtos.Inventory.Item;
    using POGOProtos.Map.Fort;
    using POGOProtos.Map.Pokemon;
    using POGOProtos.Networking.Responses;

    using Utils;

    public class Logic
    {
        #region Constructors

        public Logic(ISettings clientSettings)
        {
            var apiFailureStrategy = new ApiFailureStrategy();
            _clientSettings = clientSettings;
            _client = new Client(_clientSettings, apiFailureStrategy);
            apiFailureStrategy.Client = _client;
            _inventory = new Inventory(_client);
            _navigation = new Navigation(_client);
            _stats = new Statistics();
        }

        #endregion

        #region Fields, properties, indexers and constants

        public readonly Client _client;
        private readonly ISettings _clientSettings;
        public readonly Inventory _inventory;
        private readonly Navigation _navigation;
        public readonly Statistics _stats;
        private GetPlayerResponse _playerProfile;
        private readonly Random random = new Random();

        #endregion

        #region Methods and other members

        private async Task CatchEncounter(EncounterResponse encounter, MapPokemon pokemon)
        {
            var attemptCounter = 1;
            CatchPokemonResponse caughtPokemonResponse;
            do
            {
                var encounterResponse1 = encounter;
                float? nullable1;
                if (encounterResponse1 == null)
                {
                    nullable1 = new float?();
                }
                else
                {
                    var captureProbability1 = encounterResponse1.CaptureProbability;
                    if (captureProbability1 == null)
                    {
                        nullable1 = new float?();
                    }
                    else
                    {
                        var captureProbability2 = captureProbability1.CaptureProbability_;
                        nullable1 = captureProbability2 != null ? captureProbability2.FirstOrDefault() : new float?();
                    }
                }
                var probability = nullable1;
                var pokeball = await GetBestBall(encounter);
                int? nullable2;
                if (pokeball == ItemId.ItemUnknown)
                {
                    var format = "No Pokeballs - We missed a {0} with CP {1}";
                    // ISSUE: variable of a boxed type
                    var local1 = (Enum) pokemon.PokemonId;
                    var encounterResponse2 = encounter;
                    int? nullable3;
                    if (encounterResponse2 == null)
                    {
                        nullable2 = new int?();
                        nullable3 = nullable2;
                    }
                    else
                    {
                        var wildPokemon = encounterResponse2.WildPokemon;
                        if (wildPokemon == null)
                        {
                            nullable2 = new int?();
                            nullable3 = nullable2;
                        }
                        else
                        {
                            var pokemonData = wildPokemon.PokemonData;
                            if (pokemonData == null)
                            {
                                nullable2 = new int?();
                                nullable3 = nullable2;
                            }
                            else
                            {
                                nullable3 = pokemonData.Cp;
                            }
                        }
                    }
                    // ISSUE: variable of a boxed type
                    var local2 = (ValueType) nullable3;
                    Logger.Write(string.Format(format, local1, local2), LogLevel.Caught, ConsoleColor.Black);
                    break;
                }

                if (probability.HasValue && probability.Value < 0.35)
                {
                    var wildPokemon = encounter.WildPokemon;
                    int num1;
                    if (wildPokemon == null)
                    {
                        num1 = 0;
                    }
                    else
                    {
                        var pokemonData = wildPokemon.PokemonData;
                        nullable2 = pokemonData != null ? pokemonData.Cp : new int?();
                        var num2 = 400;
                        num1 = nullable2.GetValueOrDefault() > num2 ? (nullable2.HasValue ? 1 : 0) : 0;
                    }
                    if (num1 != 0)
                    {
                        goto label_25;
                    }
                }

                var encounterResponse3 = encounter;
                PokemonData poke;
                if (encounterResponse3 == null)
                {
                    poke = null;
                }
                else
                {
                    var wildPokemon = encounterResponse3.WildPokemon;
                    poke = wildPokemon != null ? wildPokemon.PokemonData : null;
                }
                if (PokemonInfo.CalculatePokemonPerfection(poke) < _clientSettings.KeepMinIVPercentage)
                {
                    goto label_27;
                }

                label_25:
                await UseBerry(pokemon.EncounterId, pokemon.SpawnPointId);
                label_27:
                var distance = LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude,
                    pokemon.Latitude, pokemon.Longitude);
                caughtPokemonResponse =
                    await
                        _client.Encounter.CatchPokemon(pokemon.EncounterId, pokemon.SpawnPointId, pokeball,
                            random.NextDouble(0.5, 1.0)*1.85 + 0.1, random.NextDouble() > 0.75 ? 0.0 : 1.0, 1.0);
                if (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
                {
                    foreach (var xp in caughtPokemonResponse.CaptureAward.Xp)
                    {
                        _stats.AddExperience(xp);
                    }

                    _stats.IncreasePokemons();
                    _stats.GetStardust(await _inventory.GetStarDust());
                }

                _stats.UpdateConsoleTitle(_inventory);
                var encounterResponse4 = encounter;
                RepeatedField<float> repeatedField;
                if (encounterResponse4 == null)
                {
                    repeatedField = null;
                }
                else
                {
                    var captureProbability = encounterResponse4.CaptureProbability;
                    repeatedField = captureProbability != null ? captureProbability.CaptureProbability_ : null;
                }
                if (repeatedField != null)
                {
                    Func<ItemId, string> func = a =>
                    {
                        switch (a)
                        {
                            case ItemId.ItemPokeBall:
                                return "Poke";
                            case ItemId.ItemGreatBall:
                                return "Great";
                            case ItemId.ItemUltraBall:
                                return "Ultra";
                            case ItemId.ItemMasterBall:
                                return "Master";
                            default:
                                return "Unknown";
                        }
                    };
                    var str1 = attemptCounter > 1
                        ? string.Format("{0} Attempt #{1}", caughtPokemonResponse.Status, attemptCounter)
                        : string.Format("{0}", caughtPokemonResponse.Status);
                    var format =
                        "({0}) | {1} Lvl {2} ({3}/{4} CP) ({5}% perfect) | Chance: {6}% | {7}m dist | with a {8}Ball.";
                    var objArray = new object[9];
                    objArray[0] = str1;
                    objArray[1] = pokemon.PokemonId;
                    var index1 = 2;
                    var wildPokemon1 = encounter.WildPokemon;
                    // ISSUE: variable of a boxed type
                    var local1 =
                        (ValueType) PokemonInfo.GetLevel(wildPokemon1 != null ? wildPokemon1.PokemonData : null);
                    objArray[index1] = local1;
                    var index2 = 3;
                    var wildPokemon2 = encounter.WildPokemon;
                    int? nullable3;
                    if (wildPokemon2 == null)
                    {
                        nullable2 = new int?();
                        nullable3 = nullable2;
                    }
                    else
                    {
                        var pokemonData = wildPokemon2.PokemonData;
                        if (pokemonData == null)
                        {
                            nullable2 = new int?();
                            nullable3 = nullable2;
                        }
                        else
                        {
                            nullable3 = pokemonData.Cp;
                        }
                    }
                    // ISSUE: variable of a boxed type
                    var local2 = (ValueType) nullable3;
                    objArray[index2] = local2;
                    var index3 = 4;
                    var wildPokemon3 = encounter.WildPokemon;
                    // ISSUE: variable of a boxed type
                    var local3 =
                        (ValueType) PokemonInfo.CalculateMaxCp(wildPokemon3 != null ? wildPokemon3.PokemonData : null);
                    objArray[index3] = local3;
                    var index4 = 5;
                    var wildPokemon4 = encounter.WildPokemon;
                    var @string =
                        Math.Round(
                            PokemonInfo.CalculatePokemonPerfection(wildPokemon4 != null
                                ? wildPokemon4.PokemonData
                                : null)).ToString("0.00");
                    objArray[index4] = @string;
                    var index5 = 6;
                    var captureProbability = encounter.CaptureProbability;
                    // ISSUE: variable of a boxed type
                    var local4 =
                        (ValueType)
                            Math.Round(
                                Convert.ToDouble(captureProbability != null
                                    ? captureProbability.CaptureProbability_.First()
                                    : new float?())*100.0, 2);
                    objArray[index5] = local4;
                    var index6 = 7;
                    // ISSUE: variable of a boxed type
                    var local5 = (ValueType) Math.Round(distance);
                    objArray[index6] = local5;
                    var index7 = 8;
                    var str2 = func(pokeball);
                    objArray[index7] = str2;
                    Logger.Write(string.Format(format, objArray), LogLevel.Caught, ConsoleColor.Black);
                }
                ++attemptCounter;
                await Task.Delay(random.Next(2000, 5000));
                probability = new float?();
            } while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed ||
                     caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);
        }

        private async Task EvolveAllPokemonWithEnoughCandy(IEnumerable<PokemonId> filter = null)
        {
            if (_clientSettings.UseLuckyEggsWhileEvolving)
            {
                await PopLuckyEgg(_client);
            }
            foreach (var pokemonData in await _inventory.GetPokemonToEvolve(filter))
            {
                var pokemon = pokemonData;
                var evolvePokemonResponse = await _client.Inventory.EvolvePokemon(pokemon.Id);
                Logger.Write(
                    evolvePokemonResponse.Result == EvolvePokemonResponse.Types.Result.Success
                        ? string.Format("{0} successfully for {1} xp", pokemon.PokemonId,
                            evolvePokemonResponse.ExperienceAwarded)
                        : string.Format("Failed {0}. EvolvePokemonOutProto.Result was {1}, stopping evolving {2}",
                            pokemon.PokemonId, evolvePokemonResponse.Result, pokemon.PokemonId), LogLevel.Evolve,
                    ConsoleColor.Black);
                await Task.Delay(random.Next(3000, 5000));
                pokemon = null;
            }
        }

        public async Task Execute()
        {
            Logger.Write(string.Format("Logging in via: {0}", _clientSettings.AuthType), LogLevel.Info,
                ConsoleColor.Black);
            var num = 0;
            object obj = null;
            try
            {
                await _client.Login.DoLogin();
                await PostLoginExecute();
            }
            catch (Exception ex)
            {
                obj = ex;
                num = 1;
            }
            if (num == 1)
            {
                var e = (Exception) obj;
                if (e is AccountNotVerifiedException)
                {
                    Logger.Write(
                        "The Pokemon GO account you're using is not verified or your username/password is incorrect",
                        LogLevel.Info, ConsoleColor.Black);
                }
                else if (e is InvalidResponseException)
                {
                    Logger.Write("Received an invalid response from Pokemon GO servers, servers may be offline",
                        LogLevel.Info, ConsoleColor.Black);
                }
                else if (e is PtcOfflineException)
                {
                    Logger.Write("PTC servers seem to be offline, please try again later", LogLevel.Info,
                        ConsoleColor.Black);
                }
                else if (e is GoogleOfflineException)
                {
                    Logger.Write("Google login servers seem to be offline, please try again later", LogLevel.Info,
                        ConsoleColor.Black);
                }
                else if (e is GoogleException)
                {
                    if (e.Message.ToLower().Contains("needsbrowser"))
                    {
                        Logger.Write(
                            "As you have Google Two Factor Auth enabled, you will need to create and use an App Specific Password",
                            LogLevel.Info, ConsoleColor.Black);
                        Logger.Write(
                            "Opening Google App-Passwords... please make a new App Password (use Other as Device)",
                            LogLevel.Info, ConsoleColor.Black);
                        Logger.Write(
                            "Opening app passwords guide in your browser, please close the bot or it will continue opening multiple tabs...",
                            LogLevel.Info, ConsoleColor.Black);
                        Process.Start("https://support.google.com/accounts/answer/185833?hl=en");
                        await Task.Delay(30000);
                    }
                    else
                    {
                        Logger.Write("Make sure you have entered the right Email & Password", LogLevel.Info,
                            ConsoleColor.Black);
                    }
                }
                else if (e.Message.ToLower().Contains("niantic"))
                {
                    Logger.Write("Pokemon GO servers are under load and returned an error, trying again...",
                        LogLevel.Info, ConsoleColor.Black);
                }
                else if (e.Message.ToLower().Contains("the value of the parameter must be from -90.0 to 90.0") ||
                         e.Message.ToLower().Contains("the value of the parameter must be from -180.0 to 180.0"))
                {
                    Logger.Write("Either your latitude or longitude coords are out of the possible range",
                        LogLevel.Info, ConsoleColor.Black);
                    Logger.Write("Make sure they are correct and if the bot is deleting the periods use commas",
                        LogLevel.Info, ConsoleColor.Black);
                }
                else
                {
                    Logger.Write(e.Message + " from " + e.Source, LogLevel.Info, ConsoleColor.Black);
                    Logger.Write("Got an exception, waiting 10 seconds and then trying automatic restart..",
                        LogLevel.Error, ConsoleColor.Black);
                }
                e = null;
            }
            obj = null;
            await Task.Delay(10000);
        }

        private async Task ExecuteCatchAllNearbyPokemons()
        {
            if (_clientSettings.OnlyFarmPokestops)
            {
                await
                    Task.Delay(random.Next(Math.Max(_clientSettings.DelayBetweenPokemonCatch, 5000),
                        Math.Max(_clientSettings.DelayBetweenPokemonCatch, 5000) + 2000));
            }
            else
            {
                Logger.Write("Looking for pokemon..", LogLevel.Debug, ConsoleColor.Black);
                var pokemons =
                    (await _client.Map.GetMapObjects()).Item1.MapCells.SelectMany(
                        i => (IEnumerable<MapPokemon>) i.CatchablePokemons)
                        .OrderBy(
                            i =>
                                LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude,
                                    _client.CurrentLongitude, i.Latitude, i.Longitude));
                foreach (var mapPokemon in pokemons)
                {
                    var pokemon = mapPokemon;
                    if (!_clientSettings.PokemonsToCatch.Contains(pokemon.PokemonId))
                    {
                        Logger.Write("Skipped " + pokemon.PokemonId, LogLevel.Info, ConsoleColor.Black);
                    }
                    else
                    {
                        await
                            Task.Delay(
                                LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude,
                                    _client.CurrentLongitude, pokemon.Latitude, pokemon.Longitude) > 100.0
                                    ? 15000
                                    : 5000);
                        var encounter =
                            await _client.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId);
                        if (encounter.Status == EncounterResponse.Types.Status.EncounterSuccess)
                        {
                            await CatchEncounter(encounter, pokemon);
                        }
                        else
                        {
                            Logger.Write(string.Format("Encounter problem: {0}", encounter.Status), LogLevel.Info,
                                ConsoleColor.Black);
                        }
                        if (encounter.Status == EncounterResponse.Types.Status.PokemonInventoryFull)
                        {
                            Logger.Write("Attempting to transfer Pokemon...", LogLevel.Info, ConsoleColor.Black);
                            if (_clientSettings.TransferDuplicatePokemon)
                            {
                                await TransferDuplicatePokemon(false);
                            }
                            else
                            {
                                Logger.Write("Pokemon transferring is disabled", LogLevel.Info, ConsoleColor.Black);
                            }
                        }
                        if (!Equals(pokemons.ElementAtOrDefault(pokemons.Count() - 1), pokemon))
                        {
                            await
                                Task.Delay(random.Next(Math.Max(_clientSettings.DelayBetweenPokemonCatch, 5000),
                                    Math.Max(_clientSettings.DelayBetweenPokemonCatch, 5000) + 2000));
                        }
                        encounter = null;
                        pokemon = null;
                    }
                }
            }
        }

        private async Task ExecuteFarmingPokestopsAndPokemons(bool path)
        {
            if (!path)
            {
                await ExecuteFarmingPokestopsAndPokemons();
            }
        }

        private async Task ExecuteFarmingPokestopsAndPokemons()
        {
            await Task.Delay(random.Next(5000, 8000));
            var distanceInMeters = LocationUtils.CalculateDistanceInMeters(_clientSettings.DefaultLatitude,
                _clientSettings.DefaultLongitude, _client.CurrentLatitude, _client.CurrentLongitude);
            if (_clientSettings.MaxTravelDistanceInMeters != 0 &&
                distanceInMeters > _clientSettings.MaxTravelDistanceInMeters)
            {
                Logger.Write(
                    string.Format("You're outside of your defined radius! Walking to start ({0}m away) in 5 seconds",
                        distanceInMeters), LogLevel.Warning, ConsoleColor.Black);
                await Task.Delay(random.Next(5000, 7000));
                Logger.Write("Moving to start location now.", LogLevel.Info, ConsoleColor.Black);
                var playerUpdateResponse =
                    await
                        _navigation.Move(
                            new GeoCoordinate(_clientSettings.DefaultLatitude, _clientSettings.DefaultLongitude),
                            _clientSettings.WalkingSpeedInKilometerPerHour, null, new CancellationToken(), false);
            }
            var pokestopList = await GetPokeStops();
            var stopsHit = 0;
            if (pokestopList.Count <= 0)
            {
                Logger.Write(
                    "No PokeStops found in your area, try a different lat/long (http://latlong.net) and a larger max distance",
                    LogLevel.Warning, ConsoleColor.Black);
            }
            while (pokestopList.Any())
            {
                pokestopList =
                    pokestopList.OrderBy(
                        i =>
                            LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude,
                                i.Latitude, i.Longitude)).ToList();
                var pokeStop = pokestopList[0];
                pokestopList.RemoveAt(0);
                var distance = LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude,
                    pokeStop.Latitude, pokeStop.Longitude);
                Logger.Write(
                    string.Format("{0} in ({1}m)",
                        (await _client.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude)).Name,
                        Math.Round(distance)), LogLevel.Info, ConsoleColor.DarkRed);
                var playerUpdateResponse =
                    await
                        _navigation.Move(new GeoCoordinate(pokeStop.Latitude, pokeStop.Longitude),
                            _clientSettings.WalkingSpeedInKilometerPerHour, ExecuteCatchAllNearbyPokemons,
                            new CancellationToken(), false);
                var fortSearchResponse =
                    await _client.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                if (fortSearchResponse.ExperienceAwarded > 0)
                {
                    _stats.AddExperience(fortSearchResponse.ExperienceAwarded);
                    _stats.UpdateConsoleTitle(_inventory);
                    Logger.Write(
                        string.Format("XP: {0}, Gems: {1}, Items: {2}", fortSearchResponse.ExperienceAwarded,
                            fortSearchResponse.GemsAwarded,
                            StringUtils.GetSummedFriendlyNameOfItemAwardList(fortSearchResponse.ItemsAwarded)),
                        LogLevel.Pokestop, ConsoleColor.Black);
                }
                await Task.Delay(random.Next(1000, 5000));
                var num = stopsHit + 1;
                stopsHit = num;
                if (num%5 == 0)
                {
                    stopsHit = 0;
                    await RecycleItems();
                    if (_clientSettings.EvolveAllPokemonWithEnoughCandy || _clientSettings.EvolveAllPokemonAboveIV)
                    {
                        await EvolveAllPokemonWithEnoughCandy(_clientSettings.PokemonsToEvolve);
                    }
                    if (_clientSettings.TransferDuplicatePokemon)
                    {
                        await TransferDuplicatePokemon(false);
                    }
                }
                pokeStop = null;
            }
        }

        private async Task<ItemId> GetBestBall(EncounterResponse encounter)
        {
            var encounterResponse1 = encounter;
            int? nullable1;
            if (encounterResponse1 == null)
            {
                nullable1 = new int?();
            }
            else
            {
                var wildPokemon = encounterResponse1.WildPokemon;
                if (wildPokemon == null)
                {
                    nullable1 = new int?();
                }
                else
                {
                    var pokemonData = wildPokemon.PokemonData;
                    nullable1 = pokemonData != null ? pokemonData.Cp : new int?();
                }
            }
            var pokemonCp = nullable1;
            var encounterResponse2 = encounter;
            PokemonData poke;
            if (encounterResponse2 == null)
            {
                poke = null;
            }
            else
            {
                var wildPokemon = encounterResponse2.WildPokemon;
                poke = wildPokemon != null ? wildPokemon.PokemonData : null;
            }
            var iV = Math.Round(PokemonInfo.CalculatePokemonPerfection(poke));
            var encounterResponse3 = encounter;
            float? nullable2;
            if (encounterResponse3 == null)
            {
                nullable2 = new float?();
            }
            else
            {
                var captureProbability = encounterResponse3.CaptureProbability;
                nullable2 = captureProbability != null ? captureProbability.CaptureProbability_.First() : new float?();
            }
            var proba = nullable2;
            var pokeBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemPokeBall);
            var greatBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemGreatBall);
            var ultraBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemUltraBall);
            var itemAmountByType = await _inventory.GetItemAmountByType(ItemId.ItemMasterBall);
            int? nullable3;
            if (itemAmountByType > 0)
            {
                nullable3 = pokemonCp;
                var num = 1200;
                if ((nullable3.GetValueOrDefault() >= num ? (nullable3.HasValue ? 1 : 0) : 0) != 0)
                {
                    return ItemId.ItemMasterBall;
                }
            }

            if (ultraBallsCount > 0)
            {
                nullable3 = pokemonCp;
                var num = 1000;
                if ((nullable3.GetValueOrDefault() >= num ? (nullable3.HasValue ? 1 : 0) : 0) != 0)
                {
                    return ItemId.ItemUltraBall;
                }
            }

            if (greatBallsCount > 0)
            {
                nullable3 = pokemonCp;
                var num = 750;
                if ((nullable3.GetValueOrDefault() >= num ? (nullable3.HasValue ? 1 : 0) : 0) != 0)
                {
                    return ItemId.ItemGreatBall;
                }
            }

            double? nullable4;
            if (ultraBallsCount > 0 && iV >= _clientSettings.KeepMinIVPercentage)
            {
                var nullable5 = proba;
                nullable4 = nullable5.HasValue ? nullable5.GetValueOrDefault() : new double?();
                var num = 0.4;
                if ((nullable4.GetValueOrDefault() < num ? (nullable4.HasValue ? 1 : 0) : 0) != 0)
                {
                    return ItemId.ItemUltraBall;
                }
            }

            if (greatBallsCount > 0 && iV >= _clientSettings.KeepMinIVPercentage)
            {
                var nullable5 = proba;
                nullable4 = nullable5.HasValue ? nullable5.GetValueOrDefault() : new double?();
                var num = 0.5;
                if ((nullable4.GetValueOrDefault() < num ? (nullable4.HasValue ? 1 : 0) : 0) != 0)
                {
                    return ItemId.ItemGreatBall;
                }
            }

            if (greatBallsCount > 0)
            {
                nullable3 = pokemonCp;
                var num = 300;
                if ((nullable3.GetValueOrDefault() >= num ? (nullable3.HasValue ? 1 : 0) : 0) != 0)
                {
                    return ItemId.ItemGreatBall;
                }
            }

            return pokeBallsCount <= 0
                ? (greatBallsCount <= 0
                    ? (ultraBallsCount <= 0
                        ? (itemAmountByType <= 0 ? ItemId.ItemUnknown : ItemId.ItemMasterBall)
                        : ItemId.ItemUltraBall)
                    : ItemId.ItemGreatBall)
                : ItemId.ItemPokeBall;
        }

        private async Task<List<FortData>> GetPokeStops()
        {
            return
                (await _client.Map.GetMapObjects()).Item1.MapCells.SelectMany(i => (IEnumerable<FortData>) i.Forts)
                    .Where(i =>
                    {
                        if (i.Type != FortType.Checkpoint ||
                            i.CooldownCompleteTimestampMs >= DateTime.UtcNow.ToUnixTime())
                        {
                            return false;
                        }
                        if (
                            LocationUtils.CalculateDistanceInMeters(_client.Settings.DefaultLatitude,
                                _client.Settings.DefaultLongitude, i.Latitude, i.Longitude) >=
                            _client.Settings.MaxTravelDistanceInMeters)
                        {
                            return _client.Settings.MaxTravelDistanceInMeters == 0;
                        }

                        return true;
                    }).ToList();
        }

        private async Task<List<FortData>> GetPokeStopsGpx()
        {
            return
                (await _client.Map.GetMapObjects()).Item1.MapCells.SelectMany(i => (IEnumerable<FortData>) i.Forts)
                    .Where(i =>
                    {
                        if (i.Type != FortType.Checkpoint ||
                            i.CooldownCompleteTimestampMs >= DateTime.UtcNow.ToUnixTime() ||
                            LocationUtils.CalculateDistanceInMeters(_client.CurrentLatitude, _client.CurrentLongitude,
                                i.Latitude, i.Longitude) >= 40.0)
                        {
                            return _client.Settings.MaxTravelDistanceInMeters == 0;
                        }

                        return true;
                    }).ToList();
        }


        public static int GetXpDiff(int level)
        {
            switch (level)
            {
                case 1:
                    return 0;
                case 2:
                    return 1000;
                case 3:
                    return 2000;
                case 4:
                    return 3000;
                case 5:
                    return 4000;
                case 6:
                    return 5000;
                case 7:
                    return 6000;
                case 8:
                    return 7000;
                case 9:
                    return 8000;
                case 10:
                    return 9000;
                case 11:
                    return 10000;
                case 12:
                    return 10000;
                case 13:
                    return 10000;
                case 14:
                    return 10000;
                case 15:
                    return 15000;
                case 16:
                    return 20000;
                case 17:
                    return 20000;
                case 18:
                    return 20000;
                case 19:
                    return 25000;
                case 20:
                    return 25000;
                case 21:
                    return 50000;
                case 22:
                    return 75000;
                case 23:
                    return 100000;
                case 24:
                    return 125000;
                case 25:
                    return 150000;
                case 26:
                    return 190000;
                case 27:
                    return 200000;
                case 28:
                    return 250000;
                case 29:
                    return 300000;
                case 30:
                    return 350000;
                case 31:
                    return 500000;
                case 32:
                    return 500000;
                case 33:
                    return 750000;
                case 34:
                    return 1000000;
                case 35:
                    return 1250000;
                case 36:
                    return 1500000;
                case 37:
                    return 2000000;
                case 38:
                    return 2500000;
                case 39:
                    return 1000000;
                case 40:
                    return 1000000;
                default:
                    return 0;
            }
        }

        private async Task PopLuckyEgg(Client client)
        {
            await Task.Delay(1000);
            await UseLuckyEgg(client);
            await Task.Delay(1000);
        }

        public async Task PostLoginExecute()
        {
            while (true)
            {
                var logic = this;
                var getPlayerResponse = logic._playerProfile;
                var player = await _client.Player.GetPlayer();
                logic._playerProfile = player;
                logic = null;
                _stats.SetUsername(_playerProfile);
                if (_clientSettings.EvolveAllPokemonWithEnoughCandy || _clientSettings.EvolveAllPokemonAboveIV)
                {
                    await EvolveAllPokemonWithEnoughCandy(_clientSettings.PokemonsToEvolve);
                }
                if (_clientSettings.TransferDuplicatePokemon)
                {
                    await TransferDuplicatePokemon(false);
                }
                _stats.UpdateConsoleTitle(_inventory);
                await RecycleItems();
                await ExecuteFarmingPokestopsAndPokemons(_clientSettings.UseGPXPathing);
                await Task.Delay(10000);
            }
        }

        private async Task RecycleItems()
        {
            foreach (var itemData in await _inventory.GetItemsToRecycle())
            {
                var item = itemData;
                var inventoryItemResponse = await _client.Inventory.RecycleItem(item.ItemId, item.Count);
                Logger.Write(string.Format("Recycled {0}x {1}", item.Count, item.ItemId), LogLevel.Recycling,
                    ConsoleColor.Black);
                _stats.AddItemsRemoved(item.Count);
                _stats.UpdateConsoleTitle(_inventory);
                await Task.Delay(random.Next(1000, 5000));
                item = null;
            }
        }

        public async Task RepeatAction(int repeat, Func<Task> action)
        {
            for (var i = 0; i < repeat; ++i)
            {
                await action();
            }
        }

        private async Task TransferDuplicatePokemon(bool keepPokemonsThatCanEvolve = false)
        {
            var didntTransferPokemon = false;
            var pokemonToTransfer =
                await
                    _inventory.GetDuplicatePokemonToTransfer(_client.Settings.PokemonsToTransfer,
                        _client.Settings.PokemonsToEvolve, false, _client.Settings.PrioritizeIVOverCP);
            Logger.Write(string.Format("Found {0} Pokemon to transfer", pokemonToTransfer.Count()), LogLevel.Info,
                ConsoleColor.Black);
            foreach (var pokemonData1 in pokemonToTransfer)
            {
                var duplicatePokemon = pokemonData1;
                if (duplicatePokemon.Cp >= _clientSettings.KeepMinCP ||
                    PokemonInfo.CalculatePokemonPerfection(duplicatePokemon) > _clientSettings.KeepMinIVPercentage)
                {
                    didntTransferPokemon = true;
                }
                else
                {
                    var releasePokemonResponse = await _client.Inventory.TransferPokemon(duplicatePokemon.Id);
                    await _inventory.DeletePokemonFromInvById(duplicatePokemon.Id);
                    _stats.IncreasePokemonsTransfered();
                    _stats.UpdateConsoleTitle(_inventory);
                    PokemonData pokemonData2;
                    if (_client.Settings.PrioritizeIVOverCP)
                    {
                        pokemonData2 = await _inventory.GetHighestPokemonOfTypeByIv(duplicatePokemon);
                    }
                    else
                    {
                        pokemonData2 = await _inventory.GetHighestPokemonOfTypeByCp(duplicatePokemon);
                    }
                    var poke = pokemonData2;
                    var format = "Transferred {0} with {1} ({2} % perfect) CP (Best: {3} | ({4} % perfect))";
                    var objArray = new object[5];
                    objArray[0] = duplicatePokemon.PokemonId;
                    objArray[1] = duplicatePokemon.Cp;
                    var index1 = 2;
                    var pokemonPerfection = PokemonInfo.CalculatePokemonPerfection(duplicatePokemon);
                    var string1 = pokemonPerfection.ToString("0.00");
                    objArray[index1] = string1;
                    var index2 = 3;
                    // ISSUE: variable of a boxed type
                    var local = (ValueType) poke.Cp;
                    objArray[index2] = local;
                    var index3 = 4;
                    pokemonPerfection = PokemonInfo.CalculatePokemonPerfection(poke);
                    var string2 = pokemonPerfection.ToString("0.00");
                    objArray[index3] = string2;
                    Logger.Write(string.Format(format, objArray), LogLevel.Transfer, ConsoleColor.Black);
                    await Task.Delay(random.Next(1000, 5000));
                    duplicatePokemon = null;
                }
            }

            if (!didntTransferPokemon)
            {
                return;
            }

            Logger.Write("There were Pokemon that weren't transferred due to your configuration settings", LogLevel.Info,
                ConsoleColor.Black);
        }

        public async Task UseBerry(ulong encounterId, string spawnPointId)
        {
            var berry = (await _inventory.GetItems()).Where(p => p.ItemId == ItemId.ItemRazzBerry).FirstOrDefault();
            if (berry == null || berry.Count <= 0)
            {
                return;
            }

            var itemCaptureResponse =
                await _client.Encounter.UseCaptureItem(encounterId, ItemId.ItemRazzBerry, spawnPointId);
            --berry.Count;
            Logger.Write(string.Format("Used Razz Berry, remaining: {0}", berry.Count), LogLevel.Berry,
                ConsoleColor.Black);
            await Task.Delay(random.Next(3000, 5000));
        }

        public async Task UseLuckyEgg(Client client)
        {
            var luckyEgg = (await _inventory.GetItems()).Where(p => p.ItemId == ItemId.ItemLuckyEgg).FirstOrDefault();
            if (luckyEgg == null || luckyEgg.Count <= 0)
            {
                return;
            }

            var itemXpBoostResponse = await _client.Inventory.UseItemXpBoost();
            Logger.Write(string.Format("Used Lucky Egg, remaining: {0}", luckyEgg.Count - 1), LogLevel.Egg,
                ConsoleColor.Black);
            await Task.Delay(random.Next(3000, 5000));
        }

        #endregion
    }
}