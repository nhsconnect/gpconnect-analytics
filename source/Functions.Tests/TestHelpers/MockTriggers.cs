using Microsoft.Azure.Functions.Worker;

namespace Functions.Tests.TestHelpers;

public class MockTriggers
{
    public static TimerInfo CreateMockTimerInfo()
    {
        return new TimerInfo
        {
            ScheduleStatus = new ScheduleStatus()
        };
    }
}