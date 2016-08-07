namespace PGB.Logic.Utils
{
    public struct BaseStats
    {
        public int BaseAttack;
        public int BaseDefense;
        public int BaseStamina;

        public BaseStats(int baseStamina, int baseAttack, int baseDefense)
        {
            BaseAttack = baseAttack;
            BaseDefense = baseDefense;
            BaseStamina = baseStamina;
        }

        public override string ToString()
        {
            return $"({BaseAttack} atk,{BaseDefense} def,{BaseStamina} sta)";
        }
    }
}