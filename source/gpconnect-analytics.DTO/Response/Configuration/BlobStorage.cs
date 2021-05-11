namespace gpconnect_analytics.DTO.Response.Configuration
{
    public class BlobStorage
    {
        public string BlobPrimaryKey { get; set; }
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
        public string QueueName { get; set; }
        public string SqlExternalDataSourceName { get; set; }
    }
}
