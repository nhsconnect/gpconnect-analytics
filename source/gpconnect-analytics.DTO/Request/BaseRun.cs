using System;

namespace gpconnect_analytics.DTO.Request
{
    public class BaseRun
    {
        public DateTime RunStartTime { get; set; }
        public DateTime RunEndTime { get; set; }
        public bool Success { get; set; }
        public System.Net.HttpStatusCode ResponseCode { get; set; }
        public string FilePath { get; set; }
        public int FileRowCount { get; set; }
        public string ErrorMessage { get; set; }
    }
}
