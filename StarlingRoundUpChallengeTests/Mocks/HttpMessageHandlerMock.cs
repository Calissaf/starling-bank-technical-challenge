using System.Net;
using System.Text;

namespace StarlingRoundUpChallengeTests.Mocks;

public class HttpMessageHandlerMock(HttpStatusCode code, string json) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = code,
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }
}