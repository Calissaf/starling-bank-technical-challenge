using System.Net;
using Microsoft.AspNetCore.Mvc;
using StarlingRoundUpChallenge.Helpers;
using StarlingRoundUpChallenge.Models.Requests;
using StarlingRoundUpChallenge.Models.Response;
using StarlingRoundUpChallenge.Models.StarlingApi;

namespace StarlingRoundUpChallenge.Services;

public class AccountService(IApiHelper apiHelper) : IAccountService
{
    private const string RFC3339Format = "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'";
    public async Task<ActionResult> RoundUp(RoundUpBetweenRequest request)
    {
        RoundUpBetweenResponse? response;
        
        //request validation
        if (request.MinTransactionTimestamp >= request.MaxTransactionTimestamp)
        {
            var errorResponse = GetErrorResponse(["Min timestamp must be before max timestamp"]);
            return new BadRequestObjectResult(errorResponse);
        }

        if (!string.IsNullOrEmpty(request.SavingsGoalRequest.Base64EncodedPhoto) && !ValidatePhoto(request.SavingsGoalRequest.Base64EncodedPhoto))
        {
            var errorResponse = GetErrorResponse(["Invalid image"]);
            return new BadRequestObjectResult(errorResponse);
        }
        
        // get accountUid
        var accounts =  await apiHelper.GetAccountsAsync();
        if (accounts == null)
        {
            var errorResponse = GetInternalServerErrorResponse("Error occured getting accounts");
            return new ObjectResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
        
        var account = GetAccountWithCurrencyType(accounts.Accounts, request.AccountCurrency.ToString());

        if (account == null)
        {
            var errorResponse = GetErrorResponse([$"Account with currency type: {request.AccountCurrency} not found"]);
            return new BadRequestObjectResult(errorResponse);
        }
        
        // get transfer feed for default category
        var allFeedItems = await apiHelper.GetSettledTransactionsBetweenAsync(account.AccountUid,
            request.MinTransactionTimestamp.ToString(RFC3339Format),
            request.MaxTransactionTimestamp.ToString(RFC3339Format));

        if (allFeedItems == null)
        {
            var errorResponse = GetInternalServerErrorResponse("Error occured getting feed items");
            return new ObjectResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
        
        // getting default category feed items as only want transactions from the main account and not any spaces tied to the user
        var mainFeedItems = allFeedItems.FeedItems.Where(x => x.CategoryUid == account.DefaultCategory).ToList();
        
        //calculate roundups
        long roundUp = 0;
        if (mainFeedItems.Count != 0)
        {
            roundUp = GetRoundUpValue(mainFeedItems);
        }
        
        //create savings goal
        var createSavingsGoalRequest = new SavingsGoalRequestV2
        {
            SavingsGoalName = request.SavingsGoalRequest.SavingsGoalName,
            Base64EncodedPhoto = request.SavingsGoalRequest.Base64EncodedPhoto,
            Currency = request.AccountCurrency.ToString()
        };
        if (request.SavingsGoalRequest.Target !=null)
        {
            createSavingsGoalRequest.Target = new CurrencyAndAmount
            {
                Currency = request.AccountCurrency.ToString(),
                MinorUnits = request.SavingsGoalRequest.Target.MinorUnits
            };
        }
        var createSavingsGoalResponse = await apiHelper.PutSavingsGoalsAsync(account.AccountUid, createSavingsGoalRequest);
        if (createSavingsGoalResponse is not { Success: true })
        {
            var errorResponse = GetInternalServerErrorResponse("Error occured creating savings goal");
            return new ObjectResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }

        // returning ok when no money to add to savings goal
        // assume user would still want to create a savings goal that could be used for future roundups even if none were resulting from the period specified
        // this could then be used for a recurring roundup feature in the future
        if (roundUp == 0)
        {
            response = new RoundUpBetweenResponse
            {
                SavingsGoalUid = createSavingsGoalResponse.SavingsGoalUid,
                Balance = new CurrencyAndAmount{Currency = createSavingsGoalRequest.Currency, MinorUnits = roundUp},
                Success = true
            };
            return new OkObjectResult(response);
        }
        
        // add money to savings goal
        var transferUid = Guid.NewGuid().ToString(); // don't need to test is unique as is first transaction for the savings goal so always will be
        var topUpRequest = new TopUpRequestV2
        {
            CurrencyAndAmount = new CurrencyAndAmount
            {
                Currency = request.AccountCurrency.ToString(),
                MinorUnits = roundUp
            }
        };

        var savingsGoalTransferResponse = await apiHelper.PutMoneySavingsGoalAsync(account.AccountUid,
            createSavingsGoalResponse.SavingsGoalUid, transferUid, topUpRequest);
        if (savingsGoalTransferResponse is not { Success: true })
        {
            var errorResponse = GetInternalServerErrorResponse("Error occured adding money to savings goal");
            return new ObjectResult(errorResponse)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
        
        response = new RoundUpBetweenResponse
        {
            SavingsGoalUid = createSavingsGoalResponse.SavingsGoalUid,
            Balance = new CurrencyAndAmount{Currency = createSavingsGoalRequest.Currency, MinorUnits = roundUp},
            Success = true
        };
        return new OkObjectResult(response);
    }

    private static bool ValidatePhoto(string base64EncodedPhoto)
    {
        var buffer = new Span<byte>(new byte[base64EncodedPhoto.Length]);
        return Convert.TryFromBase64String(base64EncodedPhoto, buffer, out _);
    }

    private static Accounts? GetAccountWithCurrencyType(Accounts[] accounts, string accountCurrency)
    {
        return accounts.FirstOrDefault(account => account.Currency == accountCurrency);
    }

    /// <summary>
    /// For each feedItem where the direction is OUT denoting a spend from the account and the item hasn't already been used in another round up
    /// Get the remaining pence value from pounds spent by getting the mod from dividing by 100
    /// If that value is 0 we ignore it as only want to roundup any spend over the whole pound value on a transaction
    /// To get the roundup value then minus the pence spend from 100
    /// </summary>
    /// <param name="feedItems"></param>
    /// <returns>Sum integer value of the remaining minor units from all the feedItems</returns>
    private static long GetRoundUpValue(List<FeedItems> feedItems)
    {
        long sum = 0;
        foreach (var feedItem in feedItems)
        {
            if (feedItem is not { Direction: "OUT", AssociatedFeedRoundUp: null }) continue;
            var remainder = feedItem.CurrencyAndAmount.MinorUnits % 100;
            if (remainder != 0) sum += 100 - remainder;
        }

        return sum;
    }

    private static ErrorResponse GetErrorResponse(List<string> errorMessages)
    {
        var errorDetails = errorMessages.Select(errorMessage => new ErrorDetail { Message = errorMessage }).ToList();

        return new ErrorResponse
        {
            ErrorDetail = errorDetails.ToArray(),
            Success = false
        };
    }

    private static ProblemDetails GetInternalServerErrorResponse(string detail)
    {
        return new ProblemDetails {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "Internal Server Error",
            Detail = detail
        };
    }
}