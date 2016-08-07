namespace PGB.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using PokemonGo.RocketAPI;

    using POGOProtos.Data;
    using POGOProtos.Data.Player;
    using POGOProtos.Enums;
    using POGOProtos.Inventory;
    using POGOProtos.Inventory.Item;
    using POGOProtos.Networking.Responses;
    using POGOProtos.Settings.Master;

    using Utils;

    public class Inventory
    {
        #region Constructors

        public Inventory(Client client)
        {
            _client = client;
        }

        #endregion

        #region Fields, properties, indexers and constants

        private readonly Client _client;

        private readonly List<ItemId> _pokeballs = new List<ItemId>
        {
            ItemId.ItemPokeBall,
            ItemId.ItemGreatBall,
            ItemId.ItemUltraBall,
            ItemId.ItemMasterBall
        };

        private readonly List<ItemId> _potions = new List<ItemId>
        {
            ItemId.ItemPotion,
            ItemId.ItemSuperPotion,
            ItemId.ItemHyperPotion,
            ItemId.ItemMaxPotion
        };

        private readonly List<ItemId> _revives = new List<ItemId>
        {
            ItemId.ItemRevive,
            ItemId.ItemMaxRevive
        };

        private GetInventoryResponse _cachedInventory;
        private DateTime _lastRefresh;

        #endregion

        #region Methods and other members

        public async Task DeletePokemonFromInvById(ulong id)
        {
            var cachedInventory = await GetCachedInventory();
            var inventoryItem = cachedInventory.InventoryDelta.InventoryItems.FirstOrDefault(i =>
            {
                if (i.InventoryItemData.PokemonData != null)
                {
                    return (long) i.InventoryItemData.PokemonData.Id == (long) id;
                }

                return false;
            });
            if (inventoryItem == null)
            {
                return;
            }

            cachedInventory.InventoryDelta.InventoryItems.Remove(inventoryItem);
        }

        private async Task<GetInventoryResponse> GetCachedInventory()
        {
            if (_lastRefresh.AddSeconds(30.0).Ticks > DateTime.UtcNow.Ticks)
            {
                return _cachedInventory;
            }

            return await RefreshCachedInventory();
        }

        public async Task<IEnumerable<PokemonData>> GetDuplicatePokemonToTransfer(
            IEnumerable<PokemonId> pokemonsToTransferArg, IEnumerable<PokemonId> pokemonsToEvolve,
            bool keepPokemonsThatCanEvolve = false, bool prioritizeIVoverCp = false)
        {
            var myPokemonList = (await GetPokemons()).ToList();
            var pokemonToTransfer = myPokemonList.Where(p =>
            {
                if (pokemonsToTransferArg.Contains(p.PokemonId) && p.DeployedFortId == string.Empty)
                {
                    return p.Favorite == 0;
                }

                return false;
            }).ToList();
            pokemonToTransfer = pokemonToTransfer.Where(p =>
            {
                var pokemonTransferFilter = GetPokemonTransferFilter(p.PokemonId);
                if (p.Cp >= pokemonTransferFilter.KeepMinCp &&
                    PokemonInfo.CalculatePokemonPerfection(p) >= pokemonTransferFilter.KeepMinIvPercentage)
                {
                    return false;
                }

                return !pokemonTransferFilter.Moves.Intersect(new PokemonMove[2]
                {
                    p.Move1,
                    p.Move2
                }).Any();
            }).ToList();
            var pokemonSettings = (await GetPokemonSettings()).ToList();
            var array = (await GetPokemonFamilies()).ToArray();
            var pokemonDataList = new List<PokemonData>();
            foreach (var grouping in pokemonToTransfer.GroupBy(p => p.PokemonId).ToList())
            {
                var pokemonGroupToTransfer = grouping;
                var val1_1 = Math.Max(GetPokemonTransferFilter(pokemonGroupToTransfer.Key).KeepMinDuplicatePokemon, 0);
                var settings = pokemonSettings.Single(x => x.PokemonId == pokemonGroupToTransfer.Key);
                var candy = array.Single(x => settings.FamilyId == x.FamilyId);
                var nullable = new int?();
                if (keepPokemonsThatCanEvolve && pokemonsToEvolve.Contains(pokemonGroupToTransfer.Key) &&
                    settings.CandyToEvolve > 0 && settings.EvolutionIds.Count != 0)
                {
                    var val2 = candy.Candy_/settings.CandyToEvolve;
                    nullable = candy.Candy_%settings.CandyToEvolve;
                    val1_1 = Math.Max(val1_1, val2);
                }
                var num1 = myPokemonList.Count(data => data.PokemonId == pokemonGroupToTransfer.Key);
                var val2_1 = pokemonGroupToTransfer.Count();
                var num2 = val1_1;
                var val1_2 = num1 - num2;
                if (val1_2 > 0)
                {
                    var num3 = Math.Min(val1_2, val2_1);
                    if (num3 > 0)
                    {
                        if (nullable.HasValue)
                        {
                            num3 = (settings.CandyToEvolve*num3 - nullable.Value)/(1 + settings.CandyToEvolve) +
                                   Math.Sign(settings.CandyToEvolve*num3 - nullable.Value)%(1 + settings.CandyToEvolve);
                        }
                        var count = val2_1 - num3;
                        if (prioritizeIVoverCp)
                        {
                            pokemonDataList.AddRange(
                                pokemonGroupToTransfer.OrderByDescending(PokemonInfo.CalculatePokemonPerfection)
                                    .ThenByDescending(n => n.Cp)
                                    .Skip(count));
                        }
                        else
                        {
                            pokemonDataList.AddRange(
                                pokemonGroupToTransfer.OrderByDescending(x => x.Cp)
                                    .ThenByDescending(PokemonInfo.CalculatePokemonPerfection)
                                    .Skip(count)
                                    .ToList());
                        }
                    }
                }
            }

            return (IEnumerable<PokemonData>) pokemonDataList;
        }

        public async Task<IEnumerable<EggIncubator>> GetEggIncubators()
        {
            return
                (await GetCachedInventory()).InventoryDelta.InventoryItems.Where(
                    x => x.InventoryItemData.EggIncubators != null)
                    .SelectMany(i => (IEnumerable<EggIncubator>) i.InventoryItemData.EggIncubators.EggIncubator)
                    .Where(i => i != null);
        }

        public async Task<IEnumerable<PokemonData>> GetEggs()
        {
            return (await GetCachedInventory()).InventoryDelta.InventoryItems.Select(i =>
            {
                var inventoryItemData = i.InventoryItemData;
                if (inventoryItemData == null)
                {
                    return (PokemonData) null;
                }

                return inventoryItemData.PokemonData;
            }).Where(p =>
            {
                if (p != null)
                {
                    return p.IsEgg;
                }

                return false;
            });
        }

        public async Task<PokemonData> GetHighestPokemonOfTypeByCp(PokemonData pokemon)
        {
            return
                (await GetPokemons()).ToList()
                    .Where(x => x.PokemonId == pokemon.PokemonId)
                    .OrderByDescending(x => x.Cp)
                    .FirstOrDefault();
        }

        public async Task<PokemonData> GetHighestPokemonOfTypeByIv(PokemonData pokemon)
        {
            return
                (await GetPokemons()).ToList()
                    .Where(x => x.PokemonId == pokemon.PokemonId)
                    .OrderByDescending(PokemonInfo.CalculatePokemonPerfection)
                    .FirstOrDefault();
        }

        public async Task<IEnumerable<PokemonData>> GetHighestsCp(int limit)
        {
            return (await GetPokemons()).ToList().OrderByDescending(x => x.Cp).ThenBy(n => n.StaminaMax).Take(limit);
        }

        public async Task<IEnumerable<PokemonData>> GetHighestsPerfect(int limit)
        {
            return (await GetPokemons()).ToList().OrderByDescending(PokemonInfo.CalculatePokemonPerfection).Take(limit);
        }

        public async Task<int> GetItemAmountByType(ItemId type)
        {
            var itemData = (await GetItems()).FirstOrDefault(i => i.ItemId == type);
            return itemData != null ? itemData.Count : 0;
        }

        public async Task<IEnumerable<ItemData>> GetItems()
        {
            return (await GetCachedInventory()).InventoryDelta.InventoryItems.Select(i =>
            {
                var inventoryItemData = i.InventoryItemData;
                if (inventoryItemData == null)
                {
                    return (ItemData) null;
                }

                return inventoryItemData.Item;
            }).Where(p => p != null);
        }

        public async Task<IEnumerable<ItemData>> GetItemsToRecycle()
        {
            var itemsToRecycle = new List<ItemData>();
            var items = await GetItems();
            itemsToRecycle.AddRange(items.ToList().Where(x => _client.Settings.ItemRecycleFilter.Any(f =>
            {
                if (f.Key == x.ItemId)
                {
                    return x.Count > f.Value;
                }

                return false;
            })).Select(x =>
            {
                var itemData = new ItemData();
                itemData.ItemId = x.ItemId;
                itemData.Count = x.Count - _client.Settings.ItemRecycleFilter.Single(f => f.Key == x.ItemId).Value;
                var num = x.Unseen ? 1 : 0;
                itemData.Unseen = num != 0;
                return itemData;
            }));
            return (IEnumerable<ItemData>) itemsToRecycle;
        }

        public async Task<LevelUpRewardsResponse> GetLevelUpRewards(Inventory inv)
        {
            var playerProfile =
                await _client.Player.GetPlayerProfile((await _client.Player.GetPlayer()).PlayerData.Username);
            return await _client.Player.GetLevelUpRewards(inv.GetPlayerStats().Result.FirstOrDefault().Level);
        }

        public double GetPerfect(PokemonData poke)
        {
            return PokemonInfo.CalculatePokemonPerfection(poke);
        }

        public async Task<IEnumerable<PlayerStats>> GetPlayerStats()
        {
            return (await GetCachedInventory()).InventoryDelta.InventoryItems.Select(i =>
            {
                var inventoryItemData = i.InventoryItemData;
                if (inventoryItemData == null)
                {
                    return (PlayerStats) null;
                }

                return inventoryItemData.PlayerStats;
            }).Where(p => p != null);
        }

        public async Task<List<InventoryItem>> GetPokeDexItems()
        {
            var inventoryItemList = new List<InventoryItem>();
            return (await _client.Inventory.GetInventory()).InventoryDelta.InventoryItems.Where(items =>
            {
                var inventoryItemData = items.InventoryItemData;
                return (inventoryItemData != null ? inventoryItemData.PokedexEntry : (PokedexEntry) null) != null;
            }).ToList();
        }

        public async Task<List<Candy>> GetPokemonFamilies()
        {
            return (await GetCachedInventory()).InventoryDelta.InventoryItems.Where(item =>
            {
                var inventoryItemData = item.InventoryItemData;
                return (inventoryItemData != null ? inventoryItemData.Candy : (Candy) null) != null;
            }).Where(item =>
            {
                var inventoryItemData = item.InventoryItemData;
                if (inventoryItemData == null)
                {
                    return true;
                }

                return (uint) inventoryItemData.Candy.FamilyId > 0U;
            }).GroupBy(item =>
            {
                var inventoryItemData = item.InventoryItemData;
                if (inventoryItemData == null)
                {
                    return new PokemonFamilyId?();
                }

                return inventoryItemData.Candy.FamilyId;
            }).Select(family => new Candy
            {
                FamilyId = family.First().InventoryItemData.Candy.FamilyId,
                Candy_ = family.First().InventoryItemData.Candy.Candy_
            }).ToList();
        }

        public async Task<IEnumerable<PokemonData>> GetPokemons()
        {
            return (await GetCachedInventory()).InventoryDelta.InventoryItems.Select(i =>
            {
                var inventoryItemData = i.InventoryItemData;
                if (inventoryItemData == null)
                {
                    return (PokemonData) null;
                }

                return inventoryItemData.PokemonData;
            }).Where(p =>
            {
                if (p != null)
                {
                    return p.PokemonId > PokemonId.Missingno;
                }

                return false;
            });
        }

        public async Task<IEnumerable<PokemonSettings>> GetPokemonSettings()
        {
            return (await _client.Download.GetItemTemplates()).ItemTemplates.Select(i => i.PokemonSettings).Where(p =>
            {
                if (p != null)
                {
                    return (uint) p.FamilyId > 0U;
                }

                return false;
            });
        }

        public async Task<IEnumerable<PokemonData>> GetPokemonToEvolve(IEnumerable<PokemonId> filter = null)
        {
            IEnumerable<PokemonData> source1 =
                (await GetPokemons()).Where(p => p.DeployedFortId == string.Empty).OrderByDescending(p => p.Cp);
            IEnumerable<PokemonId> pokemonIds = filter as PokemonId[] ?? filter.ToArray();
            if (pokemonIds.Any())
            {
                source1 = source1.Where(p =>
                {
                    if (_client.Settings.EvolveAllPokemonWithEnoughCandy)
                    {
                        return pokemonIds.Contains(p.PokemonId);
                    }

                    return false;
                });
            }
            var pokemons = source1.ToList();
            var pokemonSettings = (await GetPokemonSettings()).ToList();
            var array = (await GetPokemonFamilies()).ToArray();
            var source2 = new List<PokemonData>();
            foreach (var pokemonData in pokemons)
            {
                var pokemon = pokemonData;
                var settings = pokemonSettings.SingleOrDefault(x => x.PokemonId == pokemon.PokemonId);
                var candy = array.SingleOrDefault(x => settings.FamilyId == x.FamilyId);
                if (settings.EvolutionIds.Count != 0)
                {
                    var num =
                        source2.Count(
                            p => pokemonSettings.Single(x => x.PokemonId == p.PokemonId).FamilyId == settings.FamilyId)*
                        settings.CandyToEvolve;
                    if (candy.Candy_ - num > settings.CandyToEvolve)
                    {
                        source2.Add(pokemon);
                    }
                }
            }

            return (IEnumerable<PokemonData>) source2;
        }

        public TransferFilter GetPokemonTransferFilter(PokemonId pokemon)
        {
            return new TransferFilter(_client.Settings.KeepMinCP, Convert.ToSingle(_client.Settings.KeepMinIVPercentage),
                _client.Settings.KeepMinDuplicatePokemon, null);
        }

        public async Task<int> GetStarDust()
        {
            return (await _client.Player.GetPlayer()).PlayerData.Currencies[1].Amount;
        }

        public async Task<int> GetTotalItemCount()
        {
            var list = (await GetItems()).ToList();
            var num = 0;
            foreach (var itemData in list)
            {
                num += itemData.Count;
            }

            return num;
        }

        public async Task<GetInventoryResponse> RefreshCachedInventory()
        {
            var now = DateTime.UtcNow;
            var ss = new SemaphoreSlim(10);
            await ss.WaitAsync();
            GetInventoryResponse inventoryResponse1;
            try
            {
                _lastRefresh = now;
                var inventory = this;
                var inventoryResponse2 = inventory._cachedInventory;
                var inventory1 = await _client.Inventory.GetInventory();
                inventory._cachedInventory = inventory1;
                inventory = null;
                inventoryResponse1 = _cachedInventory;
            }
            finally
            {
                ss.Release();
            }
            return inventoryResponse1;
        }

        public async Task<UpgradePokemonResponse> UpgradePokemon(ulong pokemonid)
        {
            return await _client.Inventory.UpgradePokemon(pokemonid);
        }

        public async Task<UseIncenseResponse> UseIncenseConstantly()
        {
            return await _client.Inventory.UseIncense(ItemId.ItemIncenseOrdinary);
        }

        public async Task<UseItemXpBoostResponse> UseLuckyEggConstantly()
        {
            return await _client.Inventory.UseItemXpBoost();
        }

        #endregion
    }
}