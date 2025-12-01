using backend_monolith.Common;
using backend_monolith.DTOs.Request;
using backend_monolith.DTOs.Response;

namespace backend_monolith.Services.Interfaces;

/// <summary>
/// Loyalty account service interface
/// </summary>
public interface ILoyaltyAccountService
{
    Task<Result<LoyaltyAccountResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<LoyaltyAccountResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<LoyaltyAccountDetailsResponse>> GetAccountDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<LoyaltyAccountResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<LoyaltyAccountResponse>>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<LoyaltyAccountResponse>>> GetByTierAsync(int tier, CancellationToken cancellationToken = default);
    Task<Result<LoyaltyAccountResponse>> CreateAsync(CreateLoyaltyAccountRequest createRequest, CancellationToken cancellationToken = default);
    Task<Result<LoyaltyAccountResponse>> UpdateAsync(Guid id, UpdateLoyaltyAccountRequest updateRequest, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<LoyaltyAccountResponse>> AddPointsAsync(AddPointsRequest request, CancellationToken cancellationToken = default);
    Task<Result<LoyaltyRedemptionResponse>> RedeemPointsAsync(RedeemPointsRequest request, CancellationToken cancellationToken = default);
    Task<Result<long>> GetPointsBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
}
