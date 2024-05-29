using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StarlingRoundUpChallenge.Models.StarlingApi;

public class SavingsGoalRequestV2
{
    [Required]
    [MaxLength(255, ErrorMessage = "The savingsGoalName cannot exceed 255 characters")] 
    [JsonPropertyName("name")]
    public string? SavingsGoalName { get; set; }
    public string? Base64EncodedPhoto { get; set; }
    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "The Currency value length must be 3 characters")]
    public string Currency { get; set; }
    public CurrencyAndAmount? Target { get; set; }
}