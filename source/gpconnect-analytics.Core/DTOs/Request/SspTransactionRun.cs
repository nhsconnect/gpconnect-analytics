namespace Core.DTOs.Request
{
    public class SspTransactionRun : BaseRun
    {
        public DateTime QueryFromDate { get; set; }
        public DateTime QueryToDate { get; set; }
    }
}