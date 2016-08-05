namespace PGB.WPF.Views.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    using PokemonGo.RocketAPI.Enums;

    internal class AuthTypeEnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (AuthType) Enum.Parse(typeof(AuthType), value.ToString(), true);
        }
    }
}