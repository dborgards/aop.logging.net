using AOP.Logging.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace AOP.Logging.Sample.Services;

/// <summary>
/// Order service demonstrating logging WITHOUT the Core suffix pattern.
/// This shows the new capability where methods don't need to end with "Core".
/// The Source Generator creates wrapper methods with "Logged" suffix.
/// </summary>
[LogClass]
public partial class OrderService : IOrderService
{
    /// <summary>
    /// Creates a new order. No "Core" suffix - wrapper will be "CreateOrderLogged".
    /// </summary>
    private async Task<string> CreateOrder(string customerId, decimal amount)
    {
        await Task.Delay(50); // Simulate async work

        var orderId = Guid.NewGuid().ToString();
        return orderId;
    }

    /// <summary>
    /// Processes payment for an order. Uses specific log level.
    /// Wrapper will be "ProcessPaymentLogged".
    /// </summary>
    [LogMethod(LogLevel.Warning)]
    private async Task<bool> ProcessPayment(string orderId, decimal amount)
    {
        await Task.Delay(100); // Simulate payment processing

        // Simulate payment success
        return amount > 0;
    }

    /// <summary>
    /// Cancels an order. Synchronous method without Core suffix.
    /// Wrapper will be "CancelOrderLogged".
    /// </summary>
    private void CancelOrder(string orderId)
    {
        // Simulate cancellation logic
    }

    /// <summary>
    /// Gets order status. Returns a specific type.
    /// Wrapper will be "GetOrderStatusLogged".
    /// </summary>
    [LogMethod(LogLevel.Debug)]
    private async Task<string> GetOrderStatus(string orderId)
    {
        await Task.Delay(10); // Simulate async lookup
        return "Pending";
    }
}
