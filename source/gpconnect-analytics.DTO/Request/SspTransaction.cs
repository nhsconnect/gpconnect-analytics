namespace gpconnect_analytics.DTO.Request
{
    public class SspTransaction
    {
        public string _time { get; set; }
        public string sspFrom { get; set; }
        public string sspTo { get; set; }
        public string SspTraceId { get; set; }
        public string interaction { get; set; }
        public string responseCode { get; set; }
        public string duration { get; set; }
        public string responseSize { get; set; }
        public string responseErrorMessage { get; set; }
        public string method { get; set; }
    }
}
