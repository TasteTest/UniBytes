using backend.Common;
using backend.Common.Enums;
using backend.Controllers;
using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Xunit;

namespace Backend.Tests.Controllers;

public class PaymentsControllerTests
{
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly Mock<IStripeService> _mockStripeService;
    private readonly Mock<ILogger<PaymentsController>> _mockLogger;
    private readonly PaymentsController _controller;
    private readonly DefaultHttpContext _httpContext;

    public PaymentsControllerTests()
    {
        _mockPaymentService = new Mock<IPaymentService>();
        _mockStripeService = new Mock<IStripeService>();
        _mockLogger = new Mock<ILogger<PaymentsController>>();
        _httpContext = new DefaultHttpContext();
        _controller = new PaymentsController(_mockPaymentService.Object, _mockStripeService.Object, _mockLogger.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            }
        };
    }

    [Fact]
    public async Task GetPaymentById_ReturnsOk_WhenPaymentExists()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new PaymentResponse
        {
            Id = paymentId,
            Amount = 99.99m,
            Currency = "USD",
            Status = PaymentStatus.Succeeded
        };

        _mockPaymentService.Setup(x => x.GetPaymentByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentResponse>.Success(payment));

        // Act
        var result = await _controller.GetPaymentById(paymentId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(payment);
    }

    [Fact]
    public async Task GetPaymentById_ReturnsNotFound_WhenPaymentNotFound()
    {
        // Arrange
        var paymentId = Guid.NewGuid();

        _mockPaymentService.Setup(x => x.GetPaymentByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentResponse>.Failure("Payment not found"));

        // Act
        var result = await _controller.GetPaymentById(paymentId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetPaymentByOrderId_ReturnsOk_WhenPaymentExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var payment = new PaymentResponse
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = 49.99m
        };

        _mockPaymentService.Setup(x => x.GetPaymentByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentResponse>.Success(payment));

        // Act
        var result = await _controller.GetPaymentByOrderId(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPaymentByOrderId_ReturnsNotFound_WhenPaymentNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mockPaymentService.Setup(x => x.GetPaymentByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentResponse>.Failure("Payment not found"));

        // Act
        var result = await _controller.GetPaymentByOrderId(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetPaymentsByUserId_ReturnsOk_WhenPaymentsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var payments = new List<PaymentResponse>
        {
            new PaymentResponse { Id = Guid.NewGuid(), UserId = userId, Amount = 29.99m },
            new PaymentResponse { Id = Guid.NewGuid(), UserId = userId, Amount = 39.99m }
        };

        _mockPaymentService.Setup(x => x.GetPaymentsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<PaymentResponse>>.Success(payments));

        // Act
        var result = await _controller.GetPaymentsByUserId(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPaymentsByUserId_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockPaymentService.Setup(x => x.GetPaymentsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<PaymentResponse>>.Failure("Error"));

        // Act
        var result = await _controller.GetPaymentsByUserId(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateCheckoutSession_ReturnsOk_WhenSessionCreated()
    {
        // Arrange
        var request = new CreateCheckoutSessionRequest
        {
            UserEmail = "test@example.com",
            OrderId = Guid.NewGuid(),
            LineItems = new List<CheckoutLineItem>
            {
                new CheckoutLineItem { UnitPrice = 10m, Quantity = 2 }
            }
        };

        var response = new CheckoutSessionResponse
        {
            SessionId = "session_123",
            SessionUrl = "https://checkout.stripe.com/session_123"
        };

        _mockStripeService.Setup(x => x.CreateCheckoutSessionAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CheckoutSessionResponse>.Success(response));

        // Act
        var result = await _controller.CreateCheckoutSession(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreateCheckoutSession_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        var request = new CreateCheckoutSessionRequest
        {
            UserEmail = "test@example.com",
            OrderId = Guid.NewGuid(),
            LineItems = new List<CheckoutLineItem>()
        };

        _mockStripeService.Setup(x => x.CreateCheckoutSessionAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CheckoutSessionResponse>.Failure("Error"));

        // Act
        var result = await _controller.CreateCheckoutSession(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task VerifyPayment_ReturnsOk_WhenPaymentVerified()
    {
        // Arrange
        var sessionId = "session_123";
        var payment = new PaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Succeeded
        };

        _mockStripeService.Setup(x => x.VerifyPaymentAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentResponse>.Success(payment));

        // Act
        var result = await _controller.VerifyPayment(sessionId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task VerifyPayment_ReturnsBadRequest_WhenVerificationFails()
    {
        // Arrange
        var sessionId = "invalid_session";

        _mockStripeService.Setup(x => x.VerifyPaymentAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentResponse>.Failure("Payment not found"));

        // Act
        var result = await _controller.VerifyPayment(sessionId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task StripeWebhook_ReturnsBadRequest_WhenSignatureMissing()
    {
        // Arrange
        _httpContext.Request.Headers.Clear();
        _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{}"));

        // Act
        var result = await _controller.StripeWebhook(CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task StripeWebhook_ReturnsOk_WhenWebhookHandled()
    {
        // Arrange
        var json = "{\"type\":\"payment_intent.succeeded\"}";
        var signature = "signature_123";
        
        _httpContext.Request.Headers["Stripe-Signature"] = signature;
        _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var payment = new PaymentResponse { Id = Guid.NewGuid() };
        _mockStripeService.Setup(x => x.HandleWebhookEventAsync(json, signature, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentResponse>.Success(payment));

        // Act
        var result = await _controller.StripeWebhook(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task StripeWebhook_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        var json = "{\"type\":\"invalid\"}";
        var signature = "signature_123";
        
        _httpContext.Request.Headers["Stripe-Signature"] = signature;
        _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

        _mockStripeService.Setup(x => x.HandleWebhookEventAsync(json, signature, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentResponse>.Failure("Invalid webhook"));

        // Act
        var result = await _controller.StripeWebhook(CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}

