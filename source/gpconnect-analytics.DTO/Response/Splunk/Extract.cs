﻿using System;

namespace gpconnect_analytics.DTO.Response.Splunk
{
    public class Extract
    {
        public bool ExtractRequired { get; set; }
        public DateTime QueryFromDate { get; set; }
        public DateTime QueryToDate { get; set; }
        public TimeSpan QueryHour { get; set; }
        public bool Override { get; set; } = false;
    }
}
