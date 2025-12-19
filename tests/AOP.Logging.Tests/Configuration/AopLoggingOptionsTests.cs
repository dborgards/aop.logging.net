using AOP.Logging.Core.Configuration;
using FluentAssertions;
using Xunit;

namespace AOP.Logging.Tests.Configuration;

public class AopLoggingOptionsTests
{
    [Fact]
    public void ShouldLogNamespace_WithNoConfiguration_ReturnsTrue()
    {
        // Arrange
        var options = new AopLoggingOptions();

        // Act
        var result = options.ShouldLogNamespace("MyApp.Services");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldLogNamespace_WithIncludedNamespace_ReturnsTrue()
    {
        // Arrange
        var options = new AopLoggingOptions
        {
            IncludedNamespaces = { "MyApp.Services" }
        };

        // Act
        var result = options.ShouldLogNamespace("MyApp.Services.Implementation");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldLogNamespace_WithExcludedNamespace_ReturnsFalse()
    {
        // Arrange
        var options = new AopLoggingOptions
        {
            ExcludedNamespaces = { "MyApp.Internal" }
        };

        // Act
        var result = options.ShouldLogNamespace("MyApp.Internal.Implementation");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldLogNamespace_ExcludedTakesPrecedenceOverIncluded()
    {
        // Arrange
        var options = new AopLoggingOptions
        {
            IncludedNamespaces = { "MyApp" },
            ExcludedNamespaces = { "MyApp.Internal" }
        };

        // Act
        var result = options.ShouldLogNamespace("MyApp.Internal.Service");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldLogClass_WithNoConfiguration_ReturnsTrue()
    {
        // Arrange
        var options = new AopLoggingOptions();

        // Act
        var result = options.ShouldLogClass("MyService");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldLogClass_WithWildcardInclude_ReturnsTrue()
    {
        // Arrange
        var options = new AopLoggingOptions
        {
            IncludedClasses = { "*Service" }
        };

        // Act
        var result = options.ShouldLogClass("UserService");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldLogClass_WithWildcardExclude_ReturnsFalse()
    {
        // Arrange
        var options = new AopLoggingOptions
        {
            ExcludedClasses = { "*Internal" }
        };

        // Act
        var result = options.ShouldLogClass("MyClassInternal");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldLogClass_ExcludedTakesPrecedenceOverIncluded()
    {
        // Arrange
        var options = new AopLoggingOptions
        {
            IncludedClasses = { "*Service" },
            ExcludedClasses = { "Internal*" }
        };

        // Act
        var result = options.ShouldLogClass("InternalService");

        // Assert
        result.Should().BeFalse();
    }
}
