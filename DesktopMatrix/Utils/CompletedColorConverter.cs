using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DesktopMatrix.Utils
{
    /// <summary>
    /// 完成状态颜色转换器
    /// </summary>
    public class CompletedColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCompleted)
            {
                return isCompleted ? new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Color.FromRgb(51, 51, 51));
            }
            return new SolidColorBrush(Color.FromRgb(51, 51, 51));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}