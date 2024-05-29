using Microsoft.AspNetCore.Mvc;
using StarlingRoundUpChallenge.Models.Requests;

namespace StarlingRoundUpChallenge.Services;

public interface IAccountService
{
    public Task<ActionResult> RoundUp(RoundUpBetweenRequest request);
}