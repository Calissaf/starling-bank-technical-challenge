using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StarlingRoundUpChallenge.Helpers;
using StarlingRoundUpChallenge.Requests;
using StarlingRoundUpChallenge.Response;
using StarlingRoundUpChallenge.Services;

namespace StarlingRoundUpChallengeTests.Services;

public class AccountServiceTests
{
    private AccountService accountService;
    private Mock<IApiHelper> mockApiHelper;
    private Mock<ILogger<AccountService>> mocklogger;
    private static readonly RoundUpBetweenRequest defaultRequest = new()
    {
        MinTransactionTimestamp = "2024-05-11T00:00:00.000Z",
        MaxTransactionTimestamp = "2024-05-11T00:00:00.000Z",
        AccountCurrency = "GBP",
        SavingsGoalRequest = new SavingsGoalRequest
        {
            SavingsGoalName = "test",
        }
    };


    public AccountServiceTests()
    {
        mockApiHelper = new Mock<IApiHelper>();
        mocklogger = new Mock<ILogger<AccountService>>();
        accountService = new AccountService(mockApiHelper.Object, mocklogger.Object);
    }
    
    // get accounts tests
    /*
     * should call get accounts//
     * when get accounts returns null should return http bad request with error message//
     * when get accounts returns no accounts with matching currency should return http bad request with error message//
     */
    
    [Fact]
    public void RoundUp_CallsGetAccounts()
    {
        Setup();
        
        accountService.RoundUp(defaultRequest);
        mockApiHelper.Verify(x => x.GetAccounts(), Times.Once);
    }

    [Fact]
    public void RoundUp_WhenGetAccountsReturnsNull_ReturnBadRequest()
    {
        var expected = new BadRequestObjectResult(new ErrorResponse {ErrorDetail = new []{new ErrorDetail{message = "Error occured getting accounts"}}, success = false});
        var result = accountService.RoundUp(defaultRequest);

        result.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public void RoundUp_WhenNoAccountsWithRequestedCurrencyType_ReturnBadRequest()
    {
        mockApiHelper.Setup(x => x.GetAccounts()).ReturnsAsync(new Account{accounts = new []{new Accounts{currency = "EUR"}}});
        var expected = new BadRequestObjectResult(new ErrorResponse {ErrorDetail = new []{new ErrorDetail{message = $"Account with currency type: {defaultRequest.AccountCurrency} not found"}}, success = false});
        
        var result = accountService.RoundUp(defaultRequest);

        result.Should().BeEquivalentTo(expected);
    }
    
    //get feed items 
    /*
     * calls get feed items //
     * when get feed items returns null return http bad request //
     * if feed items empty account savings account created with 0 //
     * if feed items exist calculate roundup and transfer account with that money //
     */
    
    [Fact]
    public void RoundUp_CallsGetSettledTransactionsBetween()
    {
        Setup();
        
        accountService.RoundUp(defaultRequest);
        mockApiHelper.Verify(x => x.GetSettledTransactionsBetween(It.IsAny<string>(), defaultRequest.MinTransactionTimestamp, defaultRequest.MaxTransactionTimestamp), Times.Once);
    }
    
    [Fact]
    public void RoundUp_WhenGetSettledTransactionsBetweenReturnsNull_ReturnBadRequest()
    {
        Setup();
        mockApiHelper
            .Setup(x => x.GetSettledTransactionsBetween(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((Feed?)null);
        var expected = new BadRequestObjectResult(new ErrorResponse {ErrorDetail = new []{new ErrorDetail{message = "Error occured getting feed items"}}, success = false});
        var result = accountService.RoundUp(defaultRequest);

        result.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public void RoundUp_WhenNoSettledTransactionsOccur_CreateSavingsAccountWithEmptyBalance()
    {
        Setup();
        mockApiHelper
            .Setup(x => x.GetSettledTransactionsBetween(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Feed{feedItems = Array.Empty<FeedItems>()});
        accountService.RoundUp(defaultRequest);

        mockApiHelper.Verify(x => x.PutMoneySavingsGoal(It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>(), It.Is<TopUpRequestV2>(topUpRequestV2 => topUpRequestV2.CurrencyAndAmount.minorUnits == 0)), Times.Once);
    }
    
    [Fact]
    public void RoundUp_WhenSettledTransactionsOccurForDefaultCategoryId_CreateSavingsAccountWithExpectedBalance()
    {
        Setup();
        accountService.RoundUp(defaultRequest);
        
        mockApiHelper.Verify(x => x.PutMoneySavingsGoal(It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>(), It.Is<TopUpRequestV2>(topUpRequestV2 => topUpRequestV2.CurrencyAndAmount.minorUnits == 76)), Times.Once);
    }
    
    //create savings goal
    /*
     * calls create savings goal //
     * when create savings goal returns null return http bad request //
     * when create savings goal success is false return http bad request //
     */
    [Fact]
    public void RoundUp_CallsPutSavingsGoals()
    {
        Setup();
        
        accountService.RoundUp(defaultRequest);
        mockApiHelper.Verify(x => x.PutSavingsGoals(It.IsAny<string>(), It.Is<SavingsGoalRequestV2>(v2 => v2.SavingsGoalName == defaultRequest.SavingsGoalRequest.SavingsGoalName && v2.currency == defaultRequest.AccountCurrency && v2.Base64EncodedPhoto == defaultRequest.SavingsGoalRequest.Base64EncodedPhoto)), Times.Once);
    }
    
    [Fact]
    public void RoundUp_WhenPutSavingsGoalsIsNull_ReturnHttpBadRequest()
    {
        Setup();

        mockApiHelper.Setup(x => x.PutSavingsGoals(It.IsAny<string>(), It.IsAny<SavingsGoalRequestV2>()))
            .ReturnsAsync((CreateOrUpdateSavingsGoalResponseV2?)null);

        var expected = new BadRequestObjectResult(new ErrorResponse
        {
            ErrorDetail = new[] { new ErrorDetail { message = "Error occured creating savings goal" } }, success = false
        });
        var result = accountService.RoundUp(defaultRequest);
        
        result.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public void RoundUp_WhenPutSavingsGoalsSuccessIsFalse_ReturnHttpBadRequest()
    {
        Setup();

        mockApiHelper.Setup(x => x.PutSavingsGoals(It.IsAny<string>(), It.IsAny<SavingsGoalRequestV2>()))
            .ReturnsAsync(new CreateOrUpdateSavingsGoalResponseV2{savingsGoalUid = It.IsAny<string>(), success = false});

        var expected = new BadRequestObjectResult(new ErrorResponse
        {
            ErrorDetail = new[] { new ErrorDetail { message = "Error occured creating savings goal" } }, success = false
        });
        var result = accountService.RoundUp(defaultRequest);
        
        result.Should().BeEquivalentTo(expected);
    }
    
    // add money to savings goal
    /*
     * calls put money savings goal //
     * when response is null return http bad request //
     * when response success is false return http bad request//
     * when response success is true return ok
     */
    [Fact]
    public void RoundUp_CallsPutMoneySavingsGoals()
    {
        Setup();
        
        mockApiHelper.Setup(x => x.PutMoneySavingsGoal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopUpRequestV2>()))
            .ReturnsAsync(It.IsAny<SavingsGoalTransferResponse>());
        
        accountService.RoundUp(defaultRequest);
        mockApiHelper.Verify(x => x.PutMoneySavingsGoal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<TopUpRequestV2>(v2 => v2.CurrencyAndAmount.minorUnits == 76 && v2.CurrencyAndAmount.currency == defaultRequest.AccountCurrency)), Times.Once);
    }
    
    [Fact]
    public void RoundUp_WhenPutMoneySavingsGoalsIsNull_ReturnHttpBadRequest()
    {
        Setup();

        mockApiHelper
            .Setup(x => x.PutMoneySavingsGoal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<TopUpRequestV2>())).ReturnsAsync((SavingsGoalTransferResponse?)null);
        var expected = new BadRequestObjectResult(

            new ErrorResponse
            {
                ErrorDetail = new []
                {
                    new ErrorDetail
                    {
                        message = "Error occured adding money to savings goal"
                    }
                },
                success = false
            }
        );
        var result = accountService.RoundUp(defaultRequest);
        
        result.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public void RoundUp_WhenPutMoneySavingsGoalsSuccessIsFalse_ReturnHttpBadRequest()
    {
        Setup();

        mockApiHelper
            .Setup(x => x.PutMoneySavingsGoal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<TopUpRequestV2>())).ReturnsAsync(new SavingsGoalTransferResponse{transferUid = It.IsAny<string>(), success = false});
        var expected = new BadRequestObjectResult(

            new ErrorResponse
            {
                ErrorDetail = new []
                {
                    new ErrorDetail
                    {
                        message = "Error occured adding money to savings goal"
                    }
                },
                success = false
            }
        );
        var result = accountService.RoundUp(defaultRequest);
        
        result.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public void RoundUp_WhenPutMoneySavingsGoalsSuccessIsTrue_ReturnHttpOk()
    {
        Setup();

        mockApiHelper
            .Setup(x => x.PutMoneySavingsGoal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<TopUpRequestV2>())).ReturnsAsync(new SavingsGoalTransferResponse{transferUid = It.IsAny<string>(), success = true});
       
        var result = accountService.RoundUp(defaultRequest);
        
        result.Should().BeEquivalentTo(new OkResult());
    }
    
    

    private void Setup()
    {
        //round up = 76
        var feed = new Feed
        {
            feedItems = new[]
            {
                new FeedItems
                {
                    categoryUid = "456",
                    direction = "OUT",
                    currencyAndAmount = new CurrencyAndAmount
                    {
                        currency = "GBP",
                        minorUnits = 300
                    }
                },
                new FeedItems
                {
                    categoryUid = "456",
                    direction = "OUT",
                    currencyAndAmount = new CurrencyAndAmount
                    {
                        currency = "GBP",
                        minorUnits = 250
                    }
                },
                new FeedItems
                {
                    categoryUid = "456",
                    direction = "OUT",
                    currencyAndAmount = new CurrencyAndAmount
                    {
                        currency = "GBP",
                        minorUnits = 375
                    }
                },
                new FeedItems
                {
                    categoryUid = "456",
                    direction = "OUT",
                    currencyAndAmount = new CurrencyAndAmount
                    {
                        currency = "GBP",
                        minorUnits = 399
                    }
                },
                new FeedItems
                {
                    categoryUid = "456",
                    direction = "IN",
                    currencyAndAmount = new CurrencyAndAmount
                    {
                        currency = "GBP",
                        minorUnits = 399
                    }
                },
            }
        };
        
        var account = new Account
        {
            accounts = new []
            {
                new Accounts
                {
                    accountUid = "123",
                    defaultCategory = "456",
                    currency = "GBP"
                }
            }
        };
        var createSavingsGoalResponse = new CreateOrUpdateSavingsGoalResponseV2
        {
            savingsGoalUid = "123",
            success = true
        };
        var savingsGoalTransferResponse = new SavingsGoalTransferResponse
        {
            transferUid = "123",
            success = true
        };

        mockApiHelper.Setup(x => x.GetAccounts()).ReturnsAsync(account);
        mockApiHelper
            .Setup(x => x.GetSettledTransactionsBetween(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(feed);
        mockApiHelper.Setup(x => x.PutSavingsGoals(It.IsAny<string>(), It.IsAny<SavingsGoalRequestV2>()))
            .ReturnsAsync(createSavingsGoalResponse);
        mockApiHelper
            .Setup(x => x.PutMoneySavingsGoal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<TopUpRequestV2>())).ReturnsAsync(savingsGoalTransferResponse);
    }
}