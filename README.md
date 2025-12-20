# AOP.Logging.NET

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%2010.0-512BD4)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/AOP.Logging.Core.svg)](https://www.nuget.org/packages/AOP.Logging.Core/)
[![Semantic Release](https://github.com/dborgards/aop.logging.net/actions/workflows/semantic-release.yml/badge.svg)](https://github.com/dborgards/aop.logging.net/actions/workflows/semantic-release.yml)

A powerful, attribute-based Aspect-Oriented Programming (AOP) logging framework for C# that provides seamless method interception and automatic logging using Source Generators.

## Features

- **Attribute-Based Logging**: Simple, declarative logging with `[LogClass]`, `[LogMethod]`, and related attributes
- **Compile-Time Source Generation**: Zero runtime overhead with C# Source Generators
- **Microsoft.Extensions.Logging Integration**: Works with your existing logging infrastructure
- **Async/Await Support**: Full support for async methods and Task-based operations
- **Sensitive Data Protection**: Automatically mask sensitive data with `[SensitiveData]` attribute
- **Structured Logging**: Rich, contextual logging with proper parameter serialization
- **Configurable**: Fine-grained control over what gets logged and how
- **Dependency Injection**: First-class support for Microsoft.Extensions.DependencyInjection
- **Performance Optimized**: Minimal overhead with intelligent logging decisions

## Installation

Install the NuGet packages:

```bash
# Core library with attributes
dotnet add package AOP.Logging.Core

# Source Generator (required for compile-time code generation)
dotnet add package AOP.Logging.SourceGenerator

# Dependency Injection extensions
dotnet add package AOP.Logging.DependencyInjection
```

## Quick Start

### 1. Configure Services

```csharp
using AOP.Logging.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Add AOP logging
        services.AddAopLogging(options =>
        {
            options.DefaultLogLevel = LogLevel.Information;
            options.LogExecutionTime = true;
            options.LogParameters = true;
            options.LogReturnValues = true;
        });

        // Register your services with logging
        services.AddTransientWithLogging<IMyService, MyService>();
    })
    .Build();

await host.RunAsync();
```

### 2. Add Logging to Your Classes

```csharp
using AOP.Logging.Core.Attributes;

[LogClass]
public partial class MyService : IMyService
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    [LogMethod(LogLevel.Debug)]
    public async Task<string> GetDataAsync(int id)
    {
        await Task.Delay(100);
        return $"Data for {id}";
    }
}
```

**Important**: Classes using AOP logging must be declared as `partial`.

### 3. Run and See the Logs

```
info: MyService[0]
      Entering MyService.Add
info: MyService[0]
      Exiting MyService.Add (took 0ms)
```

## Usage Examples

### Basic Method Logging

```csharp
[LogClass]
public partial class CalculatorService
{
    public int Add(int a, int b) => a + b;

    public int Multiply(int a, int b) => a * b;
}
```

### Custom Log Levels

```csharp
[LogClass(LogLevel.Debug)]
public partial class DebugService
{
    [LogMethod(LogLevel.Warning)]
    public void PerformCriticalOperation()
    {
        // This method will log at Warning level
    }
}
```

### Sensitive Data Protection

```csharp
[LogClass]
public partial class UserService
{
    public async Task<User> CreateUserAsync(
        string email,
        [SensitiveData] string password)
    {
        // password will appear as "***SENSITIVE***" in logs
        var user = new User { Email = email };
        return user;
    }
}
```

### Exception Logging

```csharp
[LogClass]
public partial class DataService
{
    [LogException(LogLevel.Error)]
    public void ProcessData(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            throw new ArgumentException("Data cannot be null");
        }
        // Exception will be automatically logged
    }
}
```

### Async Method Support

```csharp
[LogClass]
public partial class ApiService
{
    public async Task<ApiResponse> FetchDataAsync(string endpoint)
    {
        await Task.Delay(100);
        return new ApiResponse { Data = "Success" };
    }

    public async Task<T> GetAsync<T>(string url)
    {
        // Generic async methods are fully supported
        await Task.Delay(50);
        return default(T)!;
    }
}
```

### Parameter Control

```csharp
[LogClass]
public partial class ReportService
{
    public Report Generate(
        [LogParameter(Name = "ReportId")] int id,
        [LogParameter(Skip = true)] string internalToken,
        [LogParameter(MaxLength = 50)] string description)
    {
        // internalToken won't be logged
        // description will be truncated to 50 characters
        return new Report { Id = id };
    }
}
```

### Return Value Logging

```csharp
[LogClass]
public partial class CalculationService
{
    [LogResult(Name = "CalculationResult")]
    public double Calculate(double x, double y)
    {
        return Math.Sqrt(x * x + y * y);
    }

    [LogResult(Skip = true)]
    public byte[] GetBinaryData()
    {
        // Return value won't be logged (useful for large data)
        return new byte[1024];
    }
}
```

### Selective Method Logging

```csharp
[LogClass]
public partial class MixedService
{
    public void LoggedMethod()
    {
        // This will be logged
    }

    [LogMethod(Skip = true)]
    public void NotLoggedMethod()
    {
        // This will NOT be logged
    }
}
```

## Configuration

### Global Options

```csharp
services.AddAopLogging(options =>
{
    // Default log level
    options.DefaultLogLevel = LogLevel.Information;

    // Execution time tracking
    options.LogExecutionTime = true;

    // Parameter and return value logging
    options.LogParameters = true;
    options.LogReturnValues = true;

    // Exception logging
    options.LogExceptions = true;

    // String and collection limits
    options.MaxStringLength = 1000;
    options.MaxCollectionSize = 10;

    // Structured logging
    options.UseStructuredLogging = true;

    // Namespace filtering
    options.IncludedNamespaces.Add("MyApp.Services");
    options.ExcludedNamespaces.Add("MyApp.Internal");

    // Class filtering with wildcards
    options.IncludedClasses.Add("*Service");
    options.ExcludedClasses.Add("*Internal");

    // Custom message formats
    options.EntryMessageFormat = "→ {ClassName}.{MethodName}";
    options.ExitMessageFormat = "← {ClassName}.{MethodName} ({ExecutionTime}ms)";
    options.ExceptionMessageFormat = "✗ {ClassName}.{MethodName}: {ExceptionMessage}";
});
```

### Message Format Placeholders

**Entry Messages**:
- `{ClassName}` - The name of the class
- `{MethodName}` - The name of the method
- `{Parameters}` - Formatted parameter list

**Exit Messages**:
- `{ClassName}` - The name of the class
- `{MethodName}` - The name of the method
- `{ReturnValue}` - The return value
- `{ExecutionTime}` - Execution time in milliseconds

**Exception Messages**:
- `{ClassName}` - The name of the class
- `{MethodName}` - The name of the method
- `{ExceptionType}` - The exception type name
- `{ExceptionMessage}` - The exception message
- `{ExecutionTime}` - Execution time before exception

## Attributes Reference

### `[LogClass]`

Marks a class for automatic logging of all public methods.

```csharp
[LogClass(LogLevel.Information)]
public partial class MyService { }
```

**Properties**:
- `LogLevel` - Log level for all methods (default: Information)
- `LogExecutionTime` - Track execution time (default: true)
- `LogParameters` - Log method parameters (default: true)
- `LogReturnValue` - Log return values (default: true)
- `LogExceptions` - Log exceptions (default: true)

### `[LogMethod]`

Controls logging for a specific method, overriding class-level settings.

```csharp
[LogMethod(LogLevel.Debug)]
public void MyMethod() { }
```

**Properties**:
- `LogLevel` - Log level for this method
- `LogExecutionTime` - Track execution time
- `LogParameters` - Log method parameters
- `LogReturnValue` - Log return value
- `LogExceptions` - Log exceptions
- `Skip` - Skip logging for this method
- `EntryMessage` - Custom entry message template
- `ExitMessage` - Custom exit message template

### `[LogParameter]`

Controls logging for a specific parameter.

```csharp
public void MyMethod([LogParameter(Name = "UserId")] int id) { }
```

**Properties**:
- `Skip` - Skip logging this parameter
- `Name` - Custom name in logs
- `MaxLength` - Maximum length for string values

### `[LogResult]`

Controls logging for method return values.

```csharp
[LogResult(Name = "Result")]
public int Calculate() => 42;
```

**Properties**:
- `Skip` - Skip logging the return value
- `Name` - Custom name in logs
- `MaxLength` - Maximum length for string values

### `[LogException]`

Controls exception logging behavior.

```csharp
[LogException(LogLevel.Error)]
public void RiskyOperation() { }
```

**Properties**:
- `LogLevel` - Log level for exceptions (default: Error)
- `IncludeDetails` - Include stack trace and inner exceptions (default: true)
- `Rethrow` - Rethrow the exception after logging (default: true)
- `Message` - Custom exception message template

### `[SensitiveData]`

Marks data as sensitive, preventing it from being logged.

```csharp
public void Login([SensitiveData] string password) { }
```

**Properties**:
- `MaskValue` - The mask to use (default: "***SENSITIVE***")
- `ShowLength` - Show the length of sensitive data (default: false)

## Dependency Injection Extensions

### Service Registration with Logging

```csharp
// Transient
services.AddTransientWithLogging<IMyService, MyService>();

// Scoped
services.AddScopedWithLogging<IMyService, MyService>();

// Singleton
services.AddSingletonWithLogging<IMyService, MyService>();
```

These extension methods automatically inject the `IMethodLogger` into your services.

## Best Practices

1. **Use `partial` classes**: Classes with logging attributes must be declared as `partial`
2. **Protect sensitive data**: Always use `[SensitiveData]` for passwords, tokens, and PII
3. **Choose appropriate log levels**: Use Debug for verbose logging, Information for normal flow, Warning for unusual situations
4. **Limit collection sizes**: Set `MaxCollectionSize` to prevent logging large collections
5. **Skip unnecessary logging**: Use `Skip = true` for methods that don't need logging
6. **Use structured logging**: Enable `UseStructuredLogging` for better log analysis

## Performance Considerations

- **Compile-time generation**: All logging code is generated at compile time, not runtime
- **Zero reflection**: No reflection is used during logging execution
- **Conditional logging**: Logs are only formatted when the log level is enabled
- **Minimal allocations**: Optimized for low memory allocation
- **Async-friendly**: Async methods are properly handled without blocking

## Requirements

- .NET 8.0 or .NET 10.0
- C# 11.0 or higher
- Microsoft.Extensions.Logging 8.0+

## Sample Project

Check out the [sample project](./samples/AOP.Logging.Sample) for complete working examples demonstrating all features.

To run the sample:

```bash
cd samples/AOP.Logging.Sample
dotnet run
```

## Versioning

This project follows [Semantic Versioning](https://semver.org/) and uses [Conventional Commits](https://www.conventionalcommits.org/) for automated version management.

### Release Process

- **Automatic Versioning**: Versions are calculated automatically based on commit messages
- **Conventional Commits**: All commits must follow the conventional commits specification
- **semantic-release**: Automated version calculation and release management

See [VERSIONING.md](docs/VERSIONING.md) for detailed information about our versioning strategy.

### Commit Message Format

```bash
# Features (bumps MINOR version)
feat: add custom interceptor support

# Bug fixes (bumps PATCH version)
fix: resolve null reference in logger

# Breaking changes (bumps MAJOR version)
breaking: redesign attribute API
```

## Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

**Important**: All commits must follow [Conventional Commits](https://www.conventionalcommits.org/) format for proper version management.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/dborgards/aop.logging.net/issues)
- **Discussions**: [GitHub Discussions](https://github.com/dborgards/aop.logging.net/discussions)

## Roadmap

- [ ] Custom interceptor support
- [ ] Performance counters integration
- [ ] OpenTelemetry integration
- [ ] Configuration from appsettings.json
- [ ] Advanced filtering expressions
- [ ] Log correlation support

---

Made with ❤️ by the AOP.Logging community
