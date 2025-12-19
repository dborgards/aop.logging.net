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
}
