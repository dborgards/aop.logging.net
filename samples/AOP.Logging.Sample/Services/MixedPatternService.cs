using AOP.Logging.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace AOP.Logging.Sample.Services;

/// <summary>
/// Service demonstrating MIXED usage - both Core suffix and non-Core methods.
/// This shows backward compatibility with existing Core patterns.
/// </summary>
[LogClass]
public partial class MixedPatternService
{
    /// <summary>
    /// Old pattern: Method with "Core" suffix.
    /// Wrapper will be "ProcessData" (Core removed for backward compatibility).
    /// </summary>
    private async Task<int> ProcessDataCore(string data)
    {
        await Task.Delay(50);
        return data.Length;
    }

    /// <summary>
    /// New pattern: Method without "Core" suffix.
    /// Wrapper will be "ValidateInputLogged" (Logged suffix added).
    /// </summary>
    private bool ValidateInput(string input)
    {
        return !string.IsNullOrEmpty(input);
    }

    /// <summary>
    /// Old pattern: Another Core method.
    /// Wrapper will be "TransformData" (Core removed).
    /// </summary>
    [LogMethod(LogLevel.Debug)]
    private string TransformDataCore(string input)
    {
        return input.ToUpper();
    }

    /// <summary>
    /// New pattern: Method without Core.
    /// Wrapper will be "CalculateHashLogged" (Logged suffix added).
    /// </summary>
    private int CalculateHash(string value)
    {
        return value.GetHashCode();
    }
}
