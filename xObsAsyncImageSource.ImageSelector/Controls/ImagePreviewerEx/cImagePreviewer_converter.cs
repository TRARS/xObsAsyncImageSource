using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace xObsAsyncImageSource.ImageSelector.Controls.ImagePreviewerEx
{
    public class cImagePreviewer_converter_borderbrush : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool flag)
            {
                try { return values[1]; }
                catch { }
                return new SolidColorBrush(Colors.Red);
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class cImagePreviewer_converter_borderthickness : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? 1 : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class cImagePreviewer_converter_imagesource : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imagePath = $"{value}";
            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                try
                {
                    if (Path.IsPathRooted(imagePath))
                    {
                        // 尝试将路径转换为 ImageSource 对象
                        var imageSource = new BitmapImage(new Uri(imagePath));
                        return imageSource;
                    }
                    else
                    {
                        // 尝试将路径转换为 ImageSource 对象
                        string packUri = $"pack://application:,,,/{imagePath}";
                        // 尝试将 Pack URI 转换为 ImageSource 对象
                        var imageSource = new BitmapImage(new Uri(packUri));
                        return imageSource;
                    }
                }
                catch { }
            }

            return new WriteableBitmap(32, 32, 96, 96, PixelFormats.Bgra32, null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
