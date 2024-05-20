using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StarlingRoundUpChallenge.Requests;

public class RoundUpBetweenRequest
{
    [BindRequired, FromQuery(Name = "minTransactionTimestamp")]
    public string MinTransactionTimestamp { get; init; }
    [BindRequired, FromQuery(Name = "maxTransactionTimestamp")]
    public string MaxTransactionTimestamp { get; init; }
    public string AccountCurrency { get; init; }
    [BindRequired, FromBody]
    public SavingsGoalRequest SavingsGoalRequest { get; init; }
}