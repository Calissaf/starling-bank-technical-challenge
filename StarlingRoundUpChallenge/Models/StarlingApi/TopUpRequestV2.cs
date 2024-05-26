using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace StarlingRoundUpChallenge.Models.StarlingApi;

public class TopUpRequestV2
{
    [JsonPropertyName("amount")]
    public CurrencyAndAmount CurrencyAndAmount { get; set; }
}