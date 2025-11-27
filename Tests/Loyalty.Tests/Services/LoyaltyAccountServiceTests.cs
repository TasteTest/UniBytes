using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using backend_loyalty.Common;
using backend_loyalty.DTOs.Request;
using backend_loyalty.DTOs.Response;
using backend_loyalty.Model;
using backend_loyalty.Repositories.Interfaces;
using backend_loyalty.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Loyalty.Tests.Services;

public class LoyaltyAccountServiceTests
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IMapper _mockMapper;
    private readonly ILogger<LoyaltyAccountService> _mockLogger;
    private readonly LoyaltyAccountService _sut;

    public LoyaltyAccountServiceTests()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        _mockMapper = Substitute.For<IMapper>();
        _mockLogger = Substitute.For<ILogger<LoyaltyAccountService>>();
        _sut = new LoyaltyAccountService(_mockUnitOfWork, _mockMapper, _mockLogger);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenExists_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 100,
            Tier = backend_loyalty.Common.Enums.LoyaltyTier.Bronze,
            IsActive = true
        };

        var response = new LoyaltyAccountResponse
        {
            Id = account.Id,
            UserId = account.UserId,
            PointsBalance = account.PointsBalance,
            Tier = 0,
            TierName = "Bronze",
            IsActive = true
        };

        _mockUnitOfWork.LoyaltyAccounts.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(account);
        _mockMapper.Map<LoyaltyAccountResponse>(account).Returns(response);

        // Act
        var result = await _sut.GetByUserIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UserId.Should().Be(userId);
        result.Data.PointsBalance.Should().Be(100);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenNotExists_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUnitOfWork.LoyaltyAccounts.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((LoyaltyAccount?)null);

        // Act
        var result = await _sut.GetByUserIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateLoyaltyAccountRequest
        {
            UserId = userId,
            PointsBalance = 0,
            Tier = 0,
            IsActive = true
        };

        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 0,
            Tier = backend_loyalty.Common.Enums.LoyaltyTier.Bronze,
            IsActive = true
        };

        var response = new LoyaltyAccountResponse
        {
            Id = account.Id,
            UserId = userId,
            PointsBalance = 0,
            Tier = 0,
            TierName = "Bronze",
            IsActive = true
        };

        _mockUnitOfWork.LoyaltyAccounts.UserHasAccountAsync(userId, Arg.Any<CancellationToken>())
            .Returns(false);
        _mockMapper.Map<LoyaltyAccount>(request).Returns(account);
        _mockMapper.Map<LoyaltyAccountResponse>(account).Returns(response);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UserId.Should().Be(userId);
        
        await _mockUnitOfWork.LoyaltyAccounts.Received(1).AddAsync(
            Arg.Any<LoyaltyAccount>(), 
            Arg.Any<CancellationToken>()
        );
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenUserAlreadyHasAccount_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateLoyaltyAccountRequest
        {
            UserId = userId
        };

        _mockUnitOfWork.LoyaltyAccounts.UserHasAccountAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already has a loyalty account");
        
        await _mockUnitOfWork.LoyaltyAccounts.DidNotReceive().AddAsync(
            Arg.Any<LoyaltyAccount>(), 
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task AddPointsAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new AddPointsRequest
        {
            UserId = userId,
            Points = 50,
            Reason = "Order purchase",
            ReferenceId = Guid.NewGuid()
        };

        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 150,
            Tier = backend_loyalty.Common.Enums.LoyaltyTier.Bronze,
            IsActive = true
        };

        var response = new LoyaltyAccountResponse
        {
            Id = account.Id,
            UserId = userId,
            PointsBalance = 150,
            Tier = 0,
            TierName = "Bronze",
            IsActive = true
        };

        _mockUnitOfWork.LoyaltyAccounts.AddPointsAsync(
            userId, 
            request.Points, 
            request.Reason, 
            request.ReferenceId, 
            Arg.Any<CancellationToken>()
        ).Returns(true);

        _mockUnitOfWork.LoyaltyAccounts.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(account);
        _mockMapper.Map<LoyaltyAccountResponse>(account).Returns(response);

        // Act
        var result = await _sut.AddPointsAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PointsBalance.Should().Be(150);
        
        await _mockUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RedeemPointsAsync_WithSufficientPoints_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var request = new RedeemPointsRequest
        {
            UserId = userId,
            Points = 50,
            RewardType = "FREE_DRINK",
            RewardMetadata = "{\"rewardId\":\"free-drink\"}"
        };

        var account = new LoyaltyAccount
        {
            Id = accountId,
            UserId = userId,
            PointsBalance = 100,
            Tier = backend_loyalty.Common.Enums.LoyaltyTier.Bronze,
            IsActive = true
        };

        var redemption = new LoyaltyRedemption
        {
            Id = Guid.NewGuid(),
            LoyaltyAccountId = accountId,
            PointsUsed = 50,
            RewardType = "FREE_DRINK",
            RewardMetadata = "{\"rewardId\":\"free-drink\"}",
            CreatedAt = DateTime.UtcNow
        };

        var redemptionResponse = new LoyaltyRedemptionResponse
        {
            Id = redemption.Id,
            LoyaltyAccountId = accountId,
            PointsUsed = 50,
            RewardType = "FREE_DRINK",
            RewardMetadata = "{\"rewardId\":\"free-drink\"}",
            CreatedAt = DateTime.UtcNow
        };

        _mockUnitOfWork.LoyaltyAccounts.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(account);
        _mockUnitOfWork.LoyaltyAccounts.DeductPointsAsync(
            userId, 
            request.Points, 
            Arg.Any<string>(), 
            null, 
            Arg.Any<CancellationToken>()
        ).Returns(true);
        _mockMapper.Map<LoyaltyRedemptionResponse>(Arg.Any<LoyaltyRedemption>())
            .Returns(redemptionResponse);

        // Act
        var result = await _sut.RedeemPointsAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PointsUsed.Should().Be(50);
        result.Data.RewardType.Should().Be("FREE_DRINK");
        
        await _mockUnitOfWork.LoyaltyRedemptions.Received(1).AddAsync(
            Arg.Any<LoyaltyRedemption>(), 
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task RedeemPointsAsync_WithInsufficientPoints_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RedeemPointsRequest
        {
            UserId = userId,
            Points = 150,
            RewardType = "FREE_DRINK"
        };

        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 50,
            Tier = backend_loyalty.Common.Enums.LoyaltyTier.Bronze,
            IsActive = true
        };

        _mockUnitOfWork.LoyaltyAccounts.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(account);

        // Act
        var result = await _sut.RedeemPointsAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Insufficient points");
        
        await _mockUnitOfWork.LoyaltyRedemptions.DidNotReceive().AddAsync(
            Arg.Any<LoyaltyRedemption>(), 
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetPointsBalanceAsync_WhenAccountExists_ReturnsBalance()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 250,
            Tier = backend_loyalty.Common.Enums.LoyaltyTier.Silver,
            IsActive = true
        };

        _mockUnitOfWork.LoyaltyAccounts.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(account);

        // Act
        var result = await _sut.GetPointsBalanceAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(250);
    }
}
