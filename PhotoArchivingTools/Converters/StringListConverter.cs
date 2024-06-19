using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PhotoArchivingTools.Converters
{
    public class StringListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable<string> list = value as IEnumerable<string>;
            return string.Join(", ", list);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                var list = s.Split(new string[] { ",", "，" }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim());

                if (targetType == typeof(string[]))
                {
                    return list.ToArray();
                }
                else if (targetType == typeof(List<string>))
                {
                    return list.ToList();
                }
                else
                {
                    throw new Exception();
                }
            }
            throw new Exception();
        }
    }
}
