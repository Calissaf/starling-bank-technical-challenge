using System.Net;
using Microsoft.AspNetCore.Mvc;
using StarlingRoundUpChallenge.Helpers;
using StarlingRoundUpChallenge.Models.Requests;
using StarlingRoundUpChallenge.Models.Response;
using StarlingRoundUpChallenge.Models.StarlingApi;
using StarlingRoundUpChallenge.Services;

namespace StarlingRoundUpChallengeTests.Services;

public class AccountServiceTests
{
    private readonly AccountService _accountService;
    private readonly Mock<IApiHelper> _mockApiHelper;
    private static readonly RoundUpBetweenRequest DefaultRequest = new()
    {
        MinTransactionTimestamp = DateTime.Parse( "2024-05-11T00:00:00.000Z"),
        MaxTransactionTimestamp = DateTime.Parse("2024-05-12T00:00:00.000Z"),
        AccountCurrency = RoundUpBetweenRequest.AccountCurrencyTypes.GBP,
        SavingsGoalRequest = new SavingsGoalRequest
        {
            SavingsGoalName = "test"
        }
    };

    public AccountServiceTests()
    {
        _mockApiHelper = new Mock<IApiHelper>();
        _accountService = new AccountService(_mockApiHelper.Object);
    }
    
    #region RequestValidation
    [Fact]
    public async Task RoundUp_WhenRequestMinTimeGreaterThanMaxTime_ReturnBadRequest()
    {
        Setup();
        
        var request = new RoundUpBetweenRequest {
            MinTransactionTimestamp = DateTime.Parse( "2024-05-11T00:00:00.000Z"),
            MaxTransactionTimestamp = DateTime.Parse("2024-05-11T00:00:00.000Z"),
            AccountCurrency = RoundUpBetweenRequest.AccountCurrencyTypes.GBP,
            SavingsGoalRequest = new SavingsGoalRequest
            {
                SavingsGoalName = "test"
            }
        };
        
        var expected = new BadRequestObjectResult(new ErrorResponse {ErrorDetail = [new ErrorDetail{Message = "Min timestamp must be before max timestamp"}
            ]
        });
        
        var result = await _accountService.RoundUp(request);

        result.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task RoundUp_WhenRequestBase64PhotoInValid_ReturnBadRequest()
    {
        Setup();
        
        var request = new RoundUpBetweenRequest {
            MinTransactionTimestamp = DateTime.Parse( "2024-05-11T00:00:00.000Z"),
            MaxTransactionTimestamp = DateTime.Parse("2024-05-12T00:00:00.000Z"),
            AccountCurrency = RoundUpBetweenRequest.AccountCurrencyTypes.GBP,
            SavingsGoalRequest = new SavingsGoalRequest
            {
                SavingsGoalName = "test",
                Base64EncodedPhoto = "****"
            }
        };
        
        var expected = new BadRequestObjectResult(new ErrorResponse {ErrorDetail = [new ErrorDetail{Message = "Invalid image"}
            ]
        });
        
        var result = await _accountService.RoundUp(request);

        result.Should().BeEquivalentTo(expected);
    }
    #endregion

    #region GetAccounts
    [Fact]
    public async Task RoundUp_CallsGetAccounts()
    {
        Setup();
        
        await _accountService.RoundUp(DefaultRequest);
        _mockApiHelper.Verify(x => x.GetAccountsAsync(), Times.Once);
    }

    [Fact]
    public async Task RoundUp_WhenGetAccountsReturnsNull_ReturnBadRequest()
    {
        var errorResponse = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "Internal Server Error",
            Detail = "Error occured getting accounts"
        };
        var expected = new ObjectResult(errorResponse)
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };
        
        var result = await _accountService.RoundUp(DefaultRequest);

        result.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task RoundUp_WhenNoAccountsWithRequestedCurrencyType_ReturnBadRequest()
    {
        _mockApiHelper.Setup(x => x.GetAccountsAsync()).ReturnsAsync(new Account{Accounts = [new Accounts{Currency = "EUR"}
            ]
        });
        var expected = new BadRequestObjectResult(new ErrorResponse {ErrorDetail = [new ErrorDetail{Message = $"Account with currency type: {DefaultRequest.AccountCurrency} not found"}
        ], Success = false});
        
        var result = await _accountService.RoundUp(DefaultRequest);

        result.Should().BeEquivalentTo(expected);
    }
    #endregion

    #region GetFeedItems 
    [Fact]
    public async Task RoundUp_CallsGetSettledTransactionsBetween()
    {
        Setup();
        
        await _accountService.RoundUp(DefaultRequest);
        _mockApiHelper.Verify(x => x.GetSettledTransactionsBetweenAsync(It.IsAny<string>(), DefaultRequest.MinTransactionTimestamp.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"), DefaultRequest.MaxTransactionTimestamp.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'")), Times.Once);
    }
    
    [Fact]
    public async Task RoundUp_WhenGetSettledTransactionsBetweenReturnsNull_ReturnBadRequest()
    {
        Setup();
        _mockApiHelper
            .Setup(x => x.GetSettledTransactionsBetweenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((Feed?)null);
        
        var errorResponse = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "Internal Server Error",
            Detail = "Error occured getting feed items"
        };
        var expected = new ObjectResult(errorResponse)
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };
        var result = await _accountService.RoundUp(DefaultRequest);

        result.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task RoundUp_WhenNoSettledTransactionsOccur_CreateSavingsAccountWithEmptyBalance()
    {
        Setup();
        _mockApiHelper
            .Setup(x => x.GetSettledTransactionsBetweenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Feed{FeedItems = []});
        var expected = new RoundUpBetweenResponse
        {
            SavingsGoalUid = "123",
            Balance = new CurrencyAndAmount{Currency = DefaultRequest.AccountCurrency.ToString(), MinorUnits = 0},
            Success = true
        };
        var result = await _accountService.RoundUp(DefaultRequest);

        _mockApiHelper.Verify(x => x.PutSavingsGoalsAsync(It.IsAny<string>(), It.IsAny<SavingsGoalRequestV2>()), Times.Once);
        _mockApiHelper.Verify(x => x.PutMoneySavingsGoalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopUpRequestV2>()), Times.Never);
        result.Should().BeEquivalentTo(new OkObjectResult(expected));
    }
    
    [Fact]
    public async Task RoundUp_WhenSettledTransactionsOccurForDefaultCategoryId_CreateSavingsAccountWithExpectedBalance()
    {
        Setup();
        await _accountService.RoundUp(DefaultRequest);
        
        _mockApiHelper.Verify(x => x.PutMoneySavingsGoalAsync(It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>(), It.Is<TopUpRequestV2>(topUpRequestV2 => topUpRequestV2.CurrencyAndAmount.MinorUnits == 76)), Times.Once);
    }
    #endregion

    #region CreateSavingsGoal
    [Fact]
    public async Task RoundUp_CallsPutSavingsGoals()
    {
        Setup();
        
        await _accountService.RoundUp(DefaultRequest);
        _mockApiHelper.Verify(x => x.PutSavingsGoalsAsync(It.IsAny<string>(), It.Is<SavingsGoalRequestV2>(v2 => v2.SavingsGoalName == DefaultRequest.SavingsGoalRequest.SavingsGoalName && v2.Currency == DefaultRequest.AccountCurrency.ToString() && v2.Base64EncodedPhoto == DefaultRequest.SavingsGoalRequest.Base64EncodedPhoto)), Times.Once);
    }
    
    [Fact]
    public async Task RoundUp_WhenPutSavingsGoalsIsNull_ReturnHttpBadRequest()
    {
        Setup();

        _mockApiHelper.Setup(x => x.PutSavingsGoalsAsync(It.IsAny<string>(), It.IsAny<SavingsGoalRequestV2>()))
            .ReturnsAsync((CreateOrUpdateSavingsGoalResponseV2?)null);
        
        var errorResponse = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "Internal Server Error",
            Detail = "Error occured creating savings goal"
        };
        var expected = new ObjectResult(errorResponse)
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };
        var result = await _accountService.RoundUp(DefaultRequest);
        
        result.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task RoundUp_WhenPutSavingsGoalsSuccessIsFalse_ReturnHttpBadRequest()
    {
        Setup();

        _mockApiHelper.Setup(x => x.PutSavingsGoalsAsync(It.IsAny<string>(), It.IsAny<SavingsGoalRequestV2>()))
            .ReturnsAsync(new CreateOrUpdateSavingsGoalResponseV2{SavingsGoalUid = It.IsAny<string>(), Success = false});

        var errorResponse = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "Internal Server Error",
            Detail = "Error occured creating savings goal"
        };
        var expected = new ObjectResult(errorResponse)
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };
        var result = await _accountService.RoundUp(DefaultRequest);
        
        result.Should().BeEquivalentTo(expected);
    }
    #endregion

    #region AddMoneyToSavingsGoal
    [Fact]
    public async Task RoundUp_CallsPutMoneySavingsGoals()
    {
        Setup();
        
        _mockApiHelper.Setup(x => x.PutMoneySavingsGoalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopUpRequestV2>()))
            .ReturnsAsync(It.IsAny<SavingsGoalTransferResponse>());
        
        await _accountService.RoundUp(DefaultRequest);
        _mockApiHelper.Verify(x => x.PutMoneySavingsGoalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.Is<TopUpRequestV2>(v2 => v2.CurrencyAndAmount.MinorUnits == 76 && v2.CurrencyAndAmount.Currency == DefaultRequest.AccountCurrency.ToString())), Times.Once);
    }
    
    [Fact]
    public async Task RoundUp_WhenPutMoneySavingsGoalsIsNull_ReturnHttpBadRequest()
    {
        Setup();

        _mockApiHelper
            .Setup(x => x.PutMoneySavingsGoalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<TopUpRequestV2>())).ReturnsAsync((SavingsGoalTransferResponse?)null);
        
        var errorResponse = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "Internal Server Error",
            Detail = "Error occured adding money to savings goal"
        };
        var expected = new ObjectResult(errorResponse)
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };
        var result = await _accountService.RoundUp(DefaultRequest);
        
        result.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task RoundUp_WhenPutMoneySavingsGoalsSuccessIsFalse_ReturnHttpBadRequest()
    {
        Setup();

        _mockApiHelper
            .Setup(x => x.PutMoneySavingsGoalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<TopUpRequestV2>())).ReturnsAsync(new SavingsGoalTransferResponse{TransferUid = It.IsAny<string>(), Success = false});
        var errorResponse = new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "Internal Server Error",
            Detail = "Error occured adding money to savings goal"
        };
        var expected = new ObjectResult(errorResponse)
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };
        var result = await _accountService.RoundUp(DefaultRequest);
        
        result.Should().BeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task RoundUp_WhenPutMoneySavingsGoalsSuccessIsTrue_ReturnHttpOk()
    {
        Setup();
        
        var expected = new RoundUpBetweenResponse
        {
            SavingsGoalUid = "123",
            Balance = new CurrencyAndAmount{Currency = DefaultRequest.AccountCurrency.ToString() , MinorUnits = 76},
            Success = true
        };
       
        var result = await _accountService.RoundUp(DefaultRequest);
        
        result.Should().BeEquivalentTo(new OkObjectResult(expected));
    }
    #endregion
    
    private void Setup()
    {
        //round up = 76
        var feed = new Feed
        {
            FeedItems =
            [
                new FeedItems
                {
                    CategoryUid = "456",
                    Direction = "OUT",
                    CurrencyAndAmount = new CurrencyAndAmount
                    {
                        Currency = "GBP",
                        MinorUnits = 300
                    }
                },
                new FeedItems
                {
                    CategoryUid = "456",
                    Direction = "OUT",
                    CurrencyAndAmount = new CurrencyAndAmount
                    {
                        Currency = "GBP",
                        MinorUnits = 250
                    }
                },
                new FeedItems
                {
                    CategoryUid = "456",
                    Direction = "OUT",
                    CurrencyAndAmount = new CurrencyAndAmount
                    {
                        Currency = "GBP",
                        MinorUnits = 375
                    }
                },
                new FeedItems
                {
                    CategoryUid = "456",
                    Direction = "OUT",
                    CurrencyAndAmount = new CurrencyAndAmount
                    {
                        Currency = "GBP",
                        MinorUnits = 399
                    }
                },
                new FeedItems
                {
                    CategoryUid = "456",
                    Direction = "IN",
                    CurrencyAndAmount = new CurrencyAndAmount
                    {
                        Currency = "GBP",
                        MinorUnits = 399
                    }
                },
                new FeedItems
                {
                    CategoryUid = "456",
                    Direction = "OUT",
                    CurrencyAndAmount = new CurrencyAndAmount
                    {
                        Currency = "GBP",
                        MinorUnits = 399
                    },
                    AssociatedFeedRoundUp = new AssociatedFeedRoundUp
                    {
                        GoalCategoryUid = "789"
                    }
                }
            ]
        };
        
        var account = new Account
        {
            Accounts =
            [
                new Accounts
                {
                    AccountUid = "123",
                    DefaultCategory = "456",
                    Currency = "GBP"
                }
            ]
        };
        var createSavingsGoalResponse = new CreateOrUpdateSavingsGoalResponseV2
        {
            SavingsGoalUid = "123",
            Success = true
        };
        var savingsGoalTransferResponse = new SavingsGoalTransferResponse
        {
            TransferUid = "123",
            Success = true
        };

        _mockApiHelper.Setup(x => x.GetAccountsAsync()).ReturnsAsync(account);
        _mockApiHelper
            .Setup(x => x.GetSettledTransactionsBetweenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(feed);
        _mockApiHelper.Setup(x => x.PutSavingsGoalsAsync(It.IsAny<string>(), It.IsAny<SavingsGoalRequestV2>()))
            .ReturnsAsync(createSavingsGoalResponse);
        _mockApiHelper
            .Setup(x => x.PutMoneySavingsGoalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<TopUpRequestV2>())).ReturnsAsync(savingsGoalTransferResponse);
    }
}