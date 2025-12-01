using AutoMapper;
using backend.Common;
using backend.Common.Enums;
using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Modelss;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.Extensions.Logging;

namespace backend.Services;

/// <summary>
/// Loyalty account service implementation
/// </summary>
public class LoyaltyAccountService : ILoyaltyAccountService
{
    private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
    private readonly ILoyaltyTransactionRepository _loyaltyTransactionRepository;
    private readonly ILoyaltyRedemptionRepository _loyaltyRedemptionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<LoyaltyAccountService> _logger;

    // Tier thresholds based on points
    private const long BRONZE_THRESHOLD = 0;
    private const long SILVER_THRESHOLD = 100;
    private const long GOLD_THRESHOLD = 500;
    private const long PLATINUM_THRESHOLD = 1000;

    public LoyaltyAccountService(
        ILoyaltyAccountRepository loyaltyAccountRepository,
        ILoyaltyTransactionRepository loyaltyTransactionRepository,
        ILoyaltyRedemptionRepository loyaltyRedemptionRepository,
        IMapper mapper,
        ILogger<LoyaltyAccountService> logger)
    {
        _loyaltyAccountRepository = loyaltyAccountRepository;
        _loyaltyTransactionRepository = loyaltyTransactionRepository;
        _loyaltyRedemptionRepository = loyaltyRedemptionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Calculate tier based on points balance
    /// </summary>
    private LoyaltyTier CalculateTier(long pointsBalance)
    {
        if (pointsBalance >= PLATINUM_THRESHOLD) return LoyaltyTier.Platinum;
        if (pointsBalance >= GOLD_THRESHOLD) return LoyaltyTier.Gold;
        if (pointsBalance >= SILVER_THRESHOLD) return LoyaltyTier.Silver;
        return LoyaltyTier.Bronze;
    }

    /// <summary>
    /// Update account tier based on current points balance
    /// </summary>
    private void UpdateAccountTier(LoyaltyAccount account)
    {
        var newTier = CalculateTier(account.PointsBalance);
        if (account.Tier != newTier)
        {
            var oldTier = account.Tier;
            account.Tier = newTier;
            account.UpdatedAt = DateTime.UtcNow;
            _logger.LogInformation("Updated tier for account {AccountId} from {OldTier} to {NewTier} (Points: {Points})", 
                account.Id, oldTier, newTier, account.PointsBalance);
        }
    }

    public async Task<Result<LoyaltyAccountResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _loyaltyAccountRepository.GetByIdAsync(id, cancellationToken);
            if (account == null)
            {
                return Result<LoyaltyAccountResponse>.Failure($"Loyalty account with ID {id} not found");
            }

            var response = _mapper.Map<LoyaltyAccountResponse>(account);
            return Result<LoyaltyAccountResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loyalty account by ID {AccountId}", id);
            return Result<LoyaltyAccountResponse>.Failure($"Error retrieving loyalty account: {ex.Message}");
        }
    }

    public async Task<Result<LoyaltyAccountResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _loyaltyAccountRepository.GetByUserIdAsync(userId, cancellationToken);
            if (account == null)
            {
                return Result<LoyaltyAccountResponse>.Failure($"Loyalty account for user {userId} not found");
            }

            var response = _mapper.Map<LoyaltyAccountResponse>(account);
            return Result<LoyaltyAccountResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loyalty account by user ID {UserId}", userId);
            return Result<LoyaltyAccountResponse>.Failure($"Error retrieving loyalty account: {ex.Message}");
        }
    }

    public async Task<Result<LoyaltyAccountDetailsResponse>> GetAccountDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _loyaltyAccountRepository.GetByUserIdWithAllDataAsync(userId, cancellationToken);
            if (account == null)
            {
                return Result<LoyaltyAccountDetailsResponse>.Failure($"Loyalty account for user {userId} not found");
            }

            var totalPointsEarned = account.LoyaltyTransactions
                .Where(t => t.ChangeAmount > 0)
                .Sum(t => t.ChangeAmount);

            var totalPointsRedeemed = await _loyaltyRedemptionRepository.GetTotalPointsRedeemedAsync(account.Id, cancellationToken);

            var details = new LoyaltyAccountDetailsResponse
            {
                Account = _mapper.Map<LoyaltyAccountResponse>(account),
                RecentTransactions = _mapper.Map<IEnumerable<LoyaltyTransactionResponse>>(account.LoyaltyTransactions.Take(10)),
                RecentRedemptions = _mapper.Map<IEnumerable<LoyaltyRedemptionResponse>>(account.LoyaltyRedemptions.Take(10)),
                TotalPointsEarned = totalPointsEarned,
                TotalPointsRedeemed = totalPointsRedeemed
            };

            return Result<LoyaltyAccountDetailsResponse>.Success(details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loyalty account details for user {UserId}", userId);
            return Result<LoyaltyAccountDetailsResponse>.Failure($"Error retrieving account details: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<LoyaltyAccountResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var accounts = await _loyaltyAccountRepository.GetAllAsync(cancellationToken);
            var responses = _mapper.Map<IEnumerable<LoyaltyAccountResponse>>(accounts);
            return Result<IEnumerable<LoyaltyAccountResponse>>.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all loyalty accounts");
            return Result<IEnumerable<LoyaltyAccountResponse>>.Failure($"Error retrieving accounts: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<LoyaltyAccountResponse>>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var accounts = await _loyaltyAccountRepository.GetActiveAccountsAsync(cancellationToken);
            var responses = _mapper.Map<IEnumerable<LoyaltyAccountResponse>>(accounts);
            return Result<IEnumerable<LoyaltyAccountResponse>>.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active loyalty accounts");
            return Result<IEnumerable<LoyaltyAccountResponse>>.Failure($"Error retrieving active accounts: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<LoyaltyAccountResponse>>> GetByTierAsync(int tier, CancellationToken cancellationToken = default)
    {
        try
        {
            var accounts = await _loyaltyAccountRepository.GetByTierAsync(tier, cancellationToken);
            var responses = _mapper.Map<IEnumerable<LoyaltyAccountResponse>>(accounts);
            return Result<IEnumerable<LoyaltyAccountResponse>>.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting loyalty accounts by tier {Tier}", tier);
            return Result<IEnumerable<LoyaltyAccountResponse>>.Failure($"Error retrieving accounts: {ex.Message}");
        }
    }

    public async Task<Result<LoyaltyAccountResponse>> CreateAsync(CreateLoyaltyAccountRequest createRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            // Fast path: avoid creating duplicate accounts if one already exists
            var exists = await _loyaltyAccountRepository.UserHasAccountAsync(createRequest.UserId, cancellationToken);
            if (exists)
            {
                var existing = await _loyaltyAccountRepository.GetByUserIdAsync(createRequest.UserId, cancellationToken);
                if (existing != null)
                {
                    var existingResponse = _mapper.Map<LoyaltyAccountResponse>(existing);
                    return Result<LoyaltyAccountResponse>.Success(existingResponse);
                }
            }

            var account = _mapper.Map<LoyaltyAccount>(createRequest);
            account.CreatedAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _loyaltyAccountRepository.AddAsync(account, cancellationToken);
            }
            catch (DbUpdateException dbEx) when (dbEx.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                // Unique constraint violation on user_id – another request created the account in parallel.
                _logger.LogWarning(pgEx, "Loyalty account already exists for user {UserId}, returning existing account", createRequest.UserId);

                var existing = await _loyaltyAccountRepository.GetByUserIdAsync(createRequest.UserId, cancellationToken);
                if (existing != null)
                {
                    var existingResponse = _mapper.Map<LoyaltyAccountResponse>(existing);
                    return Result<LoyaltyAccountResponse>.Success(existingResponse);
                }

                // If we still can't find it, surface a friendly error
                return Result<LoyaltyAccountResponse>.Failure("Loyalty account already exists but could not be loaded.");
            }

            var response = _mapper.Map<LoyaltyAccountResponse>(account);
            _logger.LogInformation("Created loyalty account for user {UserId}", account.UserId);
            return Result<LoyaltyAccountResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating loyalty account");
            return Result<LoyaltyAccountResponse>.Failure($"Error creating account: {ex.Message}");
        }
    }

    public async Task<Result<LoyaltyAccountResponse>> UpdateAsync(Guid id, UpdateLoyaltyAccountRequest updateRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _loyaltyAccountRepository.GetByIdAsync(id, cancellationToken);
            if (account == null)
            {
                return Result<LoyaltyAccountResponse>.Failure($"Loyalty account with ID {id} not found");
            }

            _mapper.Map(updateRequest, account);
            account.UpdatedAt = DateTime.UtcNow;

            await _loyaltyAccountRepository.UpdateAsync(account, cancellationToken);

            var response = _mapper.Map<LoyaltyAccountResponse>(account);
            _logger.LogInformation("Updated loyalty account {AccountId}", id);
            return Result<LoyaltyAccountResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating loyalty account {AccountId}", id);
            return Result<LoyaltyAccountResponse>.Failure($"Error updating account: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _loyaltyAccountRepository.GetByIdAsync(id, cancellationToken);
            if (account == null)
            {
                return Result.Failure($"Loyalty account with ID {id} not found");
            }

            await _loyaltyAccountRepository.DeleteAsync(account, cancellationToken);

            _logger.LogInformation("Deleted loyalty account {AccountId}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting loyalty account {AccountId}", id);
            return Result.Failure($"Error deleting account: {ex.Message}");
        }
    }

    public async Task<Result<LoyaltyAccountResponse>> AddPointsAsync(AddPointsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _loyaltyAccountRepository.AddPointsAsync(
                request.UserId, 
                request.Points, 
                request.Reason, 
                request.ReferenceId, 
                cancellationToken);

            if (!success)
            {
                return Result<LoyaltyAccountResponse>.Failure($"Failed to add points for user {request.UserId}");
            }

            // Get updated account and recalculate tier
            var account = await _loyaltyAccountRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (account != null)
            {
                UpdateAccountTier(account);
                await _loyaltyAccountRepository.UpdateAsync(account, cancellationToken);
            }


            var response = _mapper.Map<LoyaltyAccountResponse>(account!);

            _logger.LogInformation("Added {Points} points to user {UserId}", request.Points, request.UserId);
            return Result<LoyaltyAccountResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding points for user {UserId}", request.UserId);
            return Result<LoyaltyAccountResponse>.Failure($"Error adding points: {ex.Message}");
        }
    }

    public async Task<Result<LoyaltyRedemptionResponse>> RedeemPointsAsync(RedeemPointsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _loyaltyAccountRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (account == null)
            {
                return Result<LoyaltyRedemptionResponse>.Failure($"Loyalty account for user {request.UserId} not found");
            }

            if (account.PointsBalance < request.Points)
            {
                return Result<LoyaltyRedemptionResponse>.Failure($"Insufficient points. Available: {account.PointsBalance}, Required: {request.Points}");
            }

            // Deduct points
            var success = await _loyaltyAccountRepository.DeductPointsAsync(
                request.UserId,
                request.Points,
                $"Redemption: {request.RewardType}",
                null,
                cancellationToken);

            if (!success)
            {
                return Result<LoyaltyRedemptionResponse>.Failure("Failed to deduct points");
            }

            // Get updated account and recalculate tier
            account = await _loyaltyAccountRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (account != null)
            {
                UpdateAccountTier(account);
                await _loyaltyAccountRepository.UpdateAsync(account, cancellationToken);
            }

            // Create redemption record
            var redemption = new LoyaltyRedemption
            {
                LoyaltyAccountId = account!.Id,
                PointsUsed = request.Points,
                RewardType = request.RewardType,
                RewardMetadata = request.RewardMetadata ?? "{}",
                CreatedAt = DateTime.UtcNow
            };

            await _loyaltyRedemptionRepository.AddAsync(redemption, cancellationToken);

            var response = _mapper.Map<LoyaltyRedemptionResponse>(redemption);
            _logger.LogInformation("Redeemed {Points} points for user {UserId} - {RewardType}", request.Points, request.UserId, request.RewardType);
            return Result<LoyaltyRedemptionResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error redeeming points for user {UserId}", request.UserId);
            return Result<LoyaltyRedemptionResponse>.Failure($"Error redeeming points: {ex.Message}");
        }
    }

    public async Task<Result<long>> GetPointsBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _loyaltyAccountRepository.GetByUserIdAsync(userId, cancellationToken);
            if (account == null)
            {
                return Result<long>.Failure($"Loyalty account for user {userId} not found");
            }

            return Result<long>.Success(account.PointsBalance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting points balance for user {UserId}", userId);
            return Result<long>.Failure($"Error getting balance: {ex.Message}");
        }
    }
}
