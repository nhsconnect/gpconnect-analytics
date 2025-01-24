using Core.Helpers;
using FluentAssertions;
using Xunit;

namespace gpconnect_analytics.Test;

public class DateFormatConstantsTests
{
    [Fact]
    public void ValidateDateFormatConstants()
    {
        DateFormatConstants.FilePathQueryDate.Should().Be("yyyyMMdd");
        DateFormatConstants.FilePathQueryHour.Should().Be("hhmmss");
        DateFormatConstants.FilePathQueryDateYearMonth.Should().Be("yyyy-MM");
        DateFormatConstants.FilePathNowDate.Should().Be("yyyyMMddTHHmmss");
        DateFormatConstants.SplunkQueryDate.Should().Be("MM/dd/yyyy:HH:mm:ss");
        DateFormatConstants.SplunkQueryHour.Should().Be("hhmm");
    }
}