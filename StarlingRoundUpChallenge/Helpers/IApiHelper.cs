using StarlingRoundUpChallenge.Requests;
using StarlingRoundUpChallenge.Response;

namespace StarlingRoundUpChallenge.Helpers;

public interface IApiHelper
{
    public Task<Account?> GetAccounts();

    public Task<Feed?> GetSettledTransactionsBetween(string accountUid, string minTransactionTimestamp,
        string maxTransactionTimestamp);

    public Task<CreateOrUpdateSavingsGoalResponseV2?> PutSavingsGoals(string accountUid,
        SavingsGoalRequestV2 savingsGoalRequestV2);

    public Task<SavingsGoalTransferResponse?> PutMoneySavingsGoal(string accountUid, string savingsGoalUid,
        string transferUid, TopUpRequestV2 topUpRequestV2);
}