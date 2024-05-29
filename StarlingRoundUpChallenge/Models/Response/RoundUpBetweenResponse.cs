using StarlingRoundUpChallenge.Models.StarlingApi;

namespace StarlingRoundUpChallenge.Models.Response;

public class RoundUpBetweenResponse
{
    
    public string? SavingsGoalUid { get; set; } 
    public CurrencyAndAmount? Balance { get; set; }
    public bool Success { get; set; }
}