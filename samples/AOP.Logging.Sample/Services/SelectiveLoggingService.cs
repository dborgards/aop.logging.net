using AOP.Logging.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace AOP.Logging.Sample.Services;

/// <summary>
/// Service demonstrating selective method logging without [LogClass].
/// Only methods with [LogMethod] will be logged.
/// </summary>
public partial class SelectiveLoggingService
{
    /// <summary>
    /// This method will be logged (has [LogMethod] attribute).
    /// Wrapper will be "ImportantOperationLogged".
    /// </summary>
    [LogMethod(LogLevel.Information)]
    private async Task<bool> ImportantOperation(string data)
    {
        await Task.Delay(100);
        return !string.IsNullOrEmpty(data);
    }

    /// <summary>
    /// This method will NOT be logged (no [LogMethod] and no [LogClass]).
    /// </summary>
    private void UnloggedHelper(string data)
    {
        // This won't be logged
    }

    /// <summary>
    /// This method will be logged (has [LogMethod] attribute).
    /// Wrapper will be "CriticalCheckLogged".
    /// </summary>
    [LogMethod(LogLevel.Warning)]
    private bool CriticalCheck(int value)
    {
        return value > 0;
    }

    /// <summary>
    /// Core method with [LogMethod] - tests combination.
    /// Wrapper will be "ProcessItem" (Core removed).
    /// </summary>
    [LogMethod]
    private void ProcessItemCore(string item)
    {
        // Process item
    }
}
