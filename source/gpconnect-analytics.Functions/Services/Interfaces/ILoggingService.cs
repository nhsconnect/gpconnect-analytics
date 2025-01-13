namespace function_app.Services.Interfaces
{
    public interface ILoggingService
    {
        Task PurgeErrorLog();
    }
}