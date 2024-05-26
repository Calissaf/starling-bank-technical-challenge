using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace StarlingRoundUpChallenge.Models.Requests;

public class SavingsGoalRequest
{
    [Required]
    [MaxLength(255, ErrorMessage = "The savingsGoalName cannot exceed 255 characters")]
    [JsonProperty("name")] 
    public string SavingsGoalName { get; set; }
    [JsonProperty("target")]
    public Target? Target { get; set; }
    [JsonProperty("base64EncodedPhoto")]
    public string? Base64EncodedPhoto { get; set; }
}

public class Target
{
    public int MinorUnits { get; set; }
}