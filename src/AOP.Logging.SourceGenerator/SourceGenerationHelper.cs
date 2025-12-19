namespace AOP.Logging.SourceGenerator;

/// <summary>
/// Helper class for source generation.
/// </summary>
internal static class SourceGenerationHelper
{
    /// <summary>
    /// Gets the source code for the logging attributes.
    /// This is embedded in the generated assembly to avoid requiring a reference to the core library.
    /// </summary>
    public const string AttributeSource = @"
#nullable enable

namespace AOP.Logging.Core.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal sealed class LogClassAttribute : System.Attribute
    {
        public Microsoft.Extensions.Logging.LogLevel LogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Information;
        public bool LogExecutionTime { get; set; } = true;
        public bool LogParameters { get; set; } = true;
        public bool LogReturnValue { get; set; } = true;
        public bool LogExceptions { get; set; } = true;

        public LogClassAttribute() { }
        public LogClassAttribute(Microsoft.Extensions.Logging.LogLevel logLevel) { LogLevel = logLevel; }
    }

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    internal sealed class LogMethodAttribute : System.Attribute
    {
        public Microsoft.Extensions.Logging.LogLevel LogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Information;
        public bool LogExecutionTime { get; set; } = true;
        public bool LogParameters { get; set; } = true;
        public bool LogReturnValue { get; set; } = true;
        public bool LogExceptions { get; set; } = true;
        public bool Skip { get; set; } = false;
        public string? EntryMessage { get; set; }
        public string? ExitMessage { get; set; }

        public LogMethodAttribute() { }
        public LogMethodAttribute(Microsoft.Extensions.Logging.LogLevel logLevel) { LogLevel = logLevel; }
    }

    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class LogParameterAttribute : System.Attribute
    {
        public bool Skip { get; set; } = false;
        public string? Name { get; set; }
        public int MaxLength { get; set; } = -1;

        public LogParameterAttribute() { }
        public LogParameterAttribute(string name) { Name = name; }
    }

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    internal sealed class LogResultAttribute : System.Attribute
    {
        public bool Skip { get; set; } = false;
        public string? Name { get; set; }
        public int MaxLength { get; set; } = -1;

        public LogResultAttribute() { }
        public LogResultAttribute(string name) { Name = name; }
    }

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    internal sealed class LogExceptionAttribute : System.Attribute
    {
        public Microsoft.Extensions.Logging.LogLevel LogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Error;
        public bool IncludeDetails { get; set; } = true;
        public bool Rethrow { get; set; } = true;
        public string? Message { get; set; }

        public LogExceptionAttribute() { }
        public LogExceptionAttribute(Microsoft.Extensions.Logging.LogLevel logLevel) { LogLevel = logLevel; }
    }

    [System.AttributeUsage(System.AttributeTargets.Parameter | System.AttributeTargets.Property | System.AttributeTargets.Method | System.AttributeTargets.ReturnValue,
        AllowMultiple = false, Inherited = false)]
    internal sealed class SensitiveDataAttribute : System.Attribute
    {
        public string MaskValue { get; set; } = ""***SENSITIVE***"";
        public bool ShowLength { get; set; } = false;

        public SensitiveDataAttribute() { }
        public SensitiveDataAttribute(string maskValue) { MaskValue = maskValue; }
    }
}
";
}
