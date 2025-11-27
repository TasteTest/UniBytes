using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using backend_loyalty.Common;
using backend_loyalty.Controllers;
using backend_loyalty.DTOs.Request;
using backend_loyalty.DTOs.Response;
using backend_loyalty.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Loyalty.Tests.Controllers;

public class LoyaltyAccountsControllerTests
{
    private readonly ILoyaltyAccountService _mockService;
    private readonly ILogger<LoyaltyAccountsController> _mockLogger;
    private readonly LoyaltyAccountsController _controller;

    public LoyaltyAccountsControllerTests()
    {
        _mockService = Substitute.For<ILoyaltyAccountService>();
        _mockLogger = Substitute.For<ILogger<LoyaltyAccountsController>>();
        _controller = new LoyaltyAccountsController(_mockService, _mockLogger);
    }

    [Fact]
    public async Task GetByUserId_WhenExists_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var response = new LoyaltyAccountResponse
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 100,
            Tier = 0,
            TierName = "Bronze",
            IsActive = true
        };

        _mockService.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result<LoyaltyAccountResponse>.Success(response));

        // Act
        var result = await _controller.GetByUserId(userId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedAccount = okResult.Value.Should().BeAssignableTo<LoyaltyAccountResponse>().Subject;
        returnedAccount.UserId.Should().Be(userId);
        returnedAccount.PointsBalance.Should().Be(100);
    }

    [Fact]
    public async Task GetByUserId_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockService.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result<LoyaltyAccountResponse>.Failure("Account not found"));

        // Act
        var result = await _controller.GetByUserId(userId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Account not found");
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAtAction()
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

        var response = new LoyaltyAccountResponse
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 0,
            Tier = 0,
            TierName = "Bronze",
            IsActive = true
        };

        _mockService.CreateAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result<LoyaltyAccountResponse>.Success(response));

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(LoyaltyAccountsController.GetById));
        var returnedAccount = createdResult.Value.Should().BeAssignableTo<LoyaltyAccountResponse>().Subject;
        returnedAccount.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Create_WhenUserAlreadyHasAccount_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateLoyaltyAccountRequest
        {
            UserId = Guid.NewGuid()
        };

        _mockService.CreateAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result<LoyaltyAccountResponse>.Failure("User already has account"));

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("User already has account");
    }

    [Fact]
    public async Task AddPoints_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new AddPointsRequest
        {
            UserId = userId,
            Points = 50,
            Reason = "Order purchase"
        };

        var response = new LoyaltyAccountResponse
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PointsBalance = 150,
            Tier = 0,
            TierName = "Bronze",
            IsActive = true
        };

        _mockService.AddPointsAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result<LoyaltyAccountResponse>.Success(response));

        // Act
        var result = await _controller.AddPoints(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedAccount = okResult.Value.Should().BeAssignableTo<LoyaltyAccountResponse>().Subject;
        returnedAccount.PointsBalance.Should().Be(150);
    }

    [Fact]
    public async Task RedeemPoints_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RedeemPointsRequest
        {
            UserId = userId,
            Points = 50,
            RewardType = "FREE_DRINK"
        };

        var response = new LoyaltyRedemptionResponse
        {
            Id = Guid.NewGuid(),
            LoyaltyAccountId = Guid.NewGuid(),
            PointsUsed = 50,
            RewardType = "FREE_DRINK",
            RewardMetadata = "{}",
            CreatedAt = DateTime.UtcNow
        };

        _mockService.RedeemPointsAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result<LoyaltyRedemptionResponse>.Success(response));

        // Act
        var result = await _controller.RedeemPoints(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var redemption = okResult.Value.Should().BeAssignableTo<LoyaltyRedemptionResponse>().Subject;
        redemption.PointsUsed.Should().Be(50);
        redemption.RewardType.Should().Be("FREE_DRINK");
    }

    [Fact]
    public async Task RedeemPoints_WithInsufficientPoints_ReturnsBadRequest()
    {
        // Arrange
        var request = new RedeemPointsRequest
        {
            UserId = Guid.NewGuid(),
            Points = 200,
            RewardType = "FREE_MEAL"
        };

        _mockService.RedeemPointsAsync(request, Arg.Any<CancellationToken>())
            .Returns(Result<LoyaltyRedemptionResponse>.Failure("Insufficient points"));

        // Act
        var result = await _controller.RedeemPoints(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Insufficient points");
    }

    [Fact]
    public async Task GetPointsBalance_WhenAccountExists_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        long expectedBalance = 250;

        _mockService.GetPointsBalanceAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Result<long>.Success(expectedBalance));

        // Act
        var result = await _controller.GetPointsBalance(userId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var balance = okResult.Value.Should().BeOfType<long>().Subject;
        balance.Should().Be(250);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithAccounts()
    {
        // Arrange
        var accounts = new List<LoyaltyAccountResponse>
        {
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PointsBalance = 100, TierName = "Bronze" },
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PointsBalance = 500, TierName = "Gold" }
        };

        _mockService.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<LoyaltyAccountResponse>>.Success(accounts));

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedAccounts = okResult.Value.Should().BeAssignableTo<IEnumerable<LoyaltyAccountResponse>>().Subject;
        returnedAccounts.Should().HaveCount(2);
    }

    [Fact]
    public async Task Delete_WhenExists_ReturnsNoContent()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mockService.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _controller.Delete(accountId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mockService.DeleteAsync(accountId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Account not found"));

        // Act
        var result = await _controller.Delete(accountId, CancellationToken.None);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Account not found");
    }
}
