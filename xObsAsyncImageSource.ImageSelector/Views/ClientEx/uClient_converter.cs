using System;
using System.Globalization;
using System.Windows.Data;

namespace xObsAsyncImageSource.ImageSelector.Views.ClientEx
{
    class uClient_converter_content2string : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{value}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}