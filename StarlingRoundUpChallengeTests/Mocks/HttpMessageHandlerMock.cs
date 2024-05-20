using System.Net;
using System.Text;

namespace StarlingRoundUpChallengeTests.Mocks;

public class HttpMessageHandlerMock : HttpMessageHandler
{
    private HttpStatusCode _code;
    private readonly string _json;
    
    public HttpMessageHandlerMock(HttpStatusCode code, string json)
    {
        _code = code;
        _json = json;
    }
    
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = _code,
            Content = new StringContent(_json, Encoding.UTF8, "application/json")
        });
    }
}