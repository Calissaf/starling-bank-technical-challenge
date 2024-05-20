using System.Text;
using System.Web;
using Newtonsoft.Json;
using StarlingRoundUpChallenge.Requests;
using StarlingRoundUpChallenge.Response;

namespace StarlingRoundUpChallenge.Helpers;

public class ApiHelper : IApiHelper
{
    /*
     * should
     *  - call starling api
     *  - get accounts +
     *  - get settled transactions between timestamps +
     *  - create savings goal
     *  - add money to savings goal
     */

    private readonly HttpClient _client;
    private readonly ILogger<ApiHelper> _logger;

    public ApiHelper(ILogger<ApiHelper> logger, HttpClient client)
    {
        _logger = logger;
        _client = client;
    }
    
    public async Task<Account?> GetAccounts()
    {
        using var response = await _client.GetAsync("accounts");
        
        if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<Account>();
        
        var errorMessage = GetErrorMessage(response);
        _logger.LogError($"Error unable to get accounts. {errorMessage}");
        return null;
    }

    public async Task<Feed?> GetSettledTransactionsBetween(string accountUid, string minTransactionTimestamp, string maxTransactionTimestamp)
    {
        var builder = new UriBuilder($"{_client.BaseAddress}feed/account/{accountUid}/settled-transactions-between");
        var query = HttpUtility.ParseQueryString(builder.Query);
        query["minTransactionTimestamp"] = minTransactionTimestamp;
        query["maxTransactionTimestamp"] = maxTransactionTimestamp;
        builder.Query = query.ToString();
        
        using var response = await _client.GetAsync(builder.ToString());
        if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<Feed>();

        var errorMessage = GetErrorMessage(response);
        _logger.LogError($"Error unable to get settled transaction between {minTransactionTimestamp} and {maxTransactionTimestamp}. {errorMessage}");
        return null;
    }
    
    public async Task<CreateOrUpdateSavingsGoalResponseV2?> PutSavingsGoals(string accountUid, SavingsGoalRequestV2 savingsGoalRequestV2)
    {
        var content = new StringContent(JsonConvert.SerializeObject(savingsGoalRequestV2), Encoding.UTF8,
            "application/json");
        
        using var response = await _client.PutAsync($"account/{accountUid}/savings-goals", content);
        if (response.IsSuccessStatusCode)
        {
            return response.Content.ReadFromJsonAsync<CreateOrUpdateSavingsGoalResponseV2>().Result;
        }

        var errorMessage = GetErrorMessage(response); 
        _logger.LogError($"Error unable to create savings goal. {errorMessage}");
        return null;
    }

    public async Task<SavingsGoalTransferResponse?> PutMoneySavingsGoal(string accountUid, string savingsGoalUid, string transferUid, TopUpRequestV2 topUpRequestV2)
    {
        var content = new StringContent(JsonConvert.SerializeObject(topUpRequestV2), Encoding.UTF8,
            "application/json");
        
        using var response = await _client.PutAsync($"account/{accountUid}/savings-goals/{savingsGoalUid}/add-money/{transferUid}", content);
        if (response.IsSuccessStatusCode)
        {
            return response.Content.ReadFromJsonAsync<SavingsGoalTransferResponse>().Result;
        }

        var errorMessage = GetErrorMessage(response);
        _logger.LogError($"Error adding money to savings goal. {errorMessage}");
        return null;
    }

    private static string GetErrorMessage(HttpResponseMessage response)
    {
        try
        {
            var jsonErrorResponse = response.Content.ReadFromJsonAsync<ErrorResponse>().Result;
            if (jsonErrorResponse is { success: true } && jsonErrorResponse.ErrorDetail.FirstOrDefault() != null)
            {
                return jsonErrorResponse.ErrorDetail.FirstOrDefault()!.message;
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