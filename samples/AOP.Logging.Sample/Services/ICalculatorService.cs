namespace AOP.Logging.Sample.Services;

/// <summary>
/// Calculator service interface.
/// </summary>
public interface ICalculatorService
{
    int Add(int a, int b);
    int Multiply(int a, int b);
    double Divide(int a, int b);
    Task<double> CalculateAsync(int a, int b);
    void PerformComplexCalculation(int iterations);
}
