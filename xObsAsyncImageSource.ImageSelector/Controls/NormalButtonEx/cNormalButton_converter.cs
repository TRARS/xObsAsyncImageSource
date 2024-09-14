using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace xObsAsyncImageSource.ImageSelector.Controls.NormalButtonEx
{
    internal class cNormalButton_converter_childrencount2textdecorations : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var text = (string)values[0];
            var hasAny = (bool)values[1];
            var mouseover = (bool)values[2];

            return (mouseover && text.Length > 0 && !hasAny) ? TextDecorations.Underline : null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
