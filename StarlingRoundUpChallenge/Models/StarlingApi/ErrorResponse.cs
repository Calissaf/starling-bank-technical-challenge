using Newtonsoft.Json;

namespace StarlingRoundUpChallenge.Models.StarlingApi;

public class ErrorResponse
{
    [JsonProperty("errors")]
    public ErrorDetail[] ErrorDetail { get; set; }
    public bool Success { get; set; }
}