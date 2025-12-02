using AutoMapper;
using backend.Common.Enums;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services;
using backend.DTOs.Loyalty.Request;
using backend.DTOs.Loyalty.Response;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Xunit;

namespace Backend.Tests.Services;

public class LoyaltyAccountServiceTests
{
    private readonly Mock<ILoyaltyAccountRepository> _mockLoyaltyAccountRepository;
    private readonly Mock<ILoyaltyRedemptionRepository> _mockLoyaltyRedemptionRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly LoyaltyAccountService _loyaltyAccountService;

    public LoyaltyAccountServiceTests()
    {
        _mockLoyaltyAccountRepository = new Mock<ILoyaltyAccountRepository>();
        var mockLoyaltyTransactionRepository = new Mock<ILoyaltyTransactionRepository>();
        _mockLoyaltyRedemptionRepository = new Mock<ILoyaltyRedemptionRepository>();
        _mockMapper = new Mock<IMapper>();
        var mockLogger = new Mock<ILogger<LoyaltyAccountService>>();

        _loyaltyAccountService = new LoyaltyAccountService(
            _mockLoyaltyAccountRepository.Object,
            mockLoyaltyTransactionRepository.Object,
            _mockLoyaltyRedemptionRepository.Object,
            _mockMapper.Object,
            mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_AccountExists_ReturnsSuccess()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = accountId,
            UserId = Guid.NewGuid(),
            PointsBalance = 100,
            Tier = LoyaltyTier.Silver
        };

        var accountResponse = new LoyaltyAccountResponse
        {
            Id = accountId,
            UserId = account.UserId,
            PointsBalance = 100,
            Tier = LoyaltyTier.Silver
        };

        _mockLoyaltyAccountRepository.Setup(x => x.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _mockMapper.Setup(x => x.Map<LoyaltyAccountResponse>(account))
            .Returns(accountResponse);

        // Act
        var result = await _loyaltyAccountService.GetByIdAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(accountId);
        result.Data.PointsBalance.Should().Be(100);
    }

    [Fact]
    public async Task GetByIdAsync_AccountNotFound_ReturnsFailure()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mockLoyaltyAccountRepository.Setup(x => x.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoyaltyAccount?)null);

        // Act
        var result = await _loyaltyAccountService.GetByIdAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Loyalty account with ID {accountId} not found");
    }

    [Fact]
    public async Task GetByUserIdAsync_AccountExists_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 200,
            Tier = LoyaltyTier.Gold
        };

        var accountResponse = new LoyaltyAccountResponse
        {
            Id = account.Id,
            UserId = userId,
            PointsBalance = 200,
            Tier = LoyaltyTier.Gold
        };

        _mockLoyaltyAccountRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _mockMapper.Setup(x => x.Map<LoyaltyAccountResponse>(account))
            .Returns(accountResponse);

        // Act
        var result = await _loyaltyAccountService.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetAccountDetailsAsync_AccountExists_ReturnsDetailsWithTransactionsAndRedemptions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = accountId,
            UserId = userId,
            PointsBalance = 500,
            Tier = LoyaltyTier.Gold,
            LoyaltyTransactions = new List<LoyaltyTransaction>
            {
                new LoyaltyTransaction { Id = Guid.NewGuid(), LoyaltyAccountId = accountId, ChangeAmount = 100 },
                new LoyaltyTransaction { Id = Guid.NewGuid(), LoyaltyAccountId = accountId, ChangeAmount = 50 },
                new LoyaltyTransaction { Id = Guid.NewGuid(), LoyaltyAccountId = accountId, ChangeAmount = -20 }
            },
            LoyaltyRedemptions = new List<LoyaltyRedemption>
            {
                new LoyaltyRedemption { Id = Guid.NewGuid(), LoyaltyAccountId = accountId, PointsUsed = 100 }
            }
        };

        var accountResponse = new LoyaltyAccountResponse
        {
            Id = accountId,
            UserId = userId,
            PointsBalance = 500,
            Tier = LoyaltyTier.Gold
        };

        _mockLoyaltyAccountRepository.Setup(x => x.GetByUserIdWithAllDataAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _mockLoyaltyRedemptionRepository.Setup(x => x.GetTotalPointsRedeemedAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);

        _mockMapper.Setup(x => x.Map<LoyaltyAccountResponse>(account))
            .Returns(accountResponse);

        _mockMapper.Setup(x => x.Map<IEnumerable<LoyaltyTransactionResponse>>(It.IsAny<IEnumerable<LoyaltyTransaction>>()))
            .Returns(new List<LoyaltyTransactionResponse>());

        _mockMapper.Setup(x => x.Map<IEnumerable<LoyaltyRedemptionResponse>>(It.IsAny<IEnumerable<LoyaltyRedemption>>()))
            .Returns(new List<LoyaltyRedemptionResponse>());

        // Act
        var result = await _loyaltyAccountService.GetAccountDetailsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Account.Should().NotBeNull();
        result.Data.TotalPointsEarned.Should().Be(150); // 100 + 50
        result.Data.TotalPointsRedeemed.Should().Be(100);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllAccounts()
    {
        // Arrange
        var accounts = new List<LoyaltyAccount>
        {
            new LoyaltyAccount { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PointsBalance = 100 },
            new LoyaltyAccount { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PointsBalance = 200 }
        };

        var accountResponses = new List<LoyaltyAccountResponse>
        {
            new LoyaltyAccountResponse { Id = accounts[0].Id, PointsBalance = 100 },
            new LoyaltyAccountResponse { Id = accounts[1].Id, PointsBalance = 200 }
        };

        _mockLoyaltyAccountRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts);

        _mockMapper.Setup(x => x.Map<IEnumerable<LoyaltyAccountResponse>>(accounts))
            .Returns(accountResponses);

        // Act
        var result = await _loyaltyAccountService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveAccountsAsync_ReturnsActiveAccounts()
    {
        // Arrange
        var accounts = new List<LoyaltyAccount>
        {
            new LoyaltyAccount { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), IsActive = true }
        };

        var accountResponses = new List<LoyaltyAccountResponse>
        {
            new LoyaltyAccountResponse { Id = accounts[0].Id }
        };

        _mockLoyaltyAccountRepository.Setup(x => x.GetActiveAccountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts);

        _mockMapper.Setup(x => x.Map<IEnumerable<LoyaltyAccountResponse>>(accounts))
            .Returns(accountResponses);

        // Act
        var result = await _loyaltyAccountService.GetActiveAccountsAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByTierAsync_ReturnAccountsForTier()
    {
        // Arrange
        var tier = 2; // Gold
        var accounts = new List<LoyaltyAccount>
        {
            new LoyaltyAccount { Id = Guid.NewGuid(), Tier = LoyaltyTier.Gold }
        };

        var accountResponses = new List<LoyaltyAccountResponse>
        {
            new LoyaltyAccountResponse { Id = accounts[0].Id, Tier = LoyaltyTier.Gold }
        };

        _mockLoyaltyAccountRepository.Setup(x => x.GetByTierAsync(tier, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts);

        _mockMapper.Setup(x => x.Map<IEnumerable<LoyaltyAccountResponse>>(accounts))
            .Returns(accountResponses);

        // Act
        var result = await _loyaltyAccountService.GetByTierAsync(tier);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_AccountDoesNotExist_CreatesNewAccount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createRequest = new CreateLoyaltyAccountRequest
        {
            UserId = userId
        };

        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 0,
            Tier = LoyaltyTier.Bronze
        };

        var accountResponse = new LoyaltyAccountResponse
        {
            Id = account.Id,
            UserId = userId,
            PointsBalance = 0,
            Tier = LoyaltyTier.Bronze
        };

        _mockLoyaltyAccountRepository.Setup(x => x.UserHasAccountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockMapper.Setup(x => x.Map<LoyaltyAccount>(createRequest))
            .Returns(account);

        _mockMapper.Setup(x => x.Map<LoyaltyAccountResponse>(account))
            .Returns(accountResponse);

        _mockLoyaltyAccountRepository.Setup(x => x.AddAsync(It.IsAny<LoyaltyAccount>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoyaltyAccount a, CancellationToken ct) => a);

        // Act
        var result = await _loyaltyAccountService.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UserId.Should().Be(userId);

        _mockLoyaltyAccountRepository.Verify(x => x.AddAsync(It.IsAny<LoyaltyAccount>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_AccountAlreadyExists_ReturnsExistingAccount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createRequest = new CreateLoyaltyAccountRequest
        {
            UserId = userId
        };

        var existingAccount = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 100,
            Tier = LoyaltyTier.Silver
        };

        var accountResponse = new LoyaltyAccountResponse
        {
            Id = existingAccount.Id,
            UserId = userId,
            PointsBalance = 100,
            Tier = LoyaltyTier.Silver
        };

        _mockLoyaltyAccountRepository.Setup(x => x.UserHasAccountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockLoyaltyAccountRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        _mockMapper.Setup(x => x.Map<LoyaltyAccountResponse>(existingAccount))
            .Returns(accountResponse);

        // Act
        var result = await _loyaltyAccountService.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(existingAccount.Id);

        _mockLoyaltyAccountRepository.Verify(x => x.AddAsync(It.IsAny<LoyaltyAccount>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_UniqueConstraintViolation_ReturnsExistingAccount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createRequest = new CreateLoyaltyAccountRequest
        {
            UserId = userId
        };

        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 0,
            Tier = LoyaltyTier.Bronze
        };

        var existingAccount = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 50,
            Tier = LoyaltyTier.Bronze
        };

        var accountResponse = new LoyaltyAccountResponse
        {
            Id = existingAccount.Id,
            UserId = userId,
            PointsBalance = 50,
            Tier = LoyaltyTier.Bronze
        };

        _mockLoyaltyAccountRepository.Setup(x => x.UserHasAccountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockMapper.Setup(x => x.Map<LoyaltyAccount>(createRequest))
            .Returns(account);

        var pgException = new PostgresException("duplicate key", "ERROR", "ERROR", "23505");
        var dbUpdateException = new DbUpdateException("Duplicate key", pgException);

        _mockLoyaltyAccountRepository.Setup(x => x.AddAsync(It.IsAny<LoyaltyAccount>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbUpdateException);

        _mockLoyaltyAccountRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        _mockMapper.Setup(x => x.Map<LoyaltyAccountResponse>(existingAccount))
            .Returns(accountResponse);

        // Act
        var result = await _loyaltyAccountService.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(existingAccount.Id);
    }

    [Fact]
    public async Task UpdateAsync_AccountExists_UpdatesAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var updateRequest = new UpdateLoyaltyAccountRequest
        {
            IsActive = false
        };

        var account = new LoyaltyAccount
        {
            Id = accountId,
            UserId = Guid.NewGuid(),
            PointsBalance = 100,
            IsActive = true
        };

        var accountResponse = new LoyaltyAccountResponse
        {
            Id = accountId,
            PointsBalance = 100
        };

        _mockLoyaltyAccountRepository.Setup(x => x.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _mockMapper.Setup(x => x.Map(updateRequest, account));

        _mockMapper.Setup(x => x.Map<LoyaltyAccountResponse>(account))
            .Returns(accountResponse);

        _mockLoyaltyAccountRepository.Setup(x => x.UpdateAsync(It.IsAny<LoyaltyAccount>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _loyaltyAccountService.UpdateAsync(accountId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _mockLoyaltyAccountRepository.Verify(x => x.UpdateAsync(It.IsAny<LoyaltyAccount>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_AccountNotFound_ReturnsFailure()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var updateRequest = new UpdateLoyaltyAccountRequest();

        _mockLoyaltyAccountRepository.Setup(x => x.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoyaltyAccount?)null);

        // Act
        var result = await _loyaltyAccountService.UpdateAsync(accountId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Loyalty account with ID {accountId} not found");
    }

    [Fact]
    public async Task DeleteAsync_AccountExists_DeletesAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = accountId,
            UserId = Guid.NewGuid()
        };

        _mockLoyaltyAccountRepository.Setup(x => x.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _mockLoyaltyAccountRepository.Setup(x => x.DeleteAsync(account, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _loyaltyAccountService.DeleteAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _mockLoyaltyAccountRepository.Verify(x => x.DeleteAsync(account, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddPointsAsync_Success_AddsPointsAndUpdatesTier()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new AddPointsRequest
        {
            UserId = userId,
            Points = 500,
            Reason = "Purchase reward",
            ReferenceId = Guid.NewGuid()
        };

        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 500,
            Tier = LoyaltyTier.Silver
        };

        var accountResponse = new LoyaltyAccountResponse
        {
            Id = account.Id,
            UserId = userId,
            PointsBalance = 500,
            Tier = LoyaltyTier.Gold
        };

        _mockLoyaltyAccountRepository.Setup(x => x.AddPointsAsync(
            userId, request.Points, request.Reason, request.ReferenceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockLoyaltyAccountRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _mockMapper.Setup(x => x.Map<LoyaltyAccountResponse>(account))
            .Returns(accountResponse);

        _mockLoyaltyAccountRepository.Setup(x => x.UpdateAsync(It.IsAny<LoyaltyAccount>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _loyaltyAccountService.AddPointsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        _mockLoyaltyAccountRepository.Verify(x => x.AddPointsAsync(
            userId, request.Points, request.Reason, request.ReferenceId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddPointsAsync_Failure_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new AddPointsRequest
        {
            UserId = userId,
            Points = 100,
            Reason = "Test"
        };

        _mockLoyaltyAccountRepository.Setup(x => x.AddPointsAsync(
            userId, request.Points, request.Reason, request.ReferenceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _loyaltyAccountService.AddPointsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Failed to add points for user {userId}");
    }

    [Fact]
    public async Task RedeemPointsAsync_SufficientPoints_RedeemsPoints()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var request = new RedeemPointsRequest
        {
            UserId = userId,
            Points = 50,
            RewardType = "Discount",
            RewardMetadata = "{}"
        };

        var account = new LoyaltyAccount
        {
            Id = accountId,
            UserId = userId,
            PointsBalance = 100,
            Tier = LoyaltyTier.Silver
        };

        var redemption = new LoyaltyRedemption
        {
            Id = Guid.NewGuid(),
            LoyaltyAccountId = accountId,
            PointsUsed = 50,
            RewardType = "Discount"
        };

        var redemptionResponse = new LoyaltyRedemptionResponse
        {
            Id = redemption.Id,
            PointsUsed = 50,
            RewardType = "Discount"
        };

        _mockLoyaltyAccountRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _mockLoyaltyAccountRepository.Setup(x => x.DeductPointsAsync(
            userId, request.Points, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockLoyaltyRedemptionRepository.Setup(x => x.AddAsync(It.IsAny<LoyaltyRedemption>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoyaltyRedemption r, CancellationToken ct) => r);

        _mockMapper.Setup(x => x.Map<LoyaltyRedemptionResponse>(It.IsAny<LoyaltyRedemption>()))
            .Returns(redemptionResponse);

        _mockLoyaltyAccountRepository.Setup(x => x.UpdateAsync(It.IsAny<LoyaltyAccount>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _loyaltyAccountService.RedeemPointsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PointsUsed.Should().Be(50);
    }

    [Fact]
    public async Task RedeemPointsAsync_InsufficientPoints_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RedeemPointsRequest
        {
            UserId = userId,
            Points = 150,
            RewardType = "Discount"
        };

        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 100,
            Tier = LoyaltyTier.Bronze
        };

        _mockLoyaltyAccountRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _loyaltyAccountService.RedeemPointsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Insufficient points");
        result.Error.Should().Contain("Available: 100");
        result.Error.Should().Contain("Required: 150");
    }

    [Fact]
    public async Task RedeemPointsAsync_AccountNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RedeemPointsRequest
        {
            UserId = userId,
            Points = 50,
            RewardType = "Discount"
        };

        _mockLoyaltyAccountRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoyaltyAccount?)null);

        // Act
        var result = await _loyaltyAccountService.RedeemPointsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Loyalty account for user {userId} not found");
    }

    [Fact]
    public async Task GetPointsBalanceAsync_AccountExists_ReturnsBalance()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 250
        };

        _mockLoyaltyAccountRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _loyaltyAccountService.GetPointsBalanceAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(250);
    }

    [Fact]
    public async Task GetPointsBalanceAsync_AccountNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockLoyaltyAccountRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoyaltyAccount?)null);

        // Act
        var result = await _loyaltyAccountService.GetPointsBalanceAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Loyalty account for user {userId} not found");
    }

    [Fact]
    public async Task CreateAsync_ExceptionDuringGet_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createRequest = new CreateLoyaltyAccountRequest
        {
            UserId = userId
        };

        _mockLoyaltyAccountRepository.Setup(x => x.UserHasAccountAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _loyaltyAccountService.CreateAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error creating account");
    }
}

