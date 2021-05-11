using System;

namespace gpconnect_analytics.DTO.Response.Configuration
{
    public class FileType
    {
        public int FileTypeId { get; set; }
        public string DirectoryName { get; set; }
        public string FileTypeFilePrefix { get; set; }
        public string SplunkQuery { get; set; }
        public bool UsesQueryDates { get; set; }
        public DateTime? QueryFromBaseDate { get; set; }
        public int QueryPeriodHours { get; set; }
    }
}
