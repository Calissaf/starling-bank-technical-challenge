using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using StarlingRoundUpChallenge.Helpers;
using StarlingRoundUpChallenge.Models.StarlingApi;
using StarlingRoundUpChallengeTests.Mocks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace StarlingRoundUpChallengeTests.Helpers;

public class ApiHelperTests
{
    private Mock<ILogger<ApiHelper>> _mockLogger;
    private HttpMessageHandlerMock _handlerMock;
    private ApiHelper _apiHelper;
    private readonly  JsonSerializerOptions _serializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    [SetUp]
    internal void SetUp(HttpStatusCode code, string jsonResponse)
    {
        _mockLogger = new Mock<ILogger<ApiHelper>>();
        _handlerMock = new HttpMessageHandlerMock(code, jsonResponse);
        var httpClient = new HttpClient(_handlerMock)
        {
            BaseAddress = new Uri("https://api-sandbox.starlingbank.com/api/v2/")
        };
        _apiHelper = new ApiHelper(httpClient, _mockLogger.Object);
    }

    #region GetAccountAsync
    [Fact]
    public async Task GetAccount_WhenRequestIsSuccessful_ReturnAccountResponse()
    { 
        var expected = new Account
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
        var jsonResponse = JsonSerializer.Serialize(expected, _serializeOptions);
        SetUp(HttpStatusCode.OK, jsonResponse);
        
        var result = await _apiHelper.GetAccountsAsync();
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAccount_WhenRequestReturnsError_CallLoggerAndReturnNull()
    {
        var expected = new ErrorResponse{
            ErrorDetail =
            [
                new ErrorDetail
                {
                    Message = "Unable to get accounts"
                }
            ],
            Success = false
        };
        var jsonResponse = JsonSerializer.Serialize(expected, _serializeOptions);
        SetUp(HttpStatusCode.BadRequest, jsonResponse);
        
        var result = await _apiHelper.GetAccountsAsync();
        
        result.Should().BeNull();
        _mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, type) => @object.ToString().Contains("Error unable to get accounts") && type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    #endregion

    #region GetSettledTransactionsBetweenAsync
    [Fact]
    public async Task GetSettledTransactionsBetween_WhenRequestIsSuccessful_ReturnFeed()
    {
        var expected = new Feed
        {
            FeedItems =
            [
                new FeedItems
                {
                    CategoryUid = "123",
                    CurrencyAndAmount = new CurrencyAndAmount
                    {
                        Currency = "GBP",
                        MinorUnits = 10
                    },
                    Direction = "OUT",
                    FeedItemUid = "456"
                }
            ]
        };
        var jsonResponse = JsonSerializer.Serialize(expected, _serializeOptions);
        SetUp(HttpStatusCode.OK, jsonResponse);
        
        var result = await _apiHelper.GetSettledTransactionsBetweenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetSettledTransactionsBetween_WhenRequestReturnsError_CallLoggerAndReturnNull()
    {
        var expected = new ErrorResponse{
            ErrorDetail =
            [
                new ErrorDetail
                {
                    Message = "Unable to get feed"
                }
            ],
            Success = false
        };
        var jsonResponse = JsonSerializer.Serialize(expected, _serializeOptions);
        SetUp(HttpStatusCode.BadRequest, jsonResponse);
        
        var result = await _apiHelper.GetSettledTransactionsBetweenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
        
        result.Should().BeNull();
        _mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, type) => @object.ToString().Contains("Error unable to get settled transaction between") && type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    #endregion

    #region PutSavingsGoalsAsync
    [Fact]
    public async Task PutSavingsGoals_WhenRequestIsSuccessful_ReturnCreateSavingsGoalResponse()
    {
        var expected = new CreateOrUpdateSavingsGoalResponseV2
        {
            SavingsGoalUid = "123",
            Success = true
        };
        var jsonResponse = JsonSerializer.Serialize(expected, _serializeOptions);
        SetUp(HttpStatusCode.OK, jsonResponse);
        
        var result = await _apiHelper.PutSavingsGoalsAsync(It.IsAny<string>(), It.IsAny<SavingsGoalRequestV2>());
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task PutSavingsGoals_WhenRequestReturnsError_CallLoggerAndReturnNull()
    {
        var expected = new ErrorResponse{
            ErrorDetail =
            [
                new ErrorDetail
                {
                    Message = "Unable to create savings goal"
                }
            ],
            Success = false
        };
        var jsonResponse = JsonSerializer.Serialize(expected, _serializeOptions);
        SetUp(HttpStatusCode.BadRequest, jsonResponse);
        
        var result = await _apiHelper.PutSavingsGoalsAsync(It.IsAny<string>(), It.IsAny<SavingsGoalRequestV2>());
        
        result.Should().BeNull();
        _mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, type) => @object.ToString().Contains("Error unable to create savings goal") && type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    #endregion

    #region PutMoneySavingsGoalAsync
    [Fact]
    public async Task PutMoneySavingsGoal_WhenRequestIsSuccessful_ReturnAccountResponse()
    {
        var expected = new SavingsGoalTransferResponse
        {
            TransferUid = "123",
            Success = true
        };
        var jsonResponse = JsonSerializer.Serialize(expected, _serializeOptions);
        SetUp(HttpStatusCode.OK, jsonResponse);
        
        var result = await _apiHelper.PutMoneySavingsGoalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopUpRequestV2>());
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task PutMoneySavingsGoal_WhenRequestReturnsError_CallLoggerAndReturnNull()
    {
        var expected = new ErrorResponse{
            ErrorDetail =
            [
                new ErrorDetail
                {
                    Message = "Error adding money to savings goal"
                }
            ],
            Success = false
        };
        var jsonResponse = JsonSerializer.Serialize(expected, _serializeOptions);
        SetUp(HttpStatusCode.BadRequest, jsonResponse);
        
        var result = await _apiHelper.PutMoneySavingsGoalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopUpRequestV2>());
        
        result.Should().BeNull();
        _mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, type) => @object.ToString().Contains("Error adding money to savings goal") && type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    #endregion
}