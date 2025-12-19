using AOP.Logging.Core.Attributes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AOP.Logging.Tests.Attributes;

public class AttributeTests
{
    [Fact]
    public void LogClassAttribute_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var attribute = new LogClassAttribute();

        // Assert
        attribute.LogLevel.Should().Be(LogLevel.Information);
        attribute.LogExecutionTime.Should().BeTrue();
        attribute.LogParameters.Should().BeTrue();
        attribute.LogReturnValue.Should().BeTrue();
        attribute.LogExceptions.Should().BeTrue();
    }

    [Fact]
    public void LogClassAttribute_WithLogLevel_SetsLogLevelCorrectly()
    {
        // Arrange & Act
        var attribute = new LogClassAttribute(LogLevel.Debug);

        // Assert
        attribute.LogLevel.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public void LogMethodAttribute_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var attribute = new LogMethodAttribute();

        // Assert
        attribute.LogLevel.Should().Be(LogLevel.Information);
        attribute.LogExecutionTime.Should().BeTrue();
        attribute.LogParameters.Should().BeTrue();
        attribute.LogReturnValue.Should().BeTrue();
        attribute.LogExceptions.Should().BeTrue();
        attribute.Skip.Should().BeFalse();
        attribute.EntryMessage.Should().BeNull();
        attribute.ExitMessage.Should().BeNull();
    }

    [Fact]
    public void LogMethodAttribute_WithLogLevel_SetsLogLevelCorrectly()
    {
        // Arrange & Act
        var attribute = new LogMethodAttribute(LogLevel.Warning);

        // Assert
        attribute.LogLevel.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void LogParameterAttribute_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var attribute = new LogParameterAttribute();

        // Assert
        attribute.Skip.Should().BeFalse();
        attribute.Name.Should().BeNull();
        attribute.MaxLength.Should().Be(-1);
    }

    [Fact]
    public void LogParameterAttribute_WithName_SetsNameCorrectly()
    {
        // Arrange & Act
        var attribute = new LogParameterAttribute("customName");

        // Assert
        attribute.Name.Should().Be("customName");
    }

    [Fact]
    public void LogResultAttribute_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var attribute = new LogResultAttribute();

        // Assert
        attribute.Skip.Should().BeFalse();
        attribute.Name.Should().BeNull();
        attribute.MaxLength.Should().Be(-1);
    }

    [Fact]
    public void LogExceptionAttribute_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var attribute = new LogExceptionAttribute();

        // Assert
        attribute.LogLevel.Should().Be(LogLevel.Error);
        attribute.IncludeDetails.Should().BeTrue();
        attribute.Rethrow.Should().BeTrue();
        attribute.Message.Should().BeNull();
    }

    [Fact]
    public void SensitiveDataAttribute_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var attribute = new SensitiveDataAttribute();

        // Assert
        attribute.MaskValue.Should().Be("***SENSITIVE***");
        attribute.ShowLength.Should().BeFalse();
    }

    [Fact]
    public void SensitiveDataAttribute_WithCustomMask_SetsMaskCorrectly()
    {
        // Arrange & Act
        var attribute = new SensitiveDataAttribute("REDACTED");

        // Assert
        attribute.MaskValue.Should().Be("REDACTED");
    }
}
