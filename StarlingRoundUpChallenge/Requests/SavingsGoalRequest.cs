using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using StarlingRoundUpChallenge.Response;

namespace StarlingRoundUpChallenge.Requests;

public class SavingsGoalRequest
{
    [Required]
    [StringLength(255, ErrorMessage = "The SavingsGoalName value cannot exceed 255 characters")]
    [JsonProperty("name")] 
    public string SavingsGoalName { get; set; }
    [JsonProperty("target")]
    public CurrencyAndAmount? Target { get; set; }
    [JsonProperty("base64EncodedPhoto")]
    public string? Base64EncodedPhoto { get; set; }
}