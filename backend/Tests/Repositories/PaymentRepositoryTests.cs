using backend.Common.Enums;
using backend.Data;
using backend.Models;
using backend.Repositories;
using FluentAssertions;
using Xunit;

namespace Backend.Tests.Repositories;

public class PaymentRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PaymentRepository _repository;

    public PaymentRepositoryTests()
    {
        _context = RepositoryTestsHelper.CreateInMemoryContext();
        _repository = new PaymentRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetByOrderIdAsync_OrderHasPayment_ReturnsLatestPayment()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var payments = new List<Payment>
        {
            new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                UserId = userId,
                Amount = 50m,
                Status = PaymentStatus.Processing,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                UserId = userId,
                Amount = 50m,
                Status = PaymentStatus.Succeeded,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };
        await _context.Payments.AddRangeAsync(payments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByOrderIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(PaymentStatus.Succeeded);
    }

    [Fact]
    public async Task GetByOrderIdAsync_NoPaymentForOrder_ReturnsNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByOrderIdAsync(orderId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsAllUserPayments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var payments = new List<Payment>
        {
            new Payment { Id = Guid.NewGuid(), OrderId = Guid.NewGuid(), UserId = userId, Amount = 10m, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new Payment { Id = Guid.NewGuid(), OrderId = Guid.NewGuid(), UserId = userId, Amount = 20m, CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new Payment { Id = Guid.NewGuid(), OrderId = Guid.NewGuid(), UserId = otherUserId, Amount = 30m, CreatedAt = DateTime.UtcNow }
        };
        await _context.Payments.AddRangeAsync(payments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        var enumerable = result.ToList();
        enumerable.Should().HaveCount(2);
        enumerable.All(p => p.UserId == userId).Should().BeTrue();
        enumerable.First().Amount.Should().Be(20m); // Most recent first
    }

    [Fact]
    public async Task GetByProviderPaymentIdAsync_PaymentExists_ReturnsPayment()
    {
        // Arrange
        var providerPaymentId = "stripe_payment_123";
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ProviderPaymentId = providerPaymentId,
            Amount = 50m
        };
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByProviderPaymentIdAsync(providerPaymentId);

        // Assert
        result.Should().NotBeNull();
        result!.ProviderPaymentId.Should().Be(providerPaymentId);
    }

    [Fact]
    public async Task GetByProviderPaymentIdAsync_PaymentNotExists_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByProviderPaymentIdAsync("nonexistent_payment");

        // Assert
        result.Should().BeNull();
    }
}

