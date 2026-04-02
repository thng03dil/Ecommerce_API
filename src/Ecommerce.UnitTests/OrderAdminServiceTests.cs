using Ecommerce.Application.Common.Caching;
using Ecommerce.Application.Common.Pagination;
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

public class OrderAdminServiceTests
{
    private readonly Mock<IOrderRepo> _orderRepo = new();
    private readonly Mock<ICacheService> _cacheService = new();
    private readonly Mock<IOrderPaymentService> _paymentService = new();
    private readonly OrderAdminService _sut;

    public OrderAdminServiceTests()
    {
        _sut = new OrderAdminService(_orderRepo.Object, _cacheService.Object, _paymentService.Object);
    }

    #region GetAllOrdersAsync Tests
    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnPagedData()
    {
        // Arrange
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };
        var orders = new List<Order> { new() { Id = 1, OrderItems = new List<OrderItem>() } };
        _orderRepo.Setup(x => x.GetAllPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((orders, 1));

        // Act
        var result = await _sut.GetAllOrdersAsync(pagination);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Data.Should().HaveCount(1);
        result.Data.TotalCount.Should().Be(1);
    }
    #endregion

    #region CancelOrderAsync Tests
    [Fact]
    public async Task CancelOrderAsync_WhenOrderNotFound_ShouldThrowNotFound()
    {
        // Arrange
        _orderRepo.Setup(x => x.GetByIdWithItemsAndProductsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        await _sut.Invoking(s => s.CancelOrderAsync(99))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CancelOrderAsync_WhenPaid_ShouldCallRefund()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            PaymentStatus = PaymentStatus.Succeeded,
            StripePaymentIntentId = "pi_123"
        };
        _orderRepo.Setup(x => x.GetByIdWithItemsAndProductsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentService.Setup(x => x.RefundAsync("pi_123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _orderRepo.Setup(x => x.TryCancelOrderByAdminAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OrderCancelResult.Ok(1, 1, 100m, DateTime.UtcNow, null, PaymentStatus.Refunded));

        // Act
        await _sut.CancelOrderAsync(1);

        // Assert
        _paymentService.Verify(x => x.RefundAsync("pi_123", It.IsAny<CancellationToken>()), Times.Once);
        _cacheService.Verify(x => x.IncrementAsync(CacheKeyGenerator.CategoryVersionKey()), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_WhenRefundFails_ShouldThrowConflict()
    {
        // Arrange
        var order = new Order { Id = 1, PaymentStatus = PaymentStatus.Succeeded, StripePaymentIntentId = "pi_err" };
        _orderRepo.Setup(x => x.GetByIdWithItemsAndProductsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentService.Setup(x => x.RefundAsync("pi_err", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await _sut.Invoking(s => s.CancelOrderAsync(1))
            .Should().ThrowAsync<ConflictException>().WithMessage("Payment refund could not be processed.");
    }
    #endregion

    #region ApproveReturnAsync Tests
    [Fact]
    public async Task ApproveReturnAsync_WhenNotAwaitingReturn_ShouldThrowConflict()
    {
        // Arrange
        var order = new Order { Id = 1, Status = OrderStatus.Paid }; // Không phải ReturnRequested
        _orderRepo.Setup(x => x.GetByIdWithItemsAndProductsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        await _sut.Invoking(s => s.ApproveReturnAsync(1))
            .Should().ThrowAsync<ConflictException>();
    }
    #endregion

    #region UpdateOrderStatusAsync Tests
    [Fact]
    public async Task UpdateOrderStatusAsync_WhenRepoFails_ShouldReturn409()
    {
        // Arrange
        var order = new Order { Id = 1 };
        _orderRepo.Setup(x => x.GetByIdTrackedAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Giả lập repo trả về false (do vi phạm logic workflow hoặc chưa thanh toán)
        _orderRepo.Setup(x => x.TryUpdateStatusByAdminAsync(1, It.IsAny<OrderStatus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.UpdateOrderStatusAsync(1, AdminOrderFulfillmentStatus.Shipping, default);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("advance the workflow");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WhenSuccess_ShouldReturnSuccessResponse()
    {
        // Arrange
        var order = new Order { Id = 1, Status = OrderStatus.Paid, OrderItems = new List<OrderItem>() };
        _orderRepo.Setup(x => x.GetByIdTrackedAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _orderRepo.Setup(x => x.TryUpdateStatusByAdminAsync(1, OrderStatus.Shipping, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var updatedOrder = new Order { Id = 1, Status = OrderStatus.Shipping, OrderItems = new List<OrderItem>() };
        _orderRepo.Setup(x => x.GetByIdWithItemsAndProductsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(updatedOrder);

        // Act
        var result = await _sut.UpdateOrderStatusAsync(1, AdminOrderFulfillmentStatus.Shipping, default);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("successfully updated to Shipping");
    }
    #endregion
}