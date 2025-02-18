using System.Reflection;

namespace Core.Helpers
{
    public static class AttributeExtensions
    {
        public static FileTypes? GetFileType<TFilePath>(this string filePath)
        {
            return GetValueFromPath<TFilePath>(filePath);
        }

        private static FileTypes? GetValueFromPath<T>(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            var fileType = typeof(T).GetFields()
                .FirstOrDefault(field =>
                    field.GetCustomAttribute<FilePathAttribute>() is { } attribute &&
                    filePath.Contains(attribute.FilePath));

            return fileType?.GetValue(null) as FileTypes?;
        }
    }
}