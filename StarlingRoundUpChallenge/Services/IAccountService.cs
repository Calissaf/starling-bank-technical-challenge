using Microsoft.AspNetCore.Mvc;
using StarlingRoundUpChallenge.Requests;

namespace StarlingRoundUpChallenge.Services;

public interface IAccountService
{
    public ActionResult RoundUp(RoundUpBetweenRequest request);
}