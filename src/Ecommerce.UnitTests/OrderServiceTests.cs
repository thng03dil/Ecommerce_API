using Ecommerce.Application.Common.Caching;
using Ecommerce.Application.DTOs.Order;
using Ecommerce.Application.DTOs.OrderDtos;
using Ecommerce.Application.Exceptions;
using Ecommerce.Application.Services.Implementations;
using Ecommerce.Application.Services.Interfaces;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepo> _orderRepo = new();
    private readonly Mock<ICacheService> _cacheService = new();
    private readonly Mock<IOrderPaymentService> _paymentService = new();
    private readonly IOrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(_orderRepo.Object, _cacheService.Object, _paymentService.Object);
    }

    #region PlaceOrderAsync Tests
    [Fact]
    public async Task PlaceOrderAsync_WhenUserIdInvalid_ShouldReturn401()
    {
        var dto = new CreateOrderDto { Items = new List<OrderItemRequestDto> { new() { ProductId = 1, Quantity = 1 } } };

        var result = await _sut.PlaceOrderAsync(0, dto);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task PlaceOrderAsync_WhenRepoSucceeds_ShouldIncrementCacheAndReturnSuccess()
    {
        // Arrange
        int userId = 1;
        var createdDate = DateTime.UtcNow;
        var dto = new CreateOrderDto { Items = new List<OrderItemRequestDto> { new() { ProductId = 10, Quantity = 1 } } };

        _orderRepo.Setup(x => x.PlaceOrderAsync(userId, It.IsAny<IReadOnlyList<OrderLineInput>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OrderPlaceResult.Ok(99, 100m, createdDate));

        var orderEntity = new Order
        {
            Id = 99,
            UserId = userId,
            TotalAmount = 100m,
            CreatedAt = createdDate,
            OrderItems = new List<OrderItem>()
        };
        _orderRepo.Setup(x => x.GetByIdForUserWithItemsAndProductsAsync(99, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderEntity);

        // Act
        var result = await _sut.PlaceOrderAsync(userId, dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(99);
        _cacheService.Verify(x => x.IncrementAsync(CacheKeyGenerator.CategoryVersionKey()), Times.Once);
    }
    #endregion

    #region CancelPendingOrderAsync Tests
    [Fact]
    public async Task CancelPendingOrderAsync_WhenPaymentSucceeded_ShouldCallRefund()
    {
        // Arrange
        int userId = 1; int orderId = 7;
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            PaymentStatus = PaymentStatus.Succeeded,
            StripePaymentIntentId = "pi_test"
        };

        _orderRepo.Setup(x => x.GetByIdForUserWithItemsAndProductsAsync(orderId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Giả lập Refund thất bại
        _paymentService.Setup(x => x.RefundAsync("pi_test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var act = async () => await _sut.CancelPendingOrderAsync(userId, orderId);
        await act.Should().ThrowAsync<ConflictException>().WithMessage("*refund could not be processed*");
    }

    [Fact]
    public async Task CancelPendingOrderAsync_WhenOk_ShouldIncrementCache()
    {
        // Arrange
        int userId = 1; int orderId = 7;
        var order = new Order { Id = orderId, UserId = userId, PaymentStatus = PaymentStatus.NotPaid };

        _orderRepo.Setup(x => x.GetByIdForUserWithItemsAndProductsAsync(orderId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _orderRepo.Setup(x => x.TryCancelOrderByUserAsync(orderId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OrderCancelResult.Ok(orderId, userId, 100m, DateTime.UtcNow, null, PaymentStatus.Cancelled));

        // Act
        await _sut.CancelPendingOrderAsync(userId, orderId);

        // Assert
        _cacheService.Verify(x => x.IncrementAsync(CacheKeyGenerator.CategoryVersionKey()), Times.Once);
    }
    #endregion

    #region RequestReturnAsync Tests (New)
    [Fact]
    public async Task RequestReturnAsync_WhenOrderNotEligible_ShouldReturn400()
    {
        // Arrange
        _orderRepo.Setup(x => x.TryRequestReturnByUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OrderReturnRequestResult.Fail(OrderReturnRequestFailure.NotEligible));

        // Act
        var result = await _sut.RequestReturnAsync(1, 99);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("completed orders");
    }

    [Fact]
    public async Task RequestReturnAsync_WhenSuccess_ShouldReturnSuccessResponse()
    {
        // Arrange
        int userId = 1; int orderId = 99;
        _orderRepo.Setup(x => x.TryRequestReturnByUserAsync(orderId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OrderReturnRequestResult.Ok());

        var order = new Order { Id = orderId, UserId = userId, Status = OrderStatus.ReturnRequested, OrderItems = new List<OrderItem>() };
        _orderRepo.Setup(x => x.GetByIdForUserWithItemsAndProductsAsync(orderId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _sut.RequestReturnAsync(userId, orderId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Return request submitted.");
    }
    #endregion
}