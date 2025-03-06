namespace Core.Helpers
{
    public class FilePathAttribute : Attribute
    {
        public string FilePath { get; } = "";

        public FilePathAttribute(string value)
        {
            FilePath = value;
        }
    }
}