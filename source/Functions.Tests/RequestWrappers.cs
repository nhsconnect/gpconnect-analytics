using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Functions.Tests;

[ExcludeFromCodeCoverage]
public class FakeHttpRequestData : HttpRequestData
{
    public FakeHttpRequestData(FunctionContext functionContext, Uri url, Stream body = null) : base(functionContext)
    {
        Url = url;
        Body = body ?? new MemoryStream();
    }

    public override Stream Body { get; } = new MemoryStream();
    public override HttpHeadersCollection Headers { get; } = new HttpHeadersCollection();
    public override IReadOnlyCollection<IHttpCookie> Cookies { get; }
    public override Uri Url { get; }
    public override IEnumerable<ClaimsIdentity> Identities { get; }
    public override string Method { get; }

    public override HttpResponseData CreateResponse()
    {
        return new FakeHttpResponseData(FunctionContext);
    }
}

[ExcludeFromCodeCoverage]
public class FakeHttpResponseData : HttpResponseData
{
    public FakeHttpResponseData(FunctionContext functionContext) : base(functionContext)
    {
    }

    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; } = new HttpHeadersCollection();
    public override Stream Body { get; set; } = new MemoryStream();
    public override HttpCookies Cookies { get; }
}