using MahApps.Metro;
using MahApps.Metro.Controls;
using PetraERP.Shared.UI;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PetraERP.Shared
{
    public class UserHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var user = value;
            if (user == null)
                return string.Empty;
            return "Kojo" ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibiltyConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            string reverse = (parameter==null) ? "F" : (string) parameter;

            if (reverse != null)
            {
                if (reverse.Equals("T"))
                {
                    return (((bool)value) == true) ? Visibility.Collapsed : Visibility.Visible;
                }
            }

            return (((bool)value) == true) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return null != value;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ImageToBinaryConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bytes = value as byte[];
            if (bytes != null && bytes.Length > 0)
            {
                var stream = new MemoryStream(bytes);
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
                return image;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var renderTargetBitmap = value as RenderTargetBitmap;
            if (null != renderTargetBitmap)
            {
                var bitmapImage = new BitmapImage();
                var bitmapEncoder = new BmpBitmapEncoder();
                bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                using (var stream = new MemoryStream())
                {
                    bitmapEncoder.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                }
                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                byte[] data = null;
                using (var ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    data = ms.ToArray();
                }
                return data;
            }
            return null;
        }

        #endregion
    }

    public class FlyoutThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var position = (PetraERP.Shared.UI.FlyoutTheme)Enum.Parse(typeof(PetraERP.Shared.UI.FlyoutTheme), System.Convert.ToString(value));
            switch (position)
            {
                case PetraERP.Shared.UI.FlyoutTheme.AccentedTheme:
                    return MahApps.Metro.Controls.FlyoutTheme.Accent;
                case PetraERP.Shared.UI.FlyoutTheme.BaseColorTheme:
                    return MahApps.Metro.Controls.FlyoutTheme.Adapt;
                case PetraERP.Shared.UI.FlyoutTheme.InverseTheme:
                    return MahApps.Metro.Controls.FlyoutTheme.Inverse;
                default:
                    return MahApps.Metro.Controls.FlyoutTheme.Dark;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FlyoutPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var position = (VisibilityPosition)Enum.Parse(typeof(VisibilityPosition), System.Convert.ToString(value));
            switch (position)
            {
                case VisibilityPosition.Bottom:
                    return Position.Bottom;
                case VisibilityPosition.Right:
                    return Position.Right;
                case VisibilityPosition.Left:
                    return Position.Right;
                case VisibilityPosition.Top:
                    return Position.Top;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var purpose = System.Convert.ToString(parameter);
            switch (purpose)
            {
                case "ConvertToAccentColor":
                    var accentColorName = System.Convert.ToString(value);
                    var accent = ThemeManager.Accents.FirstOrDefault(a => string.CompareOrdinal(a.Name, accentColorName) == 0);
                    if (null != accent)
                        return accent.Resources["AccentColorBrush"] as Brush;
                    break;
                case "ConvertToBaseColor":
                    var baseColorName = System.Convert.ToString(value);
                    var converter = new BrushConverter();
                    if (string.CompareOrdinal(baseColorName, "BaseLight") == 0)
                    {
                        var brush = (Brush)converter.ConvertFromString("#FFFFFFFF");
                        return brush;
                    }
                    if (string.CompareOrdinal(baseColorName, "BaseDark") == 0)
                    {
                        var brush = (Brush)converter.ConvertFromString("#FF000000");
                        return brush;
                    }

                    break;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DecimalToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int num = (int)value;
            return num;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToMonthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int month = (int) value;
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
