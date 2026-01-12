using AutoMapper;
using backend.Common;
using backend.Common.Enums;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services;
using backend.Services.Interfaces;
using backend.DTOs.Payment.Request;
using backend.DTOs.Payment.Response;
using backend.DTOs.Order.Request;
using backend.DTOs.Order.Response;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using Stripe.Checkout;
using Xunit;

namespace Backend.Tests.Services;

public class StripeServiceTests
{
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;
    private readonly Mock<IIdempotencyKeyRepository> _mockIdempotencyKeyRepository;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<StripeService>> _mockLogger;
    private readonly Mock<IStripeServiceWrapper> _mockStripeWrapper;
    private readonly StripeService _stripeService;

    public StripeServiceTests()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockIdempotencyKeyRepository = new Mock<IIdempotencyKeyRepository>();
        _mockUserService = new Mock<IUserService>();
        _mockOrderService = new Mock<IOrderService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<StripeService>>();
        _mockStripeWrapper = new Mock<IStripeServiceWrapper>();

        _stripeService = new StripeService(
            _mockPaymentRepository.Object,
            _mockIdempotencyKeyRepository.Object,
            _mockUserService.Object,
            _mockOrderService.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockStripeWrapper.Object);
    }

    #region CreateCheckoutSessionAsync Tests

    [Fact]
    public async Task CreateCheckoutSessionAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var request = new CreateCheckoutSessionRequest
        {
            OrderId = Guid.NewGuid(),
            UserEmail = "notfound@example.com",
            LineItems = new List<CheckoutLineItem>
            {
                new CheckoutLineItem { Name = "Test Item", UnitPrice = 10.00m, Quantity = 1 }
            },
            SuccessUrl = "http://example.com/success",
            CancelUrl = "http://example.com/cancel"
        };

        _mockUserService.Setup(x => x.GetUserEntityByEmailAsync(request.UserEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _stripeService.CreateCheckoutSessionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_DuplicateIdempotencyKey_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateCheckoutSessionRequest
        {
            OrderId = Guid.NewGuid(),
            UserEmail = "test@example.com",
            IdempotencyKey = "duplicate_key",
            LineItems = new List<CheckoutLineItem> { new CheckoutLineItem { Name = "Item", UnitPrice = 10m, Quantity = 1 } },
            SuccessUrl = "http://example.com/success",
            CancelUrl = "http://example.com/cancel"
        };

        var user = new User { Id = userId, Email = request.UserEmail };

        _mockUserService.Setup(x => x.GetUserEntityByEmailAsync(request.UserEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockIdempotencyKeyRepository.Setup(x => x.KeyExistsAsync(request.IdempotencyKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _stripeService.CreateCheckoutSessionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Duplicate request");
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_OrderCreationFails_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateCheckoutSessionRequest
        {
            OrderId = Guid.NewGuid(),
            UserEmail = "test@example.com",
            LineItems = new List<CheckoutLineItem> { new CheckoutLineItem { Name = "Item", UnitPrice = 10m, Quantity = 1 } },
            SuccessUrl = "http://example.com/success",
            CancelUrl = "http://example.com/cancel"
        };

        var user = new User { Id = userId, Email = request.UserEmail };

        _mockUserService.Setup(x => x.GetUserEntityByEmailAsync(request.UserEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockOrderService.Setup(x => x.CreateAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Failure("Order creation failed"));

        // Act
        var result = await _stripeService.CreateCheckoutSessionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to create order");
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_Success_ReturnsCheckoutSessionResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var request = new CreateCheckoutSessionRequest
        {
            OrderId = Guid.NewGuid(),
            UserEmail = "test@example.com",
            LineItems = new List<CheckoutLineItem>
            {
                new CheckoutLineItem
                {
                    Name = "Test Item",
                    Description = "Test Description",
                    UnitPrice = 10.00m,
                    Quantity = 2,
                    ImageUrl = "http://example.com/image.jpg"
                }
            },
            SuccessUrl = "http://example.com/success",
            CancelUrl = "http://example.com/cancel"
        };

        var user = new User { Id = userId, Email = request.UserEmail };
        var orderResponse = new OrderResponse
        {
            Id = orderId,
            UserId = userId,
            TotalAmount = 20.00m,
            Currency = "USD",
            PaymentStatus = "Processing",
            OrderStatus = "Pending"
        };

        var session = new Session
        {
            Id = "cs_test_session_id",
            Url = "https://checkout.stripe.com/pay/cs_test_session_id"
        };

        _mockUserService.Setup(x => x.GetUserEntityByEmailAsync(request.UserEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockOrderService.Setup(x => x.CreateAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Success(orderResponse));

        _mockPaymentRepository.Setup(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment p, CancellationToken _) => p);

        _mockStripeWrapper.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<SessionCreateOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _mockPaymentRepository.Setup(x => x.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _stripeService.CreateCheckoutSessionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.SessionId.Should().Be("cs_test_session_id");
        result.Data.SessionUrl.Should().Be("https://checkout.stripe.com/pay/cs_test_session_id");
        result.Data.Message.Should().Be("Checkout session created successfully");
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_WithIdempotencyKey_SavesKey()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var idempotencyKey = "unique_key_123";
        var request = new CreateCheckoutSessionRequest
        {
            OrderId = Guid.NewGuid(),
            UserEmail = "test@example.com",
            IdempotencyKey = idempotencyKey,
            LineItems = new List<CheckoutLineItem> { new CheckoutLineItem { Name = "Item", UnitPrice = 10m, Quantity = 1 } },
            SuccessUrl = "http://example.com/success",
            CancelUrl = "http://example.com/cancel"
        };

        var user = new User { Id = userId, Email = request.UserEmail };
        var orderResponse = new OrderResponse { Id = orderId, UserId = userId, Currency = "USD", PaymentStatus = "Processing", OrderStatus = "Pending" };
        var session = new Session { Id = "cs_test", Url = "https://checkout.stripe.com/cs_test" };

        _mockUserService.Setup(x => x.GetUserEntityByEmailAsync(request.UserEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockIdempotencyKeyRepository.Setup(x => x.KeyExistsAsync(idempotencyKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockOrderService.Setup(x => x.CreateAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Success(orderResponse));

        _mockPaymentRepository.Setup(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment p, CancellationToken _) => p);

        _mockStripeWrapper.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<SessionCreateOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        await _stripeService.CreateCheckoutSessionAsync(request);

        // Assert
        _mockIdempotencyKeyRepository.Verify(x => x.AddAsync(
            It.Is<IdempotencyKey>(k => k.Key == idempotencyKey && k.UserId == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_StripeException_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var request = new CreateCheckoutSessionRequest
        {
            OrderId = Guid.NewGuid(),
            UserEmail = "test@example.com",
            LineItems = new List<CheckoutLineItem> { new CheckoutLineItem { Name = "Item", UnitPrice = 10m, Quantity = 1 } },
            SuccessUrl = "http://example.com/success",
            CancelUrl = "http://example.com/cancel"
        };

        var user = new User { Id = userId, Email = request.UserEmail };
        var orderResponse = new OrderResponse { Id = orderId, UserId = userId, Currency = "USD", PaymentStatus = "Processing", OrderStatus = "Pending" };

        _mockUserService.Setup(x => x.GetUserEntityByEmailAsync(request.UserEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockOrderService.Setup(x => x.CreateAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Success(orderResponse));

        _mockPaymentRepository.Setup(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment p, CancellationToken _) => p);

        _mockStripeWrapper.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<SessionCreateOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StripeException("Stripe API error"));

        // Act
        var result = await _stripeService.CreateCheckoutSessionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Stripe error");
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_GeneralException_ReturnsFailure()
    {
        // Arrange
        var request = new CreateCheckoutSessionRequest
        {
            OrderId = Guid.NewGuid(),
            UserEmail = "test@example.com",
            LineItems = new List<CheckoutLineItem> { new CheckoutLineItem { Name = "Item", UnitPrice = 10m, Quantity = 1 } },
            SuccessUrl = "http://example.com/success",
            CancelUrl = "http://example.com/cancel"
        };

        _mockUserService.Setup(x => x.GetUserEntityByEmailAsync(request.UserEmail, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _stripeService.CreateCheckoutSessionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error creating checkout session");
    }

    #endregion

    #region HandleWebhookEventAsync Tests

    [Fact]
    public async Task HandleWebhookEventAsync_InvalidSignature_ReturnsFailure()
    {
        // Arrange
        var json = "{}";
        var invalidSignature = "invalid_signature";

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(json, invalidSignature))
            .Throws(new StripeException("Invalid signature"));

        // Act
        var result = await _stripeService.HandleWebhookEventAsync(json, invalidSignature);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Webhook verification failed");
    }

    [Fact]
    public async Task HandleWebhookEventAsync_CheckoutSessionCompleted_ProcessesPayment()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var session = new Session { Id = "cs_test", ClientReferenceId = paymentId.ToString(), PaymentIntentId = "pi_test" };
        var stripeEvent = new Event { Type = "checkout.session.completed", Data = new EventData { Object = session } };

        var payment = new Payment { Id = paymentId, Status = PaymentStatus.Processing };
        var paymentResponse = new PaymentResponse { Id = paymentId };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        _mockPaymentRepository.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockPaymentRepository.Setup(x => x.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(x => x.Map<PaymentResponse>(It.IsAny<Payment>()))
            .Returns(paymentResponse);

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        _mockPaymentRepository.Verify(x => x.UpdateAsync(
            It.Is<Payment>(p => p.Status == PaymentStatus.Succeeded),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWebhookEventAsync_PaymentIntentSucceeded_ProcessesPayment()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var paymentIntent = new PaymentIntent { Id = "pi_test" };
        var stripeEvent = new Event { Type = "payment_intent.succeeded", Data = new EventData { Object = paymentIntent } };

        var payment = new Payment { Id = paymentId, ProviderChargeId = "pi_test", Status = PaymentStatus.Processing };
        var paymentResponse = new PaymentResponse { Id = paymentId };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        _mockPaymentRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockPaymentRepository.Setup(x => x.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(x => x.Map<PaymentResponse>(It.IsAny<Payment>()))
            .Returns(paymentResponse);

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleWebhookEventAsync_PaymentIntentFailed_ProcessesPayment()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var paymentIntent = new PaymentIntent
        {
            Id = "pi_test",
            LastPaymentError = new StripeError { Message = "Card declined" }
        };
        var stripeEvent = new Event { Type = "payment_intent.payment_failed", Data = new EventData { Object = paymentIntent } };

        var payment = new Payment { Id = paymentId, ProviderChargeId = "pi_test", Status = PaymentStatus.Processing };
        var paymentResponse = new PaymentResponse { Id = paymentId };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        _mockPaymentRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockPaymentRepository.Setup(x => x.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(x => x.Map<PaymentResponse>(It.IsAny<Payment>()))
            .Returns(paymentResponse);

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockPaymentRepository.Verify(x => x.UpdateAsync(
            It.Is<Payment>(p => p.Status == PaymentStatus.Failed && p.FailureMessage == "Card declined"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWebhookEventAsync_UnhandledEventType_ReturnsSuccess()
    {
        // Arrange
        var stripeEvent = new Event { Type = "some.other.event", Data = new EventData() };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleWebhookEventAsync_GeneralException_ReturnsFailure()
    {
        // Arrange
        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("Unexpected error"));

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error processing webhook");
    }

    #endregion

    #region VerifyPaymentAsync Tests

    [Fact]
    public async Task VerifyPaymentAsync_PaymentNotFound_ReturnsFailure()
    {
        // Arrange
        var sessionId = "cs_test_123";
        var session = new Session { Id = sessionId, PaymentStatus = "paid" };

        _mockStripeWrapper.Setup(x => x.GetCheckoutSessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _mockPaymentRepository.Setup(x => x.GetByProviderPaymentIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _stripeService.VerifyPaymentAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Payment not found");
    }

    [Fact]
    public async Task VerifyPaymentAsync_PaymentPaid_UpdatesStatusToSucceeded()
    {
        // Arrange
        var sessionId = "cs_test_123";
        var paymentId = Guid.NewGuid();
        var session = new Session { Id = sessionId, PaymentStatus = "paid", PaymentIntentId = "pi_test" };
        var payment = new Payment { Id = paymentId, ProviderPaymentId = sessionId, Status = PaymentStatus.Processing };
        var paymentResponse = new PaymentResponse { Id = paymentId };

        _mockStripeWrapper.Setup(x => x.GetCheckoutSessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _mockPaymentRepository.Setup(x => x.GetByProviderPaymentIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockPaymentRepository.Setup(x => x.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(x => x.Map<PaymentResponse>(It.IsAny<Payment>()))
            .Returns(paymentResponse);

        // Act
        var result = await _stripeService.VerifyPaymentAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockPaymentRepository.Verify(x => x.UpdateAsync(
            It.Is<Payment>(p => p.Status == PaymentStatus.Succeeded && p.ProviderChargeId == "pi_test"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyPaymentAsync_PaymentNotPaid_UpdatesStatusToFailed()
    {
        // Arrange
        var sessionId = "cs_test_123";
        var paymentId = Guid.NewGuid();
        var session = new Session { Id = sessionId, PaymentStatus = "unpaid" };
        var payment = new Payment { Id = paymentId, ProviderPaymentId = sessionId, Status = PaymentStatus.Processing };
        var paymentResponse = new PaymentResponse { Id = paymentId };

        _mockStripeWrapper.Setup(x => x.GetCheckoutSessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _mockPaymentRepository.Setup(x => x.GetByProviderPaymentIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockPaymentRepository.Setup(x => x.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(x => x.Map<PaymentResponse>(It.IsAny<Payment>()))
            .Returns(paymentResponse);

        // Act
        var result = await _stripeService.VerifyPaymentAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockPaymentRepository.Verify(x => x.UpdateAsync(
            It.Is<Payment>(p => p.Status == PaymentStatus.Failed && p.FailureMessage!.Contains("unpaid")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyPaymentAsync_StripeException_ReturnsFailure()
    {
        // Arrange
        var sessionId = "cs_test_123";

        _mockStripeWrapper.Setup(x => x.GetCheckoutSessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new StripeException("Session not found"));

        // Act
        var result = await _stripeService.VerifyPaymentAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Stripe error");
    }

    [Fact]
    public async Task VerifyPaymentAsync_GeneralException_ReturnsFailure()
    {
        // Arrange
        var sessionId = "cs_test_123";

        _mockStripeWrapper.Setup(x => x.GetCheckoutSessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _stripeService.VerifyPaymentAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error verifying payment");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task HandleWebhookEventAsync_CheckoutSessionCompleted_InvalidPaymentId_ReturnsFailure()
    {
        // Arrange
        var session = new Session { Id = "cs_test", ClientReferenceId = "invalid-guid" };
        var stripeEvent = new Event { Type = "checkout.session.completed", Data = new EventData { Object = session } };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid payment ID");
    }

    [Fact]
    public async Task HandleWebhookEventAsync_CheckoutSessionCompleted_PaymentNotFound_ReturnsFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var session = new Session { Id = "cs_test", ClientReferenceId = paymentId.ToString() };
        var stripeEvent = new Event { Type = "checkout.session.completed", Data = new EventData { Object = session } };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        _mockPaymentRepository.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Payment not found");
    }

    [Fact]
    public async Task HandleWebhookEventAsync_PaymentIntentSucceeded_NoPaymentFound_ReturnsSuccess()
    {
        // Arrange
        var paymentIntent = new PaymentIntent { Id = "pi_test" };
        var stripeEvent = new Event { Type = "payment_intent.succeeded", Data = new EventData { Object = paymentIntent } };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        _mockPaymentRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleWebhookEventAsync_PaymentIntentFailed_NoPaymentFound_ReturnsSuccess()
    {
        // Arrange
        var paymentIntent = new PaymentIntent { Id = "pi_test" };
        var stripeEvent = new Event { Type = "payment_intent.payment_failed", Data = new EventData { Object = paymentIntent } };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        _mockPaymentRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleWebhookEventAsync_CheckoutSessionCompleted_ExceptionDuringProcessing_ReturnsFailure()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var session = new Session { Id = "cs_test", ClientReferenceId = paymentId.ToString() };
        var stripeEvent = new Event { Type = "checkout.session.completed", Data = new EventData { Object = session } };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        _mockPaymentRepository.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error processing payment");
    }

    [Fact]
    public async Task HandleWebhookEventAsync_PaymentIntentSucceeded_ExceptionDuringProcessing_ReturnsFailure()
    {
        // Arrange
        var paymentIntent = new PaymentIntent { Id = "pi_test" };
        var stripeEvent = new Event { Type = "payment_intent.succeeded", Data = new EventData { Object = paymentIntent } };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        _mockPaymentRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error processing payment intent");
    }

    [Fact]
    public async Task HandleWebhookEventAsync_PaymentIntentFailed_ExceptionDuringProcessing_ReturnsFailure()
    {
        // Arrange
        var paymentIntent = new PaymentIntent { Id = "pi_test" };
        var stripeEvent = new Event { Type = "payment_intent.payment_failed", Data = new EventData { Object = paymentIntent } };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        _mockPaymentRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error processing payment intent");
    }

    [Fact]
    public async Task HandleWebhookEventAsync_PaymentIntentFailed_NullErrorMessage_UsesDefaultMessage()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var paymentIntent = new PaymentIntent { Id = "pi_test", LastPaymentError = null };
        var stripeEvent = new Event { Type = "payment_intent.payment_failed", Data = new EventData { Object = paymentIntent } };

        var payment = new Payment { Id = paymentId, ProviderChargeId = "pi_test", Status = PaymentStatus.Processing };
        var paymentResponse = new PaymentResponse { Id = paymentId };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        _mockPaymentRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockPaymentRepository.Setup(x => x.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(x => x.Map<PaymentResponse>(It.IsAny<Payment>()))
            .Returns(paymentResponse);

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockPaymentRepository.Verify(x => x.UpdateAsync(
            It.Is<Payment>(p => p.FailureMessage == "Payment failed"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region UpdatePaymentAndReturnResponseAsync Integration Tests

    [Fact]
    public async Task ProcessPaymentIntentSucceeded_VerifiesHelperMethodSetsUpdatedAtCorrectly()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var beforeUpdate = DateTime.UtcNow.AddMinutes(-1);
        var paymentIntent = new PaymentIntent 
        { 
            Id = "pi_test_helper",
            Amount = 5000,
            Currency = "ron",
            Status = "succeeded"
        };
        var stripeEvent = new Event { Type = "payment_intent.succeeded", Data = new EventData { Object = paymentIntent } };

        var payment = new Payment 
        { 
            Id = paymentId, 
            ProviderChargeId = "pi_test_helper", 
            Status = PaymentStatus.Processing,
            UpdatedAt = beforeUpdate
        };
        var paymentResponse = new PaymentResponse { Id = paymentId };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        _mockPaymentRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        Payment? capturedPayment = null;
        _mockPaymentRepository.Setup(x => x.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((p, _) => capturedPayment = p)
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(x => x.Map<PaymentResponse>(It.IsAny<Payment>()))
            .Returns(paymentResponse);

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        capturedPayment.Should().NotBeNull();
        capturedPayment!.UpdatedAt.Should().BeAfter(beforeUpdate);
        capturedPayment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ProcessPaymentIntentSucceeded_VerifiesHelperMethodSerializesProviderDataCorrectly()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var paymentIntent = new PaymentIntent 
        { 
            Id = "pi_test_serialization",
            Amount = 12345,
            Currency = "ron",
            Status = "succeeded",
            Description = "Test payment with complex data"
        };
        var stripeEvent = new Event { Type = "payment_intent.succeeded", Data = new EventData { Object = paymentIntent } };

        var payment = new Payment 
        { 
            Id = paymentId, 
            ProviderChargeId = "pi_test_serialization", 
            Status = PaymentStatus.Processing
        };
        var paymentResponse = new PaymentResponse { Id = paymentId };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        _mockPaymentRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        Payment? capturedPayment = null;
        _mockPaymentRepository.Setup(x => x.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((p, _) => capturedPayment = p)
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(x => x.Map<PaymentResponse>(It.IsAny<Payment>()))
            .Returns(paymentResponse);

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        capturedPayment.Should().NotBeNull();
        capturedPayment!.RawProviderResponse.Should().NotBeNullOrEmpty();
        capturedPayment.RawProviderResponse.Should().Contain("pi_test_serialization");
        capturedPayment.RawProviderResponse.Should().Contain("12345");
    }

    [Fact]
    public async Task ProcessPaymentIntentFailed_VerifiesHelperMethodCallsMapperCorrectly()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var paymentIntent = new PaymentIntent 
        { 
            Id = "pi_test_mapper",
            LastPaymentError = new StripeError { Message = "Card was declined" }
        };
        var stripeEvent = new Event { Type = "payment_intent.payment_failed", Data = new EventData { Object = paymentIntent } };

        var payment = new Payment 
        { 
            Id = paymentId, 
            ProviderChargeId = "pi_test_mapper", 
            Status = PaymentStatus.Processing
        };
        var paymentResponse = new PaymentResponse { Id = paymentId };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        _mockPaymentRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockPaymentRepository.Setup(x => x.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Payment? mappedPayment = null;
        _mockMapper.Setup(x => x.Map<PaymentResponse>(It.IsAny<Payment>()))
            .Callback<object>(p => mappedPayment = p as Payment)
            .Returns(paymentResponse);

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(paymentId);
        
        // Verify mapper was called with the updated payment
        _mockMapper.Verify(x => x.Map<PaymentResponse>(It.IsAny<Payment>()), Times.Once);
        mappedPayment.Should().NotBeNull();
        mappedPayment!.Status.Should().Be(PaymentStatus.Failed);
        mappedPayment.FailureMessage.Should().Be("Card was declined");
        mappedPayment.RawProviderResponse.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProcessPaymentIntentSucceeded_VerifiesHelperMethodReturnsCorrectSuccessResult()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var paymentIntent = new PaymentIntent { Id = "pi_test_result" };
        var stripeEvent = new Event { Type = "payment_intent.succeeded", Data = new EventData { Object = paymentIntent } };

        var payment = new Payment 
        { 
            Id = paymentId, 
            ProviderChargeId = "pi_test_result", 
            Status = PaymentStatus.Processing
        };
        var paymentResponse = new PaymentResponse 
        { 
            Id = paymentId,
            Amount = 100.00m
        };

        _mockStripeWrapper.Setup(x => x.ConstructWebhookEvent(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stripeEvent);

        _mockPaymentRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockPaymentRepository.Setup(x => x.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(x => x.Map<PaymentResponse>(It.IsAny<Payment>()))
            .Returns(paymentResponse);

        // Act
        var result = await _stripeService.HandleWebhookEventAsync("{}", "sig");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(paymentResponse);
        result.Error.Should().BeNull();
    }

    #endregion
}
