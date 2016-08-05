namespace PGB.WPF.Internals
{
    using System.IO;
    using System.Runtime.Serialization.Formatters;

    using Models;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    internal static class Settings
    {
        private static readonly string GeneralSettingsFileName = ApplicationEnvironment.SettingsDirectory() +
                                                                 "\\GeneralSettings.json";

        private static readonly string MainWindowModelFileName = ApplicationEnvironment.SettingsDirectory() +
                                                                 "\\MainWindowModel.json";

        public static GeneralSettings GeneralSettings { get; private set; }

        public static MainWindowModel MainWindowModel { get; private set; }

        public static void Load()
        {
            GeneralSettings = new GeneralSettings();
            MainWindowModel = new MainWindowModel();
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
            settings.Converters.Add(new StringEnumConverter
            {
                CamelCaseText = true
            });
            if (File.Exists(GeneralSettingsFileName))
            {
                GeneralSettings =
                    JsonConvert.DeserializeObject<GeneralSettings>(File.ReadAllText(GeneralSettingsFileName), settings) ??
                    GeneralSettings;
            }
            if (File.Exists(MainWindowModelFileName))
            {
                MainWindowModel =
                    JsonConvert.DeserializeObject<MainWindowModel>(File.ReadAllText(MainWindowModelFileName), settings) ??
                    MainWindowModel;
            }
            Save();
        }

        public static void Save()
        {
            File.WriteAllText(GeneralSettingsFileName, JsonConvert.SerializeObject(GeneralSettings, Formatting.Indented));
            File.WriteAllText(MainWindowModelFileName, JsonConvert.SerializeObject(MainWindowModel, Formatting.Indented));
        }
    }
}