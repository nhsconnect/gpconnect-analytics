using System.Text;
using Microsoft.Azure.Functions.Worker;
using Moq;

namespace Functions.Tests.TestHelpers;

public class MockRequests
{
    public static FakeHttpRequestData CreateDateRangeMockRequest(string startDate, string endDate)
    {
        var context = new Mock<FunctionContext>();
        var requestMock = new FakeHttpRequestData(context.Object,
            new Uri($"https://localhost/api?StartDate={startDate}&EndDate={endDate}"));

        return requestMock;
    }

    public static FakeHttpRequestData CreateTodayMockRequest()
    {
        var context = new Mock<FunctionContext>();
        var requestMock = new FakeHttpRequestData(context.Object,
            new Uri($"https://localhost/api/getDataFromToday"));

        return requestMock;
    }

    public static FakeHttpRequestData MockAddDownloadRequest()
    {
        var context = new Mock<FunctionContext>();
        var requestMock = new FakeHttpRequestData(context.Object,
            new Uri($"https://localhost/api?FilePath=asid-lookup-data/test.csv"));

        return requestMock;
    }

    public static FakeHttpRequestData MockRequestNoQuery()
    {
        var context = new Mock<FunctionContext>();
        var requestMock = new FakeHttpRequestData(context.Object,
            new Uri($"https://localhost/api"));

        return requestMock;
    }

    public static FakeHttpRequestData MockRequestWithBody(string jsonBody)
    {
        var context = new Mock<FunctionContext>();
        var requestMock = new FakeHttpRequestData(
            context.Object,
            new Uri($"https://localhost/api"),
            new MemoryStream(Encoding.UTF8.GetBytes(jsonBody)));


        return requestMock;
    }
}