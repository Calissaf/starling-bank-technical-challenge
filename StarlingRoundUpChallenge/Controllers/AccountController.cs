using Microsoft.AspNetCore.Mvc;
using StarlingRoundUpChallenge.Models.Requests;
using StarlingRoundUpChallenge.Models.StarlingApi;
using StarlingRoundUpChallenge.Services;
using Exception = System.Exception;

namespace StarlingRoundUpChallenge.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController(IAccountService accountService) : ControllerBase
{

    [HttpPost]
    [Route("accounts/feed/round-up")]
    public async Task<ActionResult> PostRoundUp([FromQuery] RoundUpBetweenRequest roundUpBetweenRequest)
    {
        try
        {
            return await accountService.RoundUp(roundUpBetweenRequest);
        }
        catch (Exception e)
        {
            var errorResponse = new ErrorResponse
            {
                ErrorDetail =
                [
                    new ErrorDetail
                    {
                        Message = e.Message
                    }
                ],
                Success = false
            };
            return new BadRequestObjectResult(errorResponse);
        }
    }
}