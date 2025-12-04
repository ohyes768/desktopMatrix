using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using DesktopMatrix.Models;
namespace DesktopMatrix.Utils
{
    public class EnumToCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumType = typeof(QuadrantType);
            var values = Enum.GetValues(enumType);
            var result = new List<string>();

            foreach (var enumValue in values)
            {
                var fieldInfo = enumType.GetField(enumValue.ToString());
                var description = fieldInfo.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
                    .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
                
                result.Add(description?.Description ?? enumValue.ToString());
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}