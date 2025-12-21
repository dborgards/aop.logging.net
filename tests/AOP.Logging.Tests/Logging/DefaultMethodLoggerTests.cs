using AOP.Logging.Core.Configuration;
using AOP.Logging.Core.Logging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace AOP.Logging.Tests.Logging;

public class DefaultMethodLoggerTests
{
    private readonly ILogger<DefaultMethodLogger> _mockLogger;
    private readonly AopLoggingOptions _options;
    private readonly DefaultMethodLogger _methodLogger;

    public DefaultMethodLoggerTests()
    {
        _mockLogger = Substitute.For<ILogger<DefaultMethodLogger>>();
        _options = new AopLoggingOptions();
        var optionsWrapper = Options.Create(_options);
        _methodLogger = new DefaultMethodLogger(_mockLogger, optionsWrapper);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var action = () => new DefaultMethodLogger(null!, Options.Create(_options));

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var action = () => new DefaultMethodLogger(_mockLogger, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void LogEntry_WithParameters_LogsCorrectly()
    {
        // Arrange
        _mockLogger.IsEnabled(LogLevel.Information).Returns(true);
        var parameters = new Dictionary<string, object?>
        {
            ["param1"] = "value1",
            ["param2"] = 42
        };

        // Act
        _methodLogger.LogEntry("MyClass", "MyMethod", parameters, LogLevel.Information);

        // Assert
        _mockLogger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void LogEntry_WhenLogLevelNotEnabled_DoesNotLog()
    {
        // Arrange
        _mockLogger.IsEnabled(LogLevel.Information).Returns(false);
        var parameters = new Dictionary<string, object?>();

        // Act
        _methodLogger.LogEntry("MyClass", "MyMethod", parameters, LogLevel.Information);

        // Assert
        _mockLogger.DidNotReceive().Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void LogExit_WithReturnValue_LogsCorrectly()
    {
        // Arrange
        _mockLogger.IsEnabled(LogLevel.Information).Returns(true);
        var returnValue = "result";
        var executionTime = 150L;

        // Act
        _methodLogger.LogExit("MyClass", "MyMethod", returnValue, executionTime, LogLevel.Information);

        // Assert
        _mockLogger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void LogException_WithException_LogsCorrectly()
    {
        // Arrange
        _mockLogger.IsEnabled(LogLevel.Error).Returns(true);
        var exception = new InvalidOperationException("Test exception");
        var executionTime = 100L;

        // Act
        _methodLogger.LogException("MyClass", "MyMethod", exception, executionTime, LogLevel.Error);

        // Assert
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Is<Exception>(ex => ex == exception),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void LogEntry_WithStructuredLogging_OnlyIncludesParametersInStateWhenReferencedInTemplate()
    {
        // Arrange - Security fix: prevent unintended data leakage
        _mockLogger.IsEnabled(LogLevel.Information).Returns(true);
        _options.UseStructuredLogging = true;
        _options.EntryMessageFormat = "Entering {ClassName}.{MethodName}"; // Does NOT include {Parameters}

        var parameters = new Dictionary<string, object?>
        {
            ["password"] = "secret123",
            ["username"] = "testuser"
        };

        Dictionary<string, object?>? capturedState = null;
        _mockLogger.Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Do<object>(state => capturedState = state as Dictionary<string, object?>),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

        // Act
        _methodLogger.LogEntry("MyClass", "MyMethod", parameters, LogLevel.Information);

        // Assert
        capturedState.Should().NotBeNull();
        capturedState!.Should().ContainKey("ClassName");
        capturedState.Should().ContainKey("MethodName");
        capturedState.Should().NotContainKey("Parameters"); // Security: should NOT leak parameters
        capturedState.Should().NotContainKey("Param_password"); // Security: should NOT leak individual params
        capturedState.Should().NotContainKey("Param_username");
    }

    [Fact]
    public void LogEntry_WithStructuredLogging_IncludesParametersInStateWhenReferencedInTemplate()
    {
        // Arrange
        _mockLogger.IsEnabled(LogLevel.Information).Returns(true);
        _options.UseStructuredLogging = true;
        _options.EntryMessageFormat = "Entering {ClassName}.{MethodName} with {Parameters}"; // DOES include {Parameters}

        var parameters = new Dictionary<string, object?>
        {
            ["param1"] = "value1"
        };

        Dictionary<string, object?>? capturedState = null;
        _mockLogger.Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Do<object>(state => capturedState = state as Dictionary<string, object?>),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

        // Act
        _methodLogger.LogEntry("MyClass", "MyMethod", parameters, LogLevel.Information);

        // Assert
        capturedState.Should().NotBeNull();
        capturedState!.Should().ContainKey("Parameters"); // Should include when referenced
    }

    [Fact]
    public void LogExit_WithStructuredLogging_OnlyIncludesReturnValueInStateWhenReferencedInTemplate()
    {
        // Arrange - Security fix: prevent unintended data leakage
        _mockLogger.IsEnabled(LogLevel.Information).Returns(true);
        _options.UseStructuredLogging = true;
        _options.ExitMessageFormat = "Exiting {ClassName}.{MethodName}"; // Does NOT include {ReturnValue}

        var sensitiveReturnValue = "credit-card-number-1234-5678-9012-3456";

        Dictionary<string, object?>? capturedState = null;
        _mockLogger.Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Do<object>(state => capturedState = state as Dictionary<string, object?>),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

        // Act
        _methodLogger.LogExit("MyClass", "MyMethod", sensitiveReturnValue, 100L, LogLevel.Information);

        // Assert
        capturedState.Should().NotBeNull();
        capturedState!.Should().ContainKey("ClassName");
        capturedState.Should().ContainKey("MethodName");
        capturedState.Should().NotContainKey("ReturnValue"); // Security: should NOT leak return value
        capturedState.Should().NotContainKey("ExecutionTime"); // Should not include when not referenced
    }

    [Fact]
    public void LogExit_WithStructuredLogging_IncludesReturnValueInStateWhenReferencedInTemplate()
    {
        // Arrange
        _mockLogger.IsEnabled(LogLevel.Information).Returns(true);
        _options.UseStructuredLogging = true;
        _options.ExitMessageFormat = "Exiting {ClassName}.{MethodName} with result {ReturnValue}"; // DOES include {ReturnValue}

        Dictionary<string, object?>? capturedState = null;
        _mockLogger.Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Do<object>(state => capturedState = state as Dictionary<string, object?>),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

        // Act
        _methodLogger.LogExit("MyClass", "MyMethod", "result", 100L, LogLevel.Information);

        // Assert
        capturedState.Should().NotBeNull();
        capturedState!.Should().ContainKey("ReturnValue"); // Should include when referenced
    }

    [Fact]
    public void FormatValue_WithLargeEnumerable_DoesNotEnumerateEntireCollection()
    {
        // Arrange - Security fix: prevent DoS from unbounded enumeration
        _mockLogger.IsEnabled(LogLevel.Information).Returns(true);
        _options.MaxCollectionSize = 10;

        var enumerationCount = 0;
        IEnumerable<int> InfiniteEnumerable()
        {
            while (true)
            {
                enumerationCount++;
                yield return enumerationCount;
            }
        }

        var parameters = new Dictionary<string, object?>
        {
            ["data"] = InfiniteEnumerable()
        };

        // Act
        _methodLogger.LogEntry("MyClass", "MyMethod", parameters, LogLevel.Information);

        // Assert - Should only enumerate up to MaxCollectionSize + 1 (to check if it exceeds)
        // This is the critical security fix: bounded enumeration prevents DoS
        enumerationCount.Should().BeLessOrEqualTo(_options.MaxCollectionSize + 1);

        // Verify the log method was called
        _mockLogger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
