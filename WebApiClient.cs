using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Logging;

namespace WebApiClient;

public interface IWebApiClient
{
    Task DoRequest<TRequest>(HttpMethod httpMethod, TRequest request, Uri uri);
    Task<TResponse> DoRequest<TRequest, TResponse>(HttpMethod httpMethod, TRequest request, Uri uri);
    Task DoEmptyRequest(HttpMethod httpMethod, Uri uri);
    Task<TResponse> DoEmptyRequest<TResponse>(HttpMethod httpMethod, Uri uri);
}

public class WebApiClient : IWebApiClient
{
    private const string MEDIATYPE = "application/json";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRequestProvider _requestProvider;
    private readonly ILogger<WebApiClient> _logger;

    public WebApiClient(IHttpClientFactory httpClientFactory, ILogger<WebApiClient> logger)
    {
        _logger = logger;
    }

    public WebApiClient(
        IHttpClientFactory httpClientFactory,
        IRequestProvider requestProvider,
        ILogger<WebApiClient> logger)
        : this(httpClientFactory, logger)
    {
        _requestProvider = requestProvider;
    }

    public async Task DoRequest<TRequest>(HttpMethod httpMethod, TRequest request, Uri uri)
    {
        await DoRequestInternal(httpMethod, uri, request);
    }

    public async Task<TResponse> DoRequest<TRequest, TResponse>(HttpMethod httpMethod, TRequest request, Uri uri)
    {
        var response = await DoRequestInternal(httpMethod, uri, request);

        return await DeserializeContent<TResponse>(response);
    }

    public async Task DoEmptyRequest(HttpMethod httpMethod, Uri uri)
    {
        await DoRequestInternal(httpMethod, uri);
    }

    public async Task<TResponse> DoEmptyRequest<TResponse>(HttpMethod httpMethod, Uri uri)
    {
        var response = await DoRequestInternal(httpMethod, uri);

        return await DeserializeContent<TResponse>(response);
    }

    private async Task<HttpResponseMessage> DoRequestInternal(HttpMethod httpMethod, Uri uri,
        object requestContent = null)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromMinutes(10);
        httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(MEDIATYPE));

        if (_requestProvider != null)
        {
            httpClient.DefaultRequestHeaders.Add("RequestId", _requestProvider.GetCurrentRequestId());
        }

        var request = new HttpRequestMessage
        {
            Method = httpMethod,
            RequestUri = uri
        };

        var requestBody = string.Empty;
        if (requestContent != null)
        {
            requestBody = SerializeFunctions.SerializeToJson(requestContent);
            request.Content = new StringContent(SerializeFunctions.SerializeToJson(requestContent), Encoding.UTF8,
                MEDIATYPE);
        }

        _logger.LogInformation($"Send request. {httpMethod} Url: {uri.AbsoluteUri}\r{requestBody}");
        var response = await httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogInformation(
            $"Receive response. {httpMethod} Url: {uri.AbsoluteUri}. StatusCode: {response.StatusCode}\r{responseBody}");

        return response;
    }

    public static async Task<T> DeserializeContent<T>(HttpResponseMessage httpReponse)
    {
        var stringContent = await httpReponse.Content.ReadAsStringAsync();

        return SerializeFunctions.DeserializeFromJson<T>(stringContent);
    }
}