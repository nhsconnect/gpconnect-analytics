using System;
using System.Linq;

namespace gpconnect_analytics.Helpers
{
    public static class AttributeExtensions
    {
        public static FileTypes? GetFileType<FilePath>(this string filePath)
        {
            return GetValueFromPath<FilePath>(filePath);
        }

        public static FileTypes? GetValueFromPath<FilePath>(string filePath)
        {
            var fileType = typeof(FilePath).GetFields()
                                       .Where(x => Attribute.GetCustomAttribute(x, typeof(FilePathAttribute)) is FilePathAttribute filePathAttribute && filePath.Contains(filePathAttribute?.FilePath))
                                       .FirstOrDefault();
            return fileType != null ? (FileTypes)fileType.GetValue(filePath) : (FileTypes?)null;
        }
    }
}