using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace StarlingRoundUpChallenge.Response;

public class FeedItems
{
    public string feedItemUid { get; set; }
    public string categoryUid { get; set; }
    [JsonPropertyName("amount")]
    [JsonProperty(PropertyName = "amount")]
    public CurrencyAndAmount currencyAndAmount { get; set; }
    public string direction { get; set; }
    //ToDo check round up id is empty when calculating my round ups
    [JsonProperty("roundUp")]
    public AssociatedFeedRoundUp associatedFeedRoundUp { get; set; }
}