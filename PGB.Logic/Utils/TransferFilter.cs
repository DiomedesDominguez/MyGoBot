using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGB.Logic.Utils
{
    using POGOProtos.Enums;

    public class TransferFilter
    {
        public int KeepMinCp { get; set; }

        public float KeepMinIvPercentage { get; set; }

        public int KeepMinDuplicatePokemon { get; set; }

        public List<PokemonMove> Moves { get; set; }

        public TransferFilter()
        {
        }

        public TransferFilter(int keepMinCp, float keepMinIvPercentage, int keepMinDuplicatePokemon, List<PokemonMove> moves = null)
        {
            this.KeepMinCp = keepMinCp;
            this.KeepMinIvPercentage = keepMinIvPercentage;
            this.KeepMinDuplicatePokemon = keepMinDuplicatePokemon;
            this.Moves = moves ?? new List<PokemonMove>();
        }
    }
}
