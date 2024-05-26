using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StarlingRoundUpChallenge.Models.Requests;

public class RoundUpBetweenRequest
{
    [BindRequired, FromQuery(Name = "minTransactionTimestamp")]
    public DateTime MinTransactionTimestamp { get; init; }

    [BindRequired, FromQuery(Name = "maxTransactionTimestamp")]
    public DateTime MaxTransactionTimestamp { get; init; }

    public AccountCurrencyTypes AccountCurrency { get; init; }
    [BindRequired, FromBody] 
    public SavingsGoalRequest SavingsGoalRequest { get; init; }

    public enum AccountCurrencyTypes
    {
        GBP,
        EUR
    }
}