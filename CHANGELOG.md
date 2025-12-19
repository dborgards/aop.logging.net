# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial release of AOP.Logging.NET framework
- Core library with attribute-based logging (`LogClass`, `LogMethod`, `LogParameter`, `LogResult`, `LogException`, `SensitiveData`)
- Source Generator for compile-time method interception
- Integration with Microsoft.Extensions.Logging
- Dependency Injection extensions for IServiceCollection
- Support for async/await methods
- Structured logging with contextual information
- Configurable logging options (namespaces, classes, log levels)
- Sensitive data protection
- Execution time tracking
- Parameter and return value logging
- Exception logging with detailed information
- Sample project demonstrating all features
- Comprehensive unit tests with xUnit
- Documentation (README, CONTRIBUTING)

## [1.0.0] - TBD

### Added
- Initial public release

[Unreleased]: https://github.com/dborgards/aop.logging.net/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/dborgards/aop.logging.net/releases/tag/v1.0.0
