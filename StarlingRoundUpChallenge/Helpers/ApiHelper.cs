using System.Text;
using System.Text.Json;
using System.Web;
using StarlingRoundUpChallenge.Models.StarlingApi;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace StarlingRoundUpChallenge.Helpers;

public class ApiHelper(HttpClient client, ILogger<ApiHelper> logger) : IApiHelper
{
    /// <remarks>
    /// ApiHelper logs all errors from api calls and returns null to prevent leaking sensitive information to the user 
    /// </remarks>
    /// 
    private readonly  JsonSerializerOptions _serializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public async Task<Account?> GetAccountsAsync()
    {
        using var response = await client.GetAsync("accounts");
        
        if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<Account>(_serializeOptions);
        
        var errorMessage = GetErrorMessage(response);
        logger.LogError($"Error unable to get accounts. {errorMessage}");
        return null;
    }

    public async Task<Feed?> GetSettledTransactionsBetweenAsync(string accountUid, string minTransactionTimestamp, string maxTransactionTimestamp)
    {
        var builder = new UriBuilder($"{client.BaseAddress}feed/account/{accountUid}/settled-transactions-between");
        var query = HttpUtility.ParseQueryString(builder.Query);
        query["minTransactionTimestamp"] = minTransactionTimestamp;
        query["maxTransactionTimestamp"] = maxTransactionTimestamp;
        builder.Query = query.ToString();
        
        using var response = await client.GetAsync(builder.ToString());
        if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<Feed>(_serializeOptions);

        var errorMessage = GetErrorMessage(response);
        logger.LogError($"Error unable to get settled transaction between {minTransactionTimestamp} and {maxTransactionTimestamp}. {errorMessage}");
        return null;
    }
    
    public async Task<CreateOrUpdateSavingsGoalResponseV2?> PutSavingsGoalsAsync(string accountUid, SavingsGoalRequestV2 savingsGoalRequestV2)
    {
        var content = new StringContent(JsonSerializer.Serialize(savingsGoalRequestV2, _serializeOptions), Encoding.UTF8,
            "application/json");
        
        using var response = await client.PutAsync($"account/{accountUid}/savings-goals", content);
        if (response.IsSuccessStatusCode)
        {
            return response.Content.ReadFromJsonAsync<CreateOrUpdateSavingsGoalResponseV2>(_serializeOptions).Result;
        }

        var errorMessage = GetErrorMessage(response); 
        logger.LogError($"Error unable to create savings goal. {errorMessage}");
        return null;
    }

    public async Task<SavingsGoalTransferResponse?> PutMoneySavingsGoalAsync(string accountUid, string savingsGoalUid, string transferUid, TopUpRequestV2 topUpRequestV2)
    {
        var content = new StringContent(JsonSerializer.Serialize(topUpRequestV2, _serializeOptions), Encoding.UTF8,
            "application/json");
        
        using var response = await client.PutAsync($"account/{accountUid}/savings-goals/{savingsGoalUid}/add-money/{transferUid}", content);
        if (response.IsSuccessStatusCode)
        {
            return response.Content.ReadFromJsonAsync<SavingsGoalTransferResponse>(_serializeOptions).Result;
        }

        var errorMessage = GetErrorMessage(response);
        logger.LogError($"Error adding money to savings goal. {errorMessage}");
        return null;
    }

    private static string GetErrorMessage(HttpResponseMessage response)
    {
        try
        {
            var jsonErrorResponse = response.Content.ReadFromJsonAsync<ErrorResponse>().Result;
            if (jsonErrorResponse is { Success: true } && jsonErrorResponse.ErrorDetail.FirstOrDefault() != null)
            {
                var errorMessage = jsonErrorResponse.ErrorDetail.FirstOrDefault()?.Message;
                if (errorMessage != null) return errorMessage;
            }
        }
        catch
        {
            var stringErrorResponse = response.Content.ReadAsStringAsync().Result;
            if (stringErrorResponse.Length > 0)
            {
                return stringErrorResponse;   
            }
        }

        return $"StatusCode: {response.StatusCode.GetHashCode()}, ReasonPhrase: {response.ReasonPhrase}";
    }
}