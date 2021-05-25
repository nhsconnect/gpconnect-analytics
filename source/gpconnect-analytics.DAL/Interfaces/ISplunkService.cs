﻿using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.DTO.Response.Splunk;
using System.Threading.Tasks;

namespace gpconnect_analytics.DAL.Interfaces
{
    public interface ISplunkService
    {
        Task<ExtractResponse> DownloadCSV(FileType fileType);
    }
}
