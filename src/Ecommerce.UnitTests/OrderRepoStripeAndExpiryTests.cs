using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Ecommerce.UnitTests;

public class OrderRepoStripeAndExpiryTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    #region Stripe & Payment Tests

    [Fact]
    public async Task TryMarkPaidByStripeSessionAsync_WhenPending_ShouldUpdateToPaid()
    {
        // Arrange
        await using var ctx = CreateContext();
        var sessionId = "cs_test_123";
        ctx.Orders.Add(new Order
        {
            UserId = 1,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.NotPaid,
            StripeCheckoutSessionId = sessionId
        });
        await ctx.SaveChangesAsync();
        var repo = new OrderRepo(ctx);

        // Act
        var result = await repo.TryMarkPaidByStripeSessionAsync(sessionId, "pi_success");

        // Assert
        result.Should().BeTrue();
        var order = await ctx.Orders.FirstAsync();
        order.Status.Should().Be(OrderStatus.Paid);
        order.PaymentStatus.Should().Be(PaymentStatus.Succeeded);
        order.StripePaymentIntentId.Should().Be("pi_success");
        order.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public async Task TryMarkPaidByStripeSessionAsync_IsIdempotent()
    {
        // Arrange
        await using var ctx = CreateContext();
        var sessionId = "cs_idempotent";
        ctx.Orders.Add(new Order { UserId = 1, Status = OrderStatus.Paid, StripeCheckoutSessionId = sessionId });
        await ctx.SaveChangesAsync();
        var repo = new OrderRepo(ctx);

        // Act
        var result = await repo.TryMarkPaidByStripeSessionAsync(sessionId, "pi_already_done");

        // Assert
        result.Should().BeTrue(); // Đã Paid rồi thì trả về true nhưng không update lại PaidAt
    }

    [Fact]
    public async Task TryApplyPaymentFailedAsync_ShouldUpdateErrorAndStatus()
    {
        // Arrange
        await using var ctx = CreateContext();
        var order = new Order { UserId = 1, Status = OrderStatus.Pending, PaymentStatus = PaymentStatus.NotPaid };
        ctx.Orders.Add(order);
        await ctx.SaveChangesAsync();
        var repo = new OrderRepo(ctx);

        // Act
        var result = await repo.TryMarkPaymentFailedByOrderIdAsync(order.Id, "Card Declined");

        // Assert
        result.Should().BeTrue();
        var updated = await ctx.Orders.FirstAsync();
        updated.PaymentStatus.Should().Be(PaymentStatus.Failed);
        updated.LastPaymentError.Should().Be("Card Declined");
    }

    #endregion

    #region Cancellation & Stock Revert Tests

   
    [Fact]
    public async Task TryCancelOrderByUserAsync_WhenStatusIsShipping_ShouldFail()
    {
        // Arrange
        await using var ctx = CreateContext();
        var order = new Order { UserId = 1, Status = OrderStatus.Shipping };
        ctx.Orders.Add(order);
        await ctx.SaveChangesAsync();
        var repo = new OrderRepo(ctx);

        // Act
        var result = await repo.TryCancelOrderByUserAsync(order.Id, 1);

        // Assert
        result.Success.Should().BeFalse();
        result.Failure.Should().Be(OrderCancelFailure.NotCancellable);
    }

    #endregion

    #region Return Request Tests

    [Fact]
    public async Task TryRequestReturnByUserAsync_WhenCompletedAndPaid_ShouldSucceed()
    {
        // Arrange
        await using var ctx = CreateContext();
        var order = new Order
        {
            UserId = 1,
            Status = OrderStatus.Completed,
            PaymentStatus = PaymentStatus.Succeeded
        };
        ctx.Orders.Add(order);
        await ctx.SaveChangesAsync();
        var repo = new OrderRepo(ctx);

        // Act
        var result = await repo.TryRequestReturnByUserAsync(order.Id, 1);

        // Assert
        result.Success.Should().BeTrue();
        var updated = await ctx.Orders.FirstAsync();
        updated.Status.Should().Be(OrderStatus.ReturnRequested);
    }

    #endregion
}