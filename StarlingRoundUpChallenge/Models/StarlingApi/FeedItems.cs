using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace StarlingRoundUpChallenge.Models.StarlingApi;

public class FeedItems
{
    public string FeedItemUid { get; set; }
    public string CategoryUid { get; set; }
    [JsonPropertyName("amount")]
    [JsonProperty(PropertyName = "amount")]
    public CurrencyAndAmount CurrencyAndAmount { get; set; }
    public string Direction { get; set; }
    //ToDo check round up id is empty when calculating my round ups
    [JsonProperty("roundUp")]
    public AssociatedFeedRoundUp AssociatedFeedRoundUp { get; set; }
}