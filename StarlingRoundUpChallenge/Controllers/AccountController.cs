using Microsoft.AspNetCore.Mvc;
using StarlingRoundUpChallenge.Requests;
using StarlingRoundUpChallenge.Response;
using StarlingRoundUpChallenge.Services;
using Exception = System.Exception;

namespace StarlingRoundUpChallenge.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAccountService accountService, ILogger<AccountController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    [HttpPost]
    [Route(("accounts/feed/round-up"))]
    public ActionResult PostRoundUp([FromQuery] RoundUpBetweenRequest roundUpBetweenRequest)
    {
        try
        {
            return _accountService.RoundUp(roundUpBetweenRequest);
        }
        catch (Exception e)
        {
            var errorResponse = new ErrorResponse
            {
                ErrorDetail = new []
                {
                    new ErrorDetail
                    {
                        message = e.Message
                    }
                },
                success = false
            };
            return new BadRequestObjectResult(errorResponse);
        }
    }
}