# Contributing to AOP.Logging.NET

Thank you for your interest in contributing to AOP.Logging.NET! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Project Structure](#project-structure)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Pull Request Process](#pull-request-process)
- [Release Process](#release-process)

## Code of Conduct

This project adheres to a code of conduct. By participating, you are expected to:

- Use welcoming and inclusive language
- Be respectful of differing viewpoints and experiences
- Gracefully accept constructive criticism
- Focus on what is best for the community
- Show empathy towards other community members

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/aop.logging.net.git
   cd aop.logging.net
   ```
3. **Add the upstream repository**:
   ```bash
   git remote add upstream https://github.com/dborgards/aop.logging.net.git
   ```
4. **Create a feature branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```

## Development Setup

### Prerequisites

- .NET SDK 8.0 or later
- Visual Studio 2022 / VS Code / Rider (recommended)
- Git

### Building the Project

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the sample project
cd samples/AOP.Logging.Sample
dotnet run
```

### IDE Setup

#### Visual Studio 2022
- Open `AOP.Logging.sln`
- Enable "Analyze and Suggest Code Fixes" in Tools > Options > Text Editor > C# > Advanced
- Install recommended extensions:
  - CodeMaid
  - SonarLint

#### VS Code
- Install C# extension
- Install .NET Core Test Explorer
- Recommended settings in `.vscode/settings.json`:
  ```json
  {
    "editor.formatOnSave": true,
    "omnisharp.enableRoslynAnalyzers": true,
    "omnisharp.enableEditorConfigSupport": true
  }
  ```

## Project Structure

```
aop.logging.net/
├── src/
│   ├── AOP.Logging.Core/              # Core library with attributes and interfaces
│   ├── AOP.Logging.SourceGenerator/   # Roslyn source generator
│   └── AOP.Logging.DependencyInjection/ # DI extensions
├── samples/
│   └── AOP.Logging.Sample/            # Sample application
├── tests/
│   └── AOP.Logging.Tests/             # Unit and integration tests
├── docs/                              # Additional documentation
├── .editorconfig                      # Code style configuration
└── AOP.Logging.sln                    # Solution file
```

### Key Components

- **Core Library**: Contains attributes, interfaces, and configuration
- **Source Generator**: Roslyn-based incremental generator for compile-time code generation
- **DI Extensions**: Service collection extensions for dependency injection
- **Tests**: xUnit tests with FluentAssertions and NSubstitute

## Development Workflow

### 1. Pick or Create an Issue

- Check [existing issues](https://github.com/dborgards/aop.logging.net/issues)
- Comment on the issue you want to work on
- For new features, create an issue first to discuss

### 2. Create a Feature Branch

```bash
git checkout -b feature/issue-number-description
# or
git checkout -b bugfix/issue-number-description
```

### 3. Make Your Changes

- Write clean, readable code
- Follow the coding standards (see below)
- Add tests for new functionality
- Update documentation if needed

### 4. Test Your Changes

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test project
dotnet test tests/AOP.Logging.Tests/AOP.Logging.Tests.csproj
```

### 5. Commit Your Changes

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```bash
git commit -m "feat: add support for custom interceptors"
git commit -m "fix: resolve null reference in DefaultMethodLogger"
git commit -m "docs: update README with new examples"
git commit -m "test: add tests for AopLoggingOptions"
git commit -m "refactor: simplify parameter formatting logic"
```

**Commit Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `test`: Adding or updating tests
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `chore`: Maintenance tasks
- `ci`: CI/CD changes

### 6. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Then create a pull request on GitHub.

## Coding Standards

### General Guidelines

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use `.editorconfig` settings (enforced automatically)
- Maximum line length: 120 characters
- Use meaningful variable and method names
- Keep methods focused and small (Single Responsibility Principle)

### Code Style

```csharp
// ✅ Good
public async Task<User> GetUserAsync(int userId)
{
    if (userId <= 0)
    {
        throw new ArgumentException("User ID must be positive", nameof(userId));
    }

    var user = await _repository.GetByIdAsync(userId);
    return user ?? throw new NotFoundException($"User {userId} not found");
}

// ❌ Bad
public async Task<User> GetUserAsync(int userId){
    if(userId<=0) throw new ArgumentException("User ID must be positive");
    return await _repository.GetByIdAsync(userId);
}
```

### Naming Conventions

- **Classes**: PascalCase - `DefaultMethodLogger`
- **Interfaces**: PascalCase with 'I' prefix - `IMethodLogger`
- **Methods**: PascalCase - `LogEntry`
- **Properties**: PascalCase - `LogLevel`
- **Fields**: camelCase with underscore prefix - `_logger`
- **Parameters**: camelCase - `methodName`
- **Constants**: PascalCase - `DefaultMaxLength`

### XML Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Logs method entry with parameters.
/// </summary>
/// <param name="className">The name of the class containing the method.</param>
/// <param name="methodName">The name of the method.</param>
/// <param name="parameters">The method parameters as key-value pairs.</param>
/// <param name="logLevel">The log level to use.</param>
public void LogEntry(string className, string methodName,
    IDictionary<string, object?> parameters, LogLevel logLevel)
{
    // Implementation
}
```

### Async Guidelines

- Use `async`/`await` for I/O-bound operations
- Append `Async` suffix to async method names
- Avoid `async void` (except event handlers)
- Use `ConfigureAwait(false)` in library code

```csharp
// ✅ Good
public async Task<string> FetchDataAsync()
{
    var result = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
    return result;
}
```

### Null Safety

- Enable nullable reference types (`<Nullable>enable</Nullable>`)
- Use `!` operator sparingly and only when certain
- Validate parameters with guard clauses

```csharp
public DefaultMethodLogger(ILogger<DefaultMethodLogger> logger, IOptions<AopLoggingOptions> options)
{
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
}
```

## Testing

### Test Structure

- Use **Arrange-Act-Assert** pattern
- One assertion per test (when possible)
- Descriptive test names

```csharp
[Fact]
public void LogEntry_WithParameters_LogsCorrectly()
{
    // Arrange
    var parameters = new Dictionary<string, object?> { ["id"] = 1 };
    _mockLogger.IsEnabled(LogLevel.Information).Returns(true);

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
```

### Test Coverage

- Aim for >80% code coverage
- Focus on business logic and public APIs
- Don't test framework code

### Running Tests

```bash
# All tests
dotnet test

# Specific test
dotnet test --filter "FullyQualifiedName~AopLoggingOptionsTests"

# With coverage
dotnet test /p:CollectCoverage=true
```

## Pull Request Process

### Before Submitting

- [ ] All tests pass
- [ ] Code follows style guidelines
- [ ] XML documentation is added for public APIs
- [ ] README is updated (if needed)
- [ ] CHANGELOG is updated (for significant changes)
- [ ] Commit messages follow Conventional Commits

### PR Template

When creating a PR, include:

```markdown
## Description
Brief description of the changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
How was this tested?

## Checklist
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] No breaking changes (or documented)
```

### Review Process

1. Automated checks (CI/CD) must pass
2. At least one maintainer approval required
3. Address review comments
4. Squash commits if requested
5. Maintainer will merge when ready

## Release Process

Releases follow [Semantic Versioning](https://semver.org/):

- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes

### Creating a Release

1. Update version in `.csproj` files
2. Update CHANGELOG.md
3. Create a tag: `git tag v1.2.3`
4. Push tag: `git push origin v1.2.3`
5. CI/CD will build and publish to NuGet

## Questions?

- Open a [Discussion](https://github.com/dborgards/aop.logging.net/discussions)
- Join our community chat
- Email the maintainers

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to AOP.Logging.NET!
