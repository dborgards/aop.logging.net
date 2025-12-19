using AOP.Logging.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace AOP.Logging.Sample.Services;

/// <summary>
/// Calculator service with automatic method logging.
/// </summary>
[LogClass(LogLevel.Information)]
public partial class CalculatorService : ICalculatorService
{
    /// <summary>
    /// Adds two numbers.
    /// </summary>
    public int Add(int a, int b)
    {
        return a + b;
    }

    /// <summary>
    /// Multiplies two numbers.
    /// </summary>
    [LogMethod(LogLevel.Debug)]
    public int Multiply(int a, int b)
    {
        return a * b;
    }

    /// <summary>
    /// Divides two numbers. Throws exception on division by zero.
    /// </summary>
    [LogException]
    public double Divide(int a, int b)
    {
        if (b == 0)
        {
            throw new DivideByZeroException("Cannot divide by zero");
        }
        return (double)a / b;
    }

    /// <summary>
    /// Performs an async calculation.
    /// </summary>
    public async Task<double> CalculateAsync(int a, int b)
    {
        await Task.Delay(100); // Simulate async work
        return Math.Sqrt(a * a + b * b);
    }

    /// <summary>
    /// Performs a complex calculation with custom log level.
    /// </summary>
    [LogMethod(LogLevel.Warning)]
    public void PerformComplexCalculation(int iterations)
    {
        var result = 0.0;
        for (int i = 0; i < iterations; i++)
        {
            result += Math.Sin(i) * Math.Cos(i);
        }
    }
}
