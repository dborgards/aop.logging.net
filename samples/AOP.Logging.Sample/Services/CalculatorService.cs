using AOP.Logging.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace AOP.Logging.Sample.Services;

/// <summary>
/// Calculator service with automatic method logging.
/// The Source Generator creates public wrapper methods that call these private Core methods.
/// </summary>
[LogClass(LogLevel.Information)]
public partial class CalculatorService : ICalculatorService
{
    /// <summary>
    /// Core implementation: Adds two numbers.
    /// </summary>
    private int AddCore(int a, int b)
    {
        return a + b;
    }

    /// <summary>
    /// Core implementation: Multiplies two numbers.
    /// </summary>
    [LogMethod(LogLevel.Debug)]
    private int MultiplyCore(int a, int b)
    {
        return a * b;
    }

    /// <summary>
    /// Core implementation: Divides two numbers. Throws exception on division by zero.
    /// </summary>
    [LogException]
    private double DivideCore(int a, int b)
    {
        if (b == 0)
        {
            throw new DivideByZeroException("Cannot divide by zero");
        }
        return (double)a / b;
    }

    /// <summary>
    /// Core implementation: Performs an async calculation.
    /// </summary>
    private async Task<double> CalculateAsyncCore(int a, int b)
    {
        await Task.Delay(100); // Simulate async work
        return Math.Sqrt(a * a + b * b);
    }

    /// <summary>
    /// Core implementation: Performs a complex calculation with custom log level.
    /// </summary>
    [LogMethod(LogLevel.Warning)]
    private void PerformComplexCalculationCore(int iterations)
    {
        var result = 0.0;
        for (int i = 0; i < iterations; i++)
        {
            result += Math.Sin(i) * Math.Cos(i);
        }
    }
}
