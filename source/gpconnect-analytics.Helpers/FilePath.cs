using System;

namespace gpconnect_analytics.Helpers
{
    public class FilePathAttribute : Attribute
    {
        public string FilePath { get; protected set; } = "";

        public FilePathAttribute(string value)
        {
            FilePath = value;
        }
    }
}
