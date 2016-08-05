using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGB.WPF.Views.Converters
{
    using System.Globalization;
    using System.Windows.Data;

    internal class BooleanConverter<T> : IValueConverter
    {
        public T False
        {
            get;
            set;
        }

        public T True
        {
            get;
            set;
        }

        public BooleanConverter(T trueValue, T falseValue)
        {
            this.True = trueValue;
            this.False = falseValue;
        }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (!(value is bool) || !(bool)value ? this.False : this.True);
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (!(value is T) ? false : EqualityComparer<T>.Default.Equals((T)value, this.True));
        }
    }
}
