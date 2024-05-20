using System.Text.Json.Serialization;
using Newtonsoft.Json;
using StarlingRoundUpChallenge.Response;

namespace StarlingRoundUpChallenge.Requests;

public class TopUpRequestV2
{
    [JsonProperty("amount")]
    public CurrencyAndAmount CurrencyAndAmount { get; set; }
}