using Newtonsoft.Json;

namespace StarlingRoundUpChallenge.Response;

public class ErrorResponse
{
    [JsonProperty("errors")]
    public ErrorDetail[] ErrorDetail { get; set; }
    public bool success { get; set; }
}