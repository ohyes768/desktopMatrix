using System;
using System.Globalization;
using System.Windows.Data;
using DesktopMatrix.Models;

namespace DesktopMatrix.Utils
{
    /// <summary>
    /// 象限显示转换器
    /// </summary>
    public class QuadrantDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is QuadrantType quadrant)
            {
                return quadrant switch
                {
                    QuadrantType.Q1 => "Q1",
                    QuadrantType.Q2 => "Q2",
                    QuadrantType.Q3 => "Q3",
                    QuadrantType.Q4 => "Q4",
                    _ => "Q1"
                };
            }
            return "Q1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str switch
                {
                    "Q1" => QuadrantType.Q1,
                    "Q2" => QuadrantType.Q2,
                    "Q3" => QuadrantType.Q3,
                    "Q4" => QuadrantType.Q4,
                    _ => QuadrantType.Q1
                };
            }
            return QuadrantType.Q1;
        }
    }
}