﻿using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PhotoArchivingTools.Converters
{
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan t)
            {
                return parameter switch
                {
                    "h" => System.Convert.ToDecimal(t.TotalHours),
                    "m" => System.Convert.ToDecimal(t.TotalMinutes),
                    "s" => System.Convert.ToDecimal(t.TotalSeconds),
                    _ => throw new NotSupportedException("Converter参数错误，未知的转换类型"),
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = System.Convert.ToDouble(value);

            return parameter switch
            {
                "h" => TimeSpan.FromHours(d),
                "m" => TimeSpan.FromMinutes(d),
                "s" => TimeSpan.FromSeconds(d),
                _ => throw new NotSupportedException("Converter参数错误，未知的转换类型"),
            };
        }
    }
}
