using backend.Common;
using backend.Common.Enums;
using backend.Controllers;
using backend.Services.Interfaces;
using backend.DTOs.Loyalty.Request;
using backend.DTOs.Loyalty.Response;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers;

public class LoyaltyAccountsControllerTests
{
    private readonly Mock<ILoyaltyAccountService> _mockLoyaltyAccountService;
    private readonly LoyaltyAccountsController _controller;

    public LoyaltyAccountsControllerTests()
    {
        _mockLoyaltyAccountService = new Mock<ILoyaltyAccountService>();
        _controller = new LoyaltyAccountsController(_mockLoyaltyAccountService.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var accounts = new List<LoyaltyAccountResponse>
        {
            new LoyaltyAccountResponse { Id = Guid.NewGuid(), PointsBalance = 100, Tier = LoyaltyTier.Bronze }
        };

        _mockLoyaltyAccountService.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<LoyaltyAccountResponse>>.Success(accounts));

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        _mockLoyaltyAccountService.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<LoyaltyAccountResponse>>.Failure("Error"));

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetActive_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var accounts = new List<LoyaltyAccountResponse>();
        _mockLoyaltyAccountService.Setup(x => x.GetActiveAccountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<LoyaltyAccountResponse>>.Success(accounts));

        // Act
        var result = await _controller.GetActive(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByTier_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var tier = 1;
        var accounts = new List<LoyaltyAccountResponse>();
        _mockLoyaltyAccountService.Setup(x => x.GetByTierAsync(tier, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<LoyaltyAccountResponse>>.Success(accounts));

        // Act
        var result = await _controller.GetByTier(tier, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new LoyaltyAccountResponse { Id = accountId, PointsBalance = 100 };

        _mockLoyaltyAccountService.Setup(x => x.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoyaltyAccountResponse>.Success(account));

        // Act
        var result = await _controller.GetById(accountId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenAccountNotFound()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mockLoyaltyAccountService.Setup(x => x.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoyaltyAccountResponse>.Failure("Not found"));

        // Act
        var result = await _controller.GetById(accountId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByUserId_ReturnsOk_WhenAccountExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var account = new LoyaltyAccountResponse { Id = Guid.NewGuid(), UserId = userId };

        _mockLoyaltyAccountService.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoyaltyAccountResponse>.Success(account));

        // Act
        var result = await _controller.GetByUserId(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAccountDetails_ReturnsOk_WhenAccountExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var details = new LoyaltyAccountDetailsResponse
        {
            Account = new LoyaltyAccountResponse { UserId = userId },
            RecentTransactions = new List<LoyaltyTransactionResponse>(),
            RecentRedemptions = new List<LoyaltyRedemptionResponse>()
        };

        _mockLoyaltyAccountService.Setup(x => x.GetAccountDetailsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoyaltyAccountDetailsResponse>.Success(details));

        // Act
        var result = await _controller.GetAccountDetails(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPointsBalance_ReturnsOk_WhenAccountExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var balance = 500L;

        _mockLoyaltyAccountService.Setup(x => x.GetPointsBalanceAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<long>.Success(balance));

        // Act
        var result = await _controller.GetPointsBalance(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(balance);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenAccountCreated()
    {
        // Arrange
        var request = new CreateLoyaltyAccountRequest { UserId = Guid.NewGuid() };
        var account = new LoyaltyAccountResponse { Id = Guid.NewGuid(), UserId = request.UserId };

        _mockLoyaltyAccountService.Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoyaltyAccountResponse>.Success(account));

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Arrange
        var request = new CreateLoyaltyAccountRequest();
        _controller.ModelState.AddModelError("UserId", "Required");

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenAccountUpdated()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var request = new UpdateLoyaltyAccountRequest { IsActive = false };
        var account = new LoyaltyAccountResponse { Id = accountId };

        _mockLoyaltyAccountService.Setup(x => x.UpdateAsync(accountId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoyaltyAccountResponse>.Success(account));

        // Act
        var result = await _controller.Update(accountId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenAccountDeleted()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mockLoyaltyAccountService.Setup(x => x.DeleteAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.Delete(accountId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task AddPoints_ReturnsOk_WhenPointsAdded()
    {
        // Arrange
        var request = new AddPointsRequest { UserId = Guid.NewGuid(), Points = 100, Reason = "Purchase" };
        var account = new LoyaltyAccountResponse { PointsBalance = 100 };

        _mockLoyaltyAccountService.Setup(x => x.AddPointsAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoyaltyAccountResponse>.Success(account));

        // Act
        var result = await _controller.AddPoints(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RedeemPoints_ReturnsOk_WhenPointsRedeemed()
    {
        // Arrange
        var request = new RedeemPointsRequest { UserId = Guid.NewGuid(), Points = 50, RewardType = "Discount" };
        var redemption = new LoyaltyRedemptionResponse { Id = Guid.NewGuid(), PointsUsed = 50 };

        _mockLoyaltyAccountService.Setup(x => x.RedeemPointsAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoyaltyRedemptionResponse>.Success(redemption));

        // Act
        var result = await _controller.RedeemPoints(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}

