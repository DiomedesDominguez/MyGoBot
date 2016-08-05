namespace PGB.WPF.Internals
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    internal class GeneralSettings
    {
        public double WindowHeight;
        public double WindowLeft;

        public double WindowTop;

        public double WindowWidth;
    }
}