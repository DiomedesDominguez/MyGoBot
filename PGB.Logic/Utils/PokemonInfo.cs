namespace PGB.Logic.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using POGOProtos.Data;
    using POGOProtos.Enums;
    using POGOProtos.Inventory;
    using POGOProtos.Settings.Master;

    public static class PokemonInfo
    {
        #region Methods and other members

        public static int CalculateCp(PokemonData poke)
        {
            return
                Math.Max(
                    (int)
                        Math.Floor(0.1*CalculateCpMultiplier(poke)*
                                   Math.Pow(poke.CpMultiplier + (double) poke.AdditionalCpMultiplier, 2.0)), 10);
        }

        public static double CalculateCpMultiplier(PokemonData poke)
        {
            var baseStats = GetBaseStats(poke.PokemonId);
            return (baseStats.BaseAttack + poke.IndividualAttack)*
                   Math.Sqrt(baseStats.BaseDefense + poke.IndividualDefense)*
                   Math.Sqrt(baseStats.BaseStamina + poke.IndividualStamina);
        }

        public static int CalculateMaxCp(PokemonData poke)
        {
            return
                Math.Max(
                    (int)
                        Math.Floor(0.1*CalculateMaxCpMultiplier(poke.PokemonId)*
                                   Math.Pow(poke.CpMultiplier + (double) poke.AdditionalCpMultiplier, 2.0)), 10);
        }

        public static double CalculateMaxCpMultiplier(PokemonId pokemonId)
        {
            var baseStats = GetBaseStats(pokemonId);
            return (baseStats.BaseAttack + 15)*Math.Sqrt(baseStats.BaseDefense + 15)*
                   Math.Sqrt(baseStats.BaseStamina + 15);
        }

        public static int CalculateMinCp(PokemonData poke)
        {
            return
                Math.Max(
                    (int)
                        Math.Floor(0.1*CalculateMinCpMultiplier(poke)*
                                   Math.Pow(poke.CpMultiplier + (double) poke.AdditionalCpMultiplier, 2.0)), 10);
        }

        public static double CalculateMinCpMultiplier(PokemonData poke)
        {
            var baseStats = GetBaseStats(poke.PokemonId);
            return baseStats.BaseAttack*Math.Sqrt(baseStats.BaseDefense)*Math.Sqrt(baseStats.BaseStamina);
        }

        public static double CalculatePokemonPerfection(PokemonData poke)
        {
            if (Math.Abs(poke.CpMultiplier + poke.AdditionalCpMultiplier) <= 0.0)
            {
                return (poke.IndividualAttack + poke.IndividualDefense + poke.IndividualStamina)/45.0*100.0;
            }

            var maxCpMultiplier = CalculateMaxCpMultiplier(poke.PokemonId);
            var minCpMultiplier = CalculateMinCpMultiplier(poke);
            return (CalculateCpMultiplier(poke) - minCpMultiplier)/(maxCpMultiplier - minCpMultiplier)*100.0;
        }

        public static BaseStats GetBaseStats(PokemonId id)
        {
            switch (id)
            {
                case PokemonId.Bulbasaur:
                    return new BaseStats(90, 126, 126);
                case PokemonId.Ivysaur:
                    return new BaseStats(120, 156, 158);
                case PokemonId.Venusaur:
                    return new BaseStats(160, 198, 200);
                case PokemonId.Charmander:
                    return new BaseStats(78, 128, 108);
                case PokemonId.Charmeleon:
                    return new BaseStats(116, 160, 140);
                case PokemonId.Charizard:
                    return new BaseStats(156, 212, 182);
                case PokemonId.Squirtle:
                    return new BaseStats(88, 112, 142);
                case PokemonId.Wartortle:
                    return new BaseStats(118, 144, 176);
                case PokemonId.Blastoise:
                    return new BaseStats(158, 186, 222);
                case PokemonId.Caterpie:
                    return new BaseStats(90, 62, 66);
                case PokemonId.Metapod:
                    return new BaseStats(100, 56, 86);
                case PokemonId.Butterfree:
                    return new BaseStats(120, 144, 144);
                case PokemonId.Weedle:
                    return new BaseStats(80, 68, 64);
                case PokemonId.Kakuna:
                    return new BaseStats(90, 62, 82);
                case PokemonId.Beedrill:
                    return new BaseStats(130, 144, 130);
                case PokemonId.Pidgey:
                    return new BaseStats(80, 94, 90);
                case PokemonId.Pidgeotto:
                    return new BaseStats(126, 126, 122);
                case PokemonId.Pidgeot:
                    return new BaseStats(166, 170, 166);
                case PokemonId.Rattata:
                    return new BaseStats(60, 92, 86);
                case PokemonId.Raticate:
                    return new BaseStats(110, 146, 150);
                case PokemonId.Spearow:
                    return new BaseStats(80, 102, 78);
                case PokemonId.Fearow:
                    return new BaseStats(130, 168, 146);
                case PokemonId.Ekans:
                    return new BaseStats(70, 112, 112);
                case PokemonId.Arbok:
                    return new BaseStats(120, 166, 166);
                case PokemonId.Pikachu:
                    return new BaseStats(70, 124, 108);
                case PokemonId.Raichu:
                    return new BaseStats(120, 200, 154);
                case PokemonId.Sandshrew:
                    return new BaseStats(100, 90, 114);
                case PokemonId.Sandslash:
                    return new BaseStats(150, 150, 172);
                case PokemonId.NidoranFemale:
                    return new BaseStats(110, 100, 104);
                case PokemonId.Nidorina:
                    return new BaseStats(140, 132, 136);
                case PokemonId.Nidoqueen:
                    return new BaseStats(180, 184, 190);
                case PokemonId.NidoranMale:
                    return new BaseStats(92, 110, 94);
                case PokemonId.Nidorino:
                    return new BaseStats(122, 142, 128);
                case PokemonId.Nidoking:
                    return new BaseStats(162, 204, 170);
                case PokemonId.Clefairy:
                    return new BaseStats(140, 116, 124);
                case PokemonId.Clefable:
                    return new BaseStats(190, 178, 178);
                case PokemonId.Vulpix:
                    return new BaseStats(76, 106, 118);
                case PokemonId.Ninetales:
                    return new BaseStats(146, 176, 194);
                case PokemonId.Jigglypuff:
                    return new BaseStats(230, 98, 54);
                case PokemonId.Wigglytuff:
                    return new BaseStats(280, 168, 108);
                case PokemonId.Zubat:
                    return new BaseStats(80, 88, 90);
                case PokemonId.Golbat:
                    return new BaseStats(150, 164, 164);
                case PokemonId.Oddish:
                    return new BaseStats(90, 134, 130);
                case PokemonId.Gloom:
                    return new BaseStats(120, 162, 158);
                case PokemonId.Vileplume:
                    return new BaseStats(150, 202, 190);
                case PokemonId.Paras:
                    return new BaseStats(70, 122, 120);
                case PokemonId.Parasect:
                    return new BaseStats(120, 162, 170);
                case PokemonId.Venonat:
                    return new BaseStats(120, 108, 118);
                case PokemonId.Venomoth:
                    return new BaseStats(140, 172, 154);
                case PokemonId.Diglett:
                    return new BaseStats(20, 108, 86);
                case PokemonId.Dugtrio:
                    return new BaseStats(70, 148, 140);
                case PokemonId.Meowth:
                    return new BaseStats(80, 104, 94);
                case PokemonId.Persian:
                    return new BaseStats(130, 156, 146);
                case PokemonId.Psyduck:
                    return new BaseStats(100, 132, 112);
                case PokemonId.Golduck:
                    return new BaseStats(160, 194, 176);
                case PokemonId.Mankey:
                    return new BaseStats(80, 122, 96);
                case PokemonId.Primeape:
                    return new BaseStats(130, 178, 150);
                case PokemonId.Growlithe:
                    return new BaseStats(110, 156, 110);
                case PokemonId.Arcanine:
                    return new BaseStats(180, 230, 180);
                case PokemonId.Poliwag:
                    return new BaseStats(80, 108, 98);
                case PokemonId.Poliwhirl:
                    return new BaseStats(130, 132, 132);
                case PokemonId.Poliwrath:
                    return new BaseStats(180, 180, 202);
                case PokemonId.Abra:
                    return new BaseStats(50, 110, 76);
                case PokemonId.Kadabra:
                    return new BaseStats(80, 150, 112);
                case PokemonId.Alakazam:
                    return new BaseStats(110, 186, 152);
                case PokemonId.Machop:
                    return new BaseStats(140, 118, 96);
                case PokemonId.Machoke:
                    return new BaseStats(160, 154, 144);
                case PokemonId.Machamp:
                    return new BaseStats(180, 198, 180);
                case PokemonId.Bellsprout:
                    return new BaseStats(100, 158, 78);
                case PokemonId.Weepinbell:
                    return new BaseStats(130, 190, 110);
                case PokemonId.Victreebel:
                    return new BaseStats(160, 222, 152);
                case PokemonId.Tentacool:
                    return new BaseStats(80, 106, 136);
                case PokemonId.Tentacruel:
                    return new BaseStats(160, 170, 196);
                case PokemonId.Geodude:
                    return new BaseStats(80, 106, 118);
                case PokemonId.Graveler:
                    return new BaseStats(110, 142, 156);
                case PokemonId.Golem:
                    return new BaseStats(160, 176, 198);
                case PokemonId.Ponyta:
                    return new BaseStats(100, 168, 138);
                case PokemonId.Rapidash:
                    return new BaseStats(130, 200, 170);
                case PokemonId.Slowpoke:
                    return new BaseStats(180, 110, 110);
                case PokemonId.Slowbro:
                    return new BaseStats(190, 184, 198);
                case PokemonId.Magnemite:
                    return new BaseStats(50, 128, 138);
                case PokemonId.Magneton:
                    return new BaseStats(100, 186, 180);
                case PokemonId.Farfetchd:
                    return new BaseStats(104, 138, 132);
                case PokemonId.Doduo:
                    return new BaseStats(70, 126, 96);
                case PokemonId.Dodrio:
                    return new BaseStats(120, 182, 150);
                case PokemonId.Seel:
                    return new BaseStats(130, 104, 138);
                case PokemonId.Dewgong:
                    return new BaseStats(180, 156, 192);
                case PokemonId.Grimer:
                    return new BaseStats(160, 124, 110);
                case PokemonId.Muk:
                    return new BaseStats(210, 180, 188);
                case PokemonId.Shellder:
                    return new BaseStats(60, 120, 112);
                case PokemonId.Cloyster:
                    return new BaseStats(100, 196, 196);
                case PokemonId.Gastly:
                    return new BaseStats(60, 136, 82);
                case PokemonId.Haunter:
                    return new BaseStats(90, 172, 118);
                case PokemonId.Gengar:
                    return new BaseStats(120, 204, 156);
                case PokemonId.Onix:
                    return new BaseStats(70, 90, 186);
                case PokemonId.Drowzee:
                    return new BaseStats(120, 104, 140);
                case PokemonId.Hypno:
                    return new BaseStats(170, 162, 196);
                case PokemonId.Krabby:
                    return new BaseStats(60, 116, 110);
                case PokemonId.Kingler:
                    return new BaseStats(110, 178, 168);
                case PokemonId.Voltorb:
                    return new BaseStats(80, 102, 124);
                case PokemonId.Electrode:
                    return new BaseStats(120, 150, 174);
                case PokemonId.Exeggcute:
                    return new BaseStats(120, 110, 132);
                case PokemonId.Exeggutor:
                    return new BaseStats(190, 232, 164);
                case PokemonId.Cubone:
                    return new BaseStats(100, 102, 150);
                case PokemonId.Marowak:
                    return new BaseStats(120, 140, 202);
                case PokemonId.Hitmonlee:
                    return new BaseStats(100, 148, 172);
                case PokemonId.Hitmonchan:
                    return new BaseStats(100, 138, 204);
                case PokemonId.Lickitung:
                    return new BaseStats(180, 126, 160);
                case PokemonId.Koffing:
                    return new BaseStats(80, 136, 142);
                case PokemonId.Weezing:
                    return new BaseStats(130, 190, 198);
                case PokemonId.Rhyhorn:
                    return new BaseStats(160, 110, 116);
                case PokemonId.Rhydon:
                    return new BaseStats(210, 166, 160);
                case PokemonId.Chansey:
                    return new BaseStats(500, 40, 60);
                case PokemonId.Tangela:
                    return new BaseStats(130, 164, 152);
                case PokemonId.Kangaskhan:
                    return new BaseStats(210, 142, 178);
                case PokemonId.Horsea:
                    return new BaseStats(60, 122, 100);
                case PokemonId.Seadra:
                    return new BaseStats(110, 176, 150);
                case PokemonId.Goldeen:
                    return new BaseStats(90, 112, 126);
                case PokemonId.Seaking:
                    return new BaseStats(160, 172, 160);
                case PokemonId.Staryu:
                    return new BaseStats(60, 130, 128);
                case PokemonId.Starmie:
                    return new BaseStats(120, 194, 192);
                case PokemonId.MrMime:
                    return new BaseStats(80, 154, 196);
                case PokemonId.Scyther:
                    return new BaseStats(140, 176, 180);
                case PokemonId.Jynx:
                    return new BaseStats(130, 172, 134);
                case PokemonId.Electabuzz:
                    return new BaseStats(130, 198, 160);
                case PokemonId.Magmar:
                    return new BaseStats(130, 214, 158);
                case PokemonId.Pinsir:
                    return new BaseStats(130, 184, 186);
                case PokemonId.Tauros:
                    return new BaseStats(150, 148, 184);
                case PokemonId.Magikarp:
                    return new BaseStats(40, 42, 84);
                case PokemonId.Gyarados:
                    return new BaseStats(190, 192, 196);
                case PokemonId.Lapras:
                    return new BaseStats(260, 186, 190);
                case PokemonId.Ditto:
                    return new BaseStats(96, 110, 110);
                case PokemonId.Eevee:
                    return new BaseStats(110, 114, 128);
                case PokemonId.Vaporeon:
                    return new BaseStats(260, 186, 168);
                case PokemonId.Jolteon:
                    return new BaseStats(130, 192, 174);
                case PokemonId.Flareon:
                    return new BaseStats(130, 238, 178);
                case PokemonId.Porygon:
                    return new BaseStats(130, 156, 158);
                case PokemonId.Omanyte:
                    return new BaseStats(70, 132, 160);
                case PokemonId.Omastar:
                    return new BaseStats(140, 180, 202);
                case PokemonId.Kabuto:
                    return new BaseStats(60, 148, 142);
                case PokemonId.Kabutops:
                    return new BaseStats(120, 190, 190);
                case PokemonId.Aerodactyl:
                    return new BaseStats(160, 182, 162);
                case PokemonId.Snorlax:
                    return new BaseStats(320, 180, 180);
                case PokemonId.Articuno:
                    return new BaseStats(180, 198, 242);
                case PokemonId.Zapdos:
                    return new BaseStats(180, 232, 194);
                case PokemonId.Moltres:
                    return new BaseStats(180, 242, 194);
                case PokemonId.Dratini:
                    return new BaseStats(82, 128, 110);
                case PokemonId.Dragonair:
                    return new BaseStats(122, 170, 152);
                case PokemonId.Dragonite:
                    return new BaseStats(182, 250, 212);
                case PokemonId.Mewtwo:
                    return new BaseStats(212, 284, 202);
                case PokemonId.Mew:
                    return new BaseStats(200, 220, 220);
                case PokemonId.Missingno:
                default:
                    return new BaseStats();
            }
        }

        public static int GetCandy(PokemonData pokemon, List<Candy> PokemonFamilies,
            IEnumerable<PokemonSettings> PokemonSettings)
        {
            var setting = PokemonSettings.FirstOrDefault(q =>
            {
                if (pokemon != null)
                {
                    return q.PokemonId.Equals(pokemon.PokemonId);
                }

                return false;
            });
            return PokemonFamilies.FirstOrDefault(q =>
            {
                if (setting != null)
                {
                    return q.FamilyId.Equals(setting.FamilyId);
                }

                return false;
            }).Candy_;
        }

        public static double GetLevel(PokemonData poke)
        {
            switch ((int) (((double) poke.CpMultiplier + (double) poke.AdditionalCpMultiplier)*1000.0))
            {
                case 784:
                    return 39.0;
                case 787:
                    return 39.5;
                case 790:
                    return 40.0;
                case 778:
                    return 38.0;
                case 781:
                    return 38.5;
                case 770:
                    return 36.5;
                case 773:
                    return 37.0;
                case 776:
                    return 37.5;
                case 764:
                    return 35.5;
                case 767:
                    return 36.0;
                case 755:
                    return 34.0;
                case 758:
                    return 34.5;
                case 761:
                    return 35.0;
                case 749:
                    return 33.0;
                case 752:
                    return 33.5;
                case 740:
                    return 31.5;
                case 743:
                    return 32.0;
                case 746:
                    return 32.5;
                case 734:
                    return 35.0;
                case 737:
                    return 31.0;
                case 719:
                    return 29.0;
                case 725:
                    return 29.5;
                case 731:
                    return 30.0;
                case 706:
                    return 28.0;
                case 713:
                    return 28.5;
                case 687:
                    return 26.5;
                case 694:
                    return 27.0;
                case 700:
                    return 27.5;
                case 674:
                    return 25.5;
                case 681:
                    return 26.0;
                case 654:
                    return 24.0;
                case 661:
                    return 24.5;
                case 667:
                    return 25.0;
                case 640:
                    return 23.0;
                case 647:
                    return 23.5;
                case 619:
                    return 21.5;
                case 626:
                    return 22.0;
                case 633:
                    return 22.5;
                case 604:
                    return 25.0;
                case 612:
                    return 21.0;
                case 582:
                    return 19.0;
                case 589:
                    return 19.5;
                case 597:
                    return 20.0;
                case 566:
                    return 18.0;
                case 574:
                    return 18.5;
                case 542:
                    return 16.5;
                case 550:
                    return 17.0;
                case 558:
                    return 17.5;
                case 525:
                    return 15.5;
                case 534:
                    return 16.0;
                case 499:
                    return 14.0;
                case 508:
                    return 14.5;
                case 517:
                    return 15.0;
                case 481:
                    return 13.0;
                case 490:
                    return 13.5;
                case 453:
                    return 11.5;
                case 462:
                    return 12.0;
                case 472:
                    return 12.5;
                case 432:
                    return 15.0;
                case 443:
                    return 11.0;
                case 399:
                    return 9.0;
                case 411:
                    return 9.5;
                case 422:
                    return 10.0;
                case 375:
                    return 8.0;
                case 387:
                    return 8.5;
                case 335:
                    return 6.5;
                case 349:
                    return 7.0;
                case 362:
                    return 7.5;
                case 306:
                    return 5.5;
                case 321:
                    return 6.0;
                case byte.MaxValue:
                    return 4.0;
                case 273:
                    return 4.5;
                case 290:
                    return 5.0;
                case 215:
                    return 3.0;
                case 236:
                    return 3.5;
                case 135:
                    return 1.5;
                case 166:
                    return 2.0;
                case 192:
                    return 2.5;
                case 93:
                case 94:
                    return 1.0;
                default:
                    return 0.0;
            }
        }

        public static PokemonMove GetPokemonMove1(PokemonData poke)
        {
            return poke.Move1;
        }

        public static PokemonMove GetPokemonMove2(PokemonData poke)
        {
            return poke.Move2;
        }

        public static int GetPowerUpLevel(PokemonData poke)
        {
            return (int) (GetLevel(poke)*2.0);
        }

        #endregion
    }
}