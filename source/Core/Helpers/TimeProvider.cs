using Core;

public class TimeProvider : ITimeProvider
{
    public DateTime UtcDateTime() => DateTime.UtcNow;

    
    public DateTime CurrentDate() => DateTime.Today;
}