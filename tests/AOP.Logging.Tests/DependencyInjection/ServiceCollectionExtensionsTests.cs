using AOP.Logging.Core.Configuration;
using AOP.Logging.Core.Interfaces;
using AOP.Logging.Core.Logging;
using AOP.Logging.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace AOP.Logging.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAopLogging_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddAopLogging();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var methodLogger = serviceProvider.GetService<IMethodLogger>();
        methodLogger.Should().NotBeNull();
        methodLogger.Should().BeOfType<DefaultMethodLogger>();
    }

    [Fact]
    public void AddAopLogging_WithConfiguration_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddAopLogging(options =>
        {
            options.DefaultLogLevel = LogLevel.Debug;
            options.LogExecutionTime = false;
            options.MaxStringLength = 500;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<AopLoggingOptions>>();
        options.Should().NotBeNull();
        options!.Value.DefaultLogLevel.Should().Be(LogLevel.Debug);
        options.Value.LogExecutionTime.Should().BeFalse();
        options.Value.MaxStringLength.Should().Be(500);
    }

    [Fact]
    public void AddAopLogging_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        var action = () => services.AddAopLogging();

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("services");
    }

    [Fact]
    public void AddTransientWithLogging_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAopLogging();

        // Act
        services.AddTransientWithLogging<ITestService, TestService>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<ITestService>();
        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    [Fact]
    public void AddScopedWithLogging_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAopLogging();

        // Act
        services.AddScopedWithLogging<ITestService, TestService>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<ITestService>();
        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    [Fact]
    public void AddSingletonWithLogging_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAopLogging();

        // Act
        services.AddSingletonWithLogging<ITestService, TestService>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<ITestService>();
        service.Should().NotBeNull();
        service.Should().BeOfType<TestService>();
    }

    // Test service interface and implementation
    public interface ITestService
    {
        void DoWork();
    }

    public class TestService : ITestService
    {
        public void DoWork() { }

        public void SetMethodLogger(IMethodLogger methodLogger)
        {
            // Method for logger injection
        }
    }
}
