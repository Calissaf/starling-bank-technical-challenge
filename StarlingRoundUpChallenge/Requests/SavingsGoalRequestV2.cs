using System.ComponentModel.DataAnnotations;

namespace StarlingRoundUpChallenge.Requests;

public class SavingsGoalRequestV2 : SavingsGoalRequest
{
    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "The Currency value length must be 3 characters")]
    public string currency { get; set; }
}