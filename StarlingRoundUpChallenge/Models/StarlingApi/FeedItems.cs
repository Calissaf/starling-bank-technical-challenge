using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace StarlingRoundUpChallenge.Models.StarlingApi;

public class FeedItems
{
    public string FeedItemUid { get; set; }
    public string CategoryUid { get; set; }
    [JsonPropertyName("amount")]
    public CurrencyAndAmount CurrencyAndAmount { get; set; }
    public string Direction { get; set; }
    [JsonProperty("roundUp")]
    public AssociatedFeedRoundUp AssociatedFeedRoundUp { get; set; }
}