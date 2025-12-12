using AutoMapper;
using backend.Common;
using backend.Common.Enums;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services;
using backend.Services.Interfaces;
using backend.DTOs.Payment.Request;
using backend.DTOs.Payment.Response;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe.Checkout;
using Xunit;

namespace Backend.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<PaymentService>> _mockLogger;
    private readonly Mock<IStripeServiceWrapper> _mockStripeWrapper;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<PaymentService>>();
        _mockStripeWrapper = new Mock<IStripeServiceWrapper>();
        _mockConfiguration = new Mock<IConfiguration>();

        _mockConfiguration.Setup(c => c["FrontendUrl"]).Returns("http://localhost:3000");

        _paymentService = new PaymentService(
            _mockPaymentRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockStripeWrapper.Object,
            _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetPaymentByIdAsync_PaymentExists_ReturnsSuccess()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 99.99m,
            Currency = "USD",
            Provider = PaymentProvider.Stripe,
            Status = PaymentStatus.Succeeded
        };

        var paymentResponse = new PaymentResponse
        {
            Id = paymentId,
            OrderId = payment.OrderId,
            UserId = payment.UserId,
            Amount = 99.99m,
            Currency = "USD",
            Status = PaymentStatus.Succeeded
        };

        _mockPaymentRepository.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockMapper.Setup(x => x.Map<PaymentResponse>(payment))
            .Returns(paymentResponse);

        // Act
        var result = await _paymentService.GetPaymentByIdAsync(paymentId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(paymentId);
        result.Data.Amount.Should().Be(99.99m);
    }

    [Fact]
    public async Task GetPaymentByIdAsync_PaymentNotFound_ReturnsFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        _mockPaymentRepository.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _paymentService.GetPaymentByIdAsync(paymentId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Payment not found");
    }

    [Fact]
    public async Task GetPaymentByIdAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        _mockPaymentRepository.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _paymentService.GetPaymentByIdAsync(paymentId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error retrieving payment");
        result.Error.Should().Contain("Database error");
    }

    [Fact]
    public async Task GetPaymentByOrderIdAsync_PaymentExists_ReturnsSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            UserId = Guid.NewGuid(),
            Amount = 49.99m,
            Currency = "USD",
            Provider = PaymentProvider.Stripe,
            Status = PaymentStatus.Processing
        };

        var paymentResponse = new PaymentResponse
        {
            Id = payment.Id,
            OrderId = orderId,
            UserId = payment.UserId,
            Amount = 49.99m,
            Currency = "USD",
            Status = PaymentStatus.Processing
        };

        _mockPaymentRepository.Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockMapper.Setup(x => x.Map<PaymentResponse>(payment))
            .Returns(paymentResponse);

        // Act
        var result = await _paymentService.GetPaymentByOrderIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task GetPaymentByOrderIdAsync_PaymentNotFound_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockPaymentRepository.Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _paymentService.GetPaymentByOrderIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Payment not found for order");
    }

    [Fact]
    public async Task GetPaymentByOrderIdAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockPaymentRepository.Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _paymentService.GetPaymentByOrderIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error retrieving payment");
        result.Error.Should().Contain("Database error");
    }

    [Fact]
    public async Task GetPaymentsByUserIdAsync_UserHasPayments_ReturnsPayments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var payments = new List<Payment>
        {
            new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                UserId = userId,
                Amount = 29.99m,
                Currency = "USD",
                Provider = PaymentProvider.Stripe,
                Status = PaymentStatus.Succeeded
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                UserId = userId,
                Amount = 39.99m,
                Currency = "USD",
                Provider = PaymentProvider.Stripe,
                Status = PaymentStatus.Succeeded
            }
        };

        var paymentResponses = new List<PaymentResponse>
        {
            new PaymentResponse { Id = payments[0].Id, Amount = 29.99m },
            new PaymentResponse { Id = payments[1].Id, Amount = 39.99m }
        };

        _mockPaymentRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        _mockMapper.Setup(x => x.Map<IEnumerable<PaymentResponse>>(payments))
            .Returns(paymentResponses);

        // Act
        var result = await _paymentService.GetPaymentsByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPaymentsByUserIdAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockPaymentRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _paymentService.GetPaymentsByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error retrieving payments");
    }

    [Fact]
    public async Task CreatePaymentAsync_ValidRequest_CreatesPayment()
    {
        // Arrange
        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 79.99m,
            Currency = "USD"
        };

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            UserId = request.UserId,
            Amount = request.Amount,
            Currency = request.Currency,
            Provider = PaymentProvider.Stripe,
            Status = PaymentStatus.Processing,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var paymentResponse = new PaymentResponse
        {
            Id = payment.Id,
            OrderId = request.OrderId,
            UserId = request.UserId,
            Amount = request.Amount,
            Currency = request.Currency,
            Status = PaymentStatus.Processing
        };

        _mockMapper.Setup(x => x.Map<Payment>(request))
            .Returns(payment);

        _mockMapper.Setup(x => x.Map<PaymentResponse>(payment))
            .Returns(paymentResponse);

        _mockPaymentRepository.Setup(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment p, CancellationToken ct) => p);

        // Act
        var result = await _paymentService.CreatePaymentAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Amount.Should().Be(79.99m);

        _mockPaymentRepository.Verify(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 79.99m,
            Currency = "USD"
        };

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            UserId = request.UserId,
            Amount = request.Amount,
            Currency = request.Currency
        };

        _mockMapper.Setup(x => x.Map<Payment>(request))
            .Returns(payment);

        _mockPaymentRepository.Setup(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _paymentService.CreatePaymentAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error creating payment");
        result.Error.Should().Contain("Database error");
    }

    [Fact]
    public async Task GetPaymentsByUserIdAsync_NoPayments_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockPaymentRepository.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payment>());

        _mockMapper.Setup(x => x.Map<IEnumerable<PaymentResponse>>(It.IsAny<IEnumerable<Payment>>()))
            .Returns(new List<PaymentResponse>());

        // Act
        var result = await _paymentService.GetPaymentsByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePaymentAsync_SetsTimestamps()
    {
        // Arrange
        var request = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Amount = 50m,
            Currency = "USD"
        };

        Payment? capturedPayment = null;
        _mockPaymentRepository.Setup(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment p, CancellationToken ct) => { capturedPayment = p; return p; });

        _mockMapper.Setup(x => x.Map<Payment>(request))
            .Returns(new Payment());

        _mockMapper.Setup(x => x.Map<PaymentResponse>(It.IsAny<Payment>()))
            .Returns(new PaymentResponse());

        // Act
        await _paymentService.CreatePaymentAsync(request);

        // Assert
        capturedPayment.Should().NotBeNull();
        capturedPayment!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        capturedPayment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    #region CreateCheckoutSessionAsync Tests

    [Fact]
    public async Task CreateCheckoutSessionAsync_Success_ReturnsSessionResponse()
    {
        // Arrange
        var request = new CreateCheckoutSessionRequest
        {
            OrderId = Guid.NewGuid(),
            UserEmail = "test@example.com",
            LineItems = new List<CheckoutLineItem>
            {
                new CheckoutLineItem { Name = "Product 1", UnitPrice = 10.00m, Quantity = 2 }
            }
        };

        var session = new Session { Id = "cs_test_123", Url = "https://checkout.stripe.com/pay/cs_test_123" };

        _mockStripeWrapper.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<SessionCreateOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _paymentService.CreateCheckoutSessionAsync(request, "user123");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.SessionId.Should().Be("cs_test_123");
        result.Data.SessionUrl.Should().Be("https://checkout.stripe.com/pay/cs_test_123");
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_MultipleLineItems_CreatesSession()
    {
        // Arrange
        var request = new CreateCheckoutSessionRequest
        {
            OrderId = Guid.NewGuid(),
            UserEmail = "test@example.com",
            LineItems = new List<CheckoutLineItem>
            {
                new CheckoutLineItem { Name = "Product 1", UnitPrice = 10.00m, Quantity = 2 },
                new CheckoutLineItem { Name = "Product 2", UnitPrice = 25.50m, Quantity = 1 }
            }
        };

        var session = new Session { Id = "cs_test_456", Url = "https://checkout.stripe.com/pay/cs_test_456" };

        SessionCreateOptions? capturedOptions = null;
        _mockStripeWrapper.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<SessionCreateOptions>(), It.IsAny<CancellationToken>()))
            .Callback<SessionCreateOptions, CancellationToken>((opts, _) => capturedOptions = opts)
            .ReturnsAsync(session);

        // Act
        var result = await _paymentService.CreateCheckoutSessionAsync(request, "user456");

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedOptions.Should().NotBeNull();
        capturedOptions!.LineItems.Should().HaveCount(2);
        capturedOptions.CustomerEmail.Should().Be("test@example.com");
        capturedOptions.Metadata["userId"].Should().Be("user456");
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_StripeException_ReturnsFailure()
    {
        // Arrange
        var request = new CreateCheckoutSessionRequest
        {
            OrderId = Guid.NewGuid(),
            UserEmail = "test@example.com",
            LineItems = new List<CheckoutLineItem>
            {
                new CheckoutLineItem { Name = "Product", UnitPrice = 10.00m, Quantity = 1 }
            }
        };

        _mockStripeWrapper.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<SessionCreateOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Stripe API error"));

        // Act
        var result = await _paymentService.CreateCheckoutSessionAsync(request, "user789");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Stripe API error");
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_SetsCorrectCurrencyAndUrls()
    {
        // Arrange
        var request = new CreateCheckoutSessionRequest
        {
            OrderId = Guid.NewGuid(),
            UserEmail = "test@example.com",
            LineItems = new List<CheckoutLineItem>
            {
                new CheckoutLineItem { Name = "Product", UnitPrice = 15.00m, Quantity = 1 }
            }
        };

        var session = new Session { Id = "cs_test", Url = "https://checkout.stripe.com" };

        SessionCreateOptions? capturedOptions = null;
        _mockStripeWrapper.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<SessionCreateOptions>(), It.IsAny<CancellationToken>()))
            .Callback<SessionCreateOptions, CancellationToken>((opts, _) => capturedOptions = opts)
            .ReturnsAsync(session);

        // Act
        await _paymentService.CreateCheckoutSessionAsync(request, "user");

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.LineItems![0].PriceData!.Currency.Should().Be("ron");
        capturedOptions.LineItems![0].PriceData!.UnitAmount.Should().Be(1500); // 15.00 * 100
        capturedOptions.SuccessUrl.Should().Contain("success");
        capturedOptions.CancelUrl.Should().Contain("checkout");
    }

    #endregion
}

