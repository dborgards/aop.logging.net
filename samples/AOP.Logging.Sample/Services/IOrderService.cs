namespace AOP.Logging.Sample.Services;

/// <summary>
/// Order service interface (demonstrating methods without Core suffix).
/// </summary>
public interface IOrderService
{
    Task<string> CreateOrderLogged(string customerId, decimal amount);
    Task<bool> ProcessPaymentLogged(string orderId, decimal amount);
    void CancelOrderLogged(string orderId);
    Task<string> GetOrderStatusLogged(string orderId);
}
