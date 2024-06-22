using Avalonia.Data.Converters;
using Avalonia.Platform.Storage;
using System;
using System.Globalization;
using System.Linq;

namespace PhotoArchivingTools.Converters
{
    public class FilePickerFileTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            return value is FilePickerFileType f
                ? $"{f.Name};{string.Join(',', f.Patterns)};{string.Join(',', f.MimeTypes)};{string.Join(',', f.AppleUniformTypeIdentifiers)}"
                : throw new Exception($"给定的对象不是{nameof(FilePickerFileType)}类型");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is not string stringValue)
            {
                throw new Exception($"给定的对象不是字符串类型");
            }

            var parts = stringValue.Split(';');
            if (parts.Length != 4)
            {
                throw new Exception($"字符串格式不正确，预期格式：Name;Patterns;MimeTypes;AppleUniformTypeIdentifiers");
            }

            var name = parts[0];
            var patterns = parts[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var mimeTypes = parts[2].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var appleUniformTypeIdentifiers = parts[3].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            return new FilePickerFileType(name)
            {
                Patterns = patterns.ToList(),
                MimeTypes = mimeTypes.ToList(),
                AppleUniformTypeIdentifiers = appleUniformTypeIdentifiers.ToList()
            };
        }
    }
}
