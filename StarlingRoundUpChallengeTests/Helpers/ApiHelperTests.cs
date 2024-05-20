using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using StarlingRoundUpChallenge.Helpers;
using StarlingRoundUpChallenge.Requests;
using StarlingRoundUpChallenge.Response;
using StarlingRoundUpChallengeTests.Mocks;

namespace StarlingRoundUpChallengeTests.Helpers;

public class ApiHelperTests
{
    private Mock<ILogger<ApiHelper>> _mockLogger;
    private HttpMessageHandlerMock _handlerMock;
    private ApiHelper _apiHelper;

    [SetUp]
    internal void SetUp(HttpStatusCode code, string jsonResponse)
    {
        _mockLogger = new Mock<ILogger<ApiHelper>>();
        _handlerMock = new HttpMessageHandlerMock(code, jsonResponse);
        var httpClient = new HttpClient(_handlerMock)
        {
            BaseAddress = new Uri("https://api-sandbox.starlingbank.com/api/v2/")
        };
        _apiHelper = new ApiHelper(_mockLogger.Object, httpClient);
    }
    
    //get account async
    /*
     * when call is successful return account // 
     * when call returns error call logger and return null //
     */
    [Fact]
    public async Task GetAccount_WhenRequestIsSuccessful_ReturnAccountResponse()
    { 
        var expected = new Account
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
        var jsonResponse = JsonConvert.SerializeObject(expected);
        SetUp(HttpStatusCode.OK, jsonResponse);
        
        var result = await _apiHelper.GetAccounts();
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAccount_WhenRequestReturnsError_CallLoggerAndReturnNull()
    {
        var expected = new ErrorResponse{
            ErrorDetail = new []
            {
                new ErrorDetail
                {
                    message = "Unable to get accounts"
                }
            },
            success = false
        };
        var jsonResponse = JsonConvert.SerializeObject(expected);
        SetUp(HttpStatusCode.BadRequest, jsonResponse);
        
        var result = await _apiHelper.GetAccounts();
        
        result.Should().BeNull();
        _mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString().Contains("Error unable to get accounts") && @type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    
    //get settled transactions between
    /*
     * when call is successful return account //
     * when call returns error call logger and return null //
     */    
    [Fact]
    public async Task GetSettledTransactionsBetween_WhenRequestIsSuccessful_ReturnFeed()
    {
        var expected = new Feed
        {
            feedItems = new[]
            {
                new FeedItems
                {
                    categoryUid = "123",
                    currencyAndAmount = new CurrencyAndAmount
                    {
                        currency = "GBP",
                        minorUnits = 10
                    },
                    direction = "OUT",
                    feedItemUid = "456"
                }
            }
        };
        var jsonResponse = JsonConvert.SerializeObject(expected);
        SetUp(HttpStatusCode.OK, jsonResponse);
        
        var result = await _apiHelper.GetSettledTransactionsBetween(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetSettledTransactionsBetween_WhenRequestReturnsError_CallLoggerAndReturnNull()
    {
        var expected = new ErrorResponse{
            ErrorDetail = new []
            {
                new ErrorDetail
                {
                    message = "Unable to get feed"
                }
            },
            success = false
        };
        var jsonResponse = JsonConvert.SerializeObject(expected);
        SetUp(HttpStatusCode.BadRequest, jsonResponse);
        
        var result = await _apiHelper.GetSettledTransactionsBetween(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
        
        result.Should().BeNull();
        _mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString().Contains("Error unable to get settled transaction between") && @type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    
    //PutSavingsGoals transactions between
    /*
     * when call is successful return account //
     * when call returns error call logger and return null //
     */  
    
    [Fact]
    public async Task PutSavingsGoals_WhenRequestIsSuccessful_ReturnCreateSavingsGoalResponse()
    {
        var expected = new CreateOrUpdateSavingsGoalResponseV2()
        {
            savingsGoalUid = "123",
            success = true
        };
        var jsonResponse = JsonConvert.SerializeObject(expected);
        SetUp(HttpStatusCode.OK, jsonResponse);
        
        var result = await _apiHelper.PutSavingsGoals(It.IsAny<string>(), It.IsAny<SavingsGoalRequestV2>());
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task PutSavingsGoals_WhenRequestReturnsError_CallLoggerAndReturnNull()
    {
        var expected = new ErrorResponse{
            ErrorDetail = new []
            {
                new ErrorDetail
                {
                    message = "Unable to create savings goal"
                }
            },
            success = false
        };
        var jsonResponse = JsonConvert.SerializeObject(expected);
        SetUp(HttpStatusCode.BadRequest, jsonResponse);
        
        var result = await _apiHelper.PutSavingsGoals(It.IsAny<string>(), It.IsAny<SavingsGoalRequestV2>());
        
        result.Should().BeNull();
        _mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString().Contains("Error unable to create savings goal") && @type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    
    //PutMoneySavingsGoal transactions between
    /*
     * when call is successful return account //
     * when call returns error call logger and return null //
     */  
    [Fact]
    public async Task PutMoneySavingsGoal_WhenRequestIsSuccessful_ReturnAccountResponse()
    {
        var expected = new SavingsGoalTransferResponse
        {
            transferUid = "123",
            success = true
        };
        var jsonResponse = JsonConvert.SerializeObject(expected);
        SetUp(HttpStatusCode.OK, jsonResponse);
        
        var result = await _apiHelper.PutMoneySavingsGoal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopUpRequestV2>());
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task PutMoneySavingsGoal_WhenRequestReturnsError_CallLoggerAndReturnNull()
    {
        var expected = new ErrorResponse{
            ErrorDetail = new []
            {
                new ErrorDetail
                {
                    message = "Error adding money to savings goal"
                }
            },
            success = false
        };
        var jsonResponse = JsonConvert.SerializeObject(expected);
        SetUp(HttpStatusCode.BadRequest, jsonResponse);
        
        var result = await _apiHelper.PutMoneySavingsGoal(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopUpRequestV2>());
        
        result.Should().BeNull();
        _mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString().Contains("Error adding money to savings goal") && @type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    
}