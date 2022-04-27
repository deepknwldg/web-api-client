using Microsoft.AspNetCore.Http;

namespace WebApiClient;

public interface IRequestProvider
{
    string CurrentCallId { get; set; }
    void ReadOrAddRequestId(HttpContext httpContext);
    string GetCurrentRequestId();
}

public class RequestProvider : IRequestProvider
{
    private string _requestId;
    public string CurrentCallId { get; set; }

    public void ReadOrAddRequestId(HttpContext httpContext)
    {
        const string requestIdHeader = "RequestId";

        if (!httpContext.Request.Headers.ContainsKey(requestIdHeader))
        {
            httpContext.Request.Headers.Add(requestIdHeader, GetCurrentRequestId());
        }
        else
        {
            _requestId = httpContext.Request.Headers[requestIdHeader];
        }
    }

    public string GetCurrentRequestId()
    {
        if (string.IsNullOrEmpty(_requestId))
        {
            _requestId = Guid.NewGuid().ToString();
        }

        return _requestId;
    }
}