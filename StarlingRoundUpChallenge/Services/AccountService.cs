using Microsoft.AspNetCore.Mvc;
using StarlingRoundUpChallenge.Helpers;
using StarlingRoundUpChallenge.Requests;
using StarlingRoundUpChallenge.Response;

namespace StarlingRoundUpChallenge.Services;

public class AccountService : IAccountService
{
    //main method
    /*
     * should:
     *  - take request body //
     *  - call api helper methods //
     *  - call private run calculation method //
     *  - error handling
     */

    private readonly IApiHelper _apiHelper;
    private ILogger<AccountService> _logger;

    public AccountService(IApiHelper apiHelper, ILogger<AccountService> logger)
    {
        _apiHelper = apiHelper;
        _logger = logger;
    }

    public ActionResult RoundUp(RoundUpBetweenRequest request)
    {
        // get accountUid
        var accounts =  _apiHelper.GetAccounts().Result;
        if (accounts == null)
        {
            var errorResponse = GetErrorResponse(new List<string>{"Error occured getting accounts"});
            return new BadRequestObjectResult(errorResponse);
        }
        
        var account = GetAccount(accounts.accounts, request.AccountCurrency);

        if (account == null)
        {
            var errorResponse = GetErrorResponse(new List<string>
                { $"Account with currency type: {request.AccountCurrency} not found" });
            return new BadRequestObjectResult(errorResponse);
        }
        
        // get transfer feed for default category
        var feedItems = _apiHelper.GetSettledTransactionsBetween(account.accountUid,
            request.MinTransactionTimestamp,
            request.MaxTransactionTimestamp).Result?.feedItems.Where(x => x.categoryUid == account.defaultCategory).ToList();
        
        if (feedItems == null)
        {
            var errorResponse = GetErrorResponse(new List<string>
                { "Error occured getting feed items" });
            return new BadRequestObjectResult(errorResponse);
        }
        
        //ToDo: what if there's no feed items for that category
        // should it still create a savings goal and transfer 0 into it?
        
        //calculate roundups
        var roundUp = 0;
        if (feedItems.Count != 0)
        {
            roundUp = GetRoundUpValue(feedItems);
        }
        
        //create savings goal
        var createSavingsGoalRequest = new SavingsGoalRequestV2
        {
            SavingsGoalName = request.SavingsGoalRequest.SavingsGoalName,
            Target = request.SavingsGoalRequest.Target,
            Base64EncodedPhoto = request.SavingsGoalRequest.Base64EncodedPhoto,
            currency = request.AccountCurrency
        };
        var createSavingsGoalResponse = _apiHelper.PutSavingsGoals(account.accountUid, createSavingsGoalRequest).Result;
        if (createSavingsGoalResponse is not { success: true })
        {
            var errorResponse = GetErrorResponse(new List<string>
                { "Error occured creating savings goal" });
            return new BadRequestObjectResult(errorResponse);
        }
        
        // add money to savings goal
        var transferUid = Guid.NewGuid().ToString(); //don't need to test this is unique as is first transaction for the savings goal so always will be
        var topUpRequest = new TopUpRequestV2
        {
            CurrencyAndAmount = new CurrencyAndAmount
            {
                currency = request.AccountCurrency,
                minorUnits = roundUp
            }
        };

        var savingsGoalTransferResponse = _apiHelper.PutMoneySavingsGoal(account.accountUid,
            createSavingsGoalResponse.savingsGoalUid, transferUid, topUpRequest).Result;
        if (savingsGoalTransferResponse is not { success: true })
        {
            var errorResponse = GetErrorResponse(new List<string>
                { "Error occured adding money to savings goal" });
            return new BadRequestObjectResult(errorResponse);
        }

        return new OkResult();
    }

    private static Accounts? GetAccount(Accounts[] accounts, string accountCurrency)
    {
        return accounts.FirstOrDefault(account => account.currency == accountCurrency);
    }

    private static int GetRoundUpValue(List<FeedItems> feedItems)
    {
        var sum = 0;
        foreach (var feedItem in feedItems)
        {
            if (feedItem.direction == "OUT")
            {
                var remainder = feedItem.currencyAndAmount.minorUnits % 100;
                if (remainder != 0) sum += 100 - remainder;
            }
        }

        return sum;
    }

    private ErrorResponse GetErrorResponse(List<string> errorMessages)
    {
        var errorDetails = errorMessages.Select(errorMessage => new ErrorDetail { message = errorMessage }).ToList();

        return new ErrorResponse
        {
            ErrorDetail = errorDetails.ToArray(),
            success = false
        };
    }
}