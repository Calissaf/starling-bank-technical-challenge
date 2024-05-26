using StarlingRoundUpChallenge.Models.StarlingApi;

namespace StarlingRoundUpChallenge.Helpers;

public interface IApiHelper
{
    public Task<Account?> GetAccountsAsync();

    public Task<Feed?> GetSettledTransactionsBetweenAsync(string accountUid, string minTransactionTimestamp,
        string maxTransactionTimestamp);

    public Task<CreateOrUpdateSavingsGoalResponseV2?> PutSavingsGoalsAsync(string accountUid,
        SavingsGoalRequestV2 savingsGoalRequestV2);

    public Task<SavingsGoalTransferResponse?> PutMoneySavingsGoalAsync(string accountUid, string savingsGoalUid,
        string transferUid, TopUpRequestV2 topUpRequestV2);
}