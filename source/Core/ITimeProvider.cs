namespace Core;

public interface ITimeProvider
{
    /// <summary>
    /// Returns the Current Date and Time as UTC
    /// </summary>
    /// <returns></returns>
    DateTime UtcDateTime();

    /// <summary>
    /// Gets current Date, time is set at 00:00
    /// </summary>
    /// <returns>current DateTime</returns>
    DateTime CurrentDate();
}