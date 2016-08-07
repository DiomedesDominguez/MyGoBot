using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGB.Logic.Utils
{
    using System.Globalization;
    

    using POGOProtos.Data.Player;
    using POGOProtos.Networking.Responses;

    public class Statistics
    {
        private int currentlevel = -1;
        public DateTime initSessionDateTime = DateTime.Now;
        private int totalExperience;
        private int totalPokemons;
        private int totalItemsRemoved;
        private int totalPokemonsTransfered;
        private int totalStardust;
        private string currentLevelInfos;
        private string playerName;

        public int TotalExperience
        {
            get; set; }

        public int TotalPokemons
        {
            get; set; }

        public int TotalItemsRemoved
        {
            get; set; }

        public int TotalPokemonsTransfered
        {
            get; set; }

        public int TotalStardust
        {
            get; set; }

        public string CurrentLevelInfos
        {
            get; set; }

        public int Currentlevel
        {
            get; set; }

        public string PlayerName
        {
            get; set; }

        public DateTime InitSessionDateTime
        {
            get; set; }

        public TimeSpan Duration
        {
            get
            {
                return DateTime.Now - this.InitSessionDateTime;
            }
        }

        public string Runtime
        {
            get
            {
                return this._getSessionRuntimeInTimeFormat();
            }
            set
            {
                
            }
        }

        public string EXPPerHour
        {
            get
            {
                return ((double)this.TotalExperience / this._getSessionRuntime()).ToString("N0");
            }
            set
            {
            }
        }

        public string PokemonPerHour
        {
            get
            {
                return ((double)this.TotalPokemons / this._getSessionRuntime()).ToString("N0");
            }
            set
            {
               
            }
        }

        public async Task<string> _getcurrentLevelInfos(Inventory inventory)
        {
            IEnumerable<PlayerStats> playerStats1 = await inventory.GetPlayerStats();
            string str = string.Empty;
            PlayerStats playerStats2 = playerStats1 != null ? playerStats1.FirstOrDefault<PlayerStats>() : (PlayerStats)null;
            if (playerStats2 != null)
            {
                double d = Math.Round((double)(playerStats2.NextLevelXp - playerStats2.PrevLevelXp - (playerStats2.Experience - playerStats2.PrevLevelXp)) / ((double)this.TotalExperience / this._getSessionRuntime()), 2);
                double num1 = 0.0;
                double num2 = 0.0;
                if (!double.IsInfinity(d) && d > 0.0)
                {
                    double @double = Convert.ToDouble(TimeSpan.FromHours(d).ToString("h\\.mm"), (IFormatProvider)CultureInfo.InvariantCulture);
                    num1 = Math.Truncate(@double);
                    num2 = Math.Round((@double - num1) * 100.0);
                }
                str = string.Format("{0} (next level in {1}h {2}m | {3}/{4} XP)", (object)playerStats2.Level, (object)num1, (object)num2, (object)(playerStats2.Experience - playerStats2.PrevLevelXp - (long)this.GetXpDiff(playerStats2.Level)), (object)(playerStats2.NextLevelXp - playerStats2.PrevLevelXp - (long)this.GetXpDiff(playerStats2.Level)));
            }
            this.Runtime = "";
            return str;
        }

        public double _getSessionRuntime()
        {
            return (DateTime.Now - this.InitSessionDateTime).TotalSeconds / 3600.0;
        }

        public string _getSessionRuntimeInTimeFormat()
        {
            return (DateTime.Now - this.InitSessionDateTime).ToString("dd\\.hh\\:mm\\:ss");
        }

        public void AddExperience(int xp)
        {
            this.TotalExperience = this.TotalExperience + xp;
        }

        public void AddItemsRemoved(int count)
        {
            this.TotalItemsRemoved = this.TotalItemsRemoved + count;
        }

        public void GetStardust(int stardust)
        {
            this.TotalStardust = stardust;
        }

        public int GetXpDiff(int level)
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

        public void IncreasePokemons()
        {
            this.TotalPokemons = this.TotalPokemons + 1;
        }

        public void IncreasePokemonsTransfered()
        {
            this.TotalPokemonsTransfered = this.TotalPokemonsTransfered + 1;
        }

        public void SetUsername(GetPlayerResponse profile)
        {
            this.PlayerName = profile.PlayerData.Username ?? "";
        }

        public override string ToString()
        {
            return
                $"{(object) this.PlayerName} - Runtime {(object) this._getSessionRuntimeInTimeFormat()} - Lvl: {(object) this.CurrentLevelInfos} | EXP/H: {(object) ((double) this.TotalExperience/this._getSessionRuntime()):0} | P/H: {(object) ((double) this.TotalPokemons/this._getSessionRuntime()):0} | Stardust: {(object) this.TotalStardust:0} | Transfered: {(object) this.TotalPokemonsTransfered:0} | Items Recycled: {(object) this.TotalItemsRemoved:0}";
        }

        public async void UpdateConsoleTitle(Inventory inventory)
        {
            try
            {
                this.CurrentLevelInfos = await this._getcurrentLevelInfos(inventory);
            }
            catch
            {
            }
        }
    }
}
