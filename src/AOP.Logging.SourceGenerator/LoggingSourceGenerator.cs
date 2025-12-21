using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace AOP.Logging.SourceGenerator;

/// <summary>
/// Source generator that creates logging wrappers for classes and methods decorated with logging attributes.
/// </summary>
[Generator]
public class LoggingSourceGenerator : IIncrementalGenerator
{
    private const string LogClassAttribute = "AOP.Logging.Core.Attributes.LogClassAttribute";
    private const string LogMethodAttribute = "AOP.Logging.Core.Attributes.LogMethodAttribute";
    private const string DefaultSensitiveDataMask = "***SENSITIVE***";
    private const int CoreSuffixLength = 4; // Length of "Core" suffix

    /// <summary>
    /// Equality comparer for ClassDeclarationSyntax based on syntax tree and span.
    /// Used for deduplication when a class has both LogClass and LogMethod attributes.
    /// </summary>
    private sealed class ClassDeclarationSyntaxComparer : IEqualityComparer<ClassDeclarationSyntax>
    {
        public static readonly ClassDeclarationSyntaxComparer Instance = new ClassDeclarationSyntaxComparer();

        public bool Equals(ClassDeclarationSyntax? x, ClassDeclarationSyntax? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.SyntaxTree == y.SyntaxTree && x.Span == y.Span;
        }

        public int GetHashCode(ClassDeclarationSyntax obj)
        {
            unchecked
            {
                // 397 is a prime number commonly used in hash code implementations for good distribution
                // Note: HashCode.Combine would be preferable but requires .NET Standard 2.1+
                return (obj.SyntaxTree.GetHashCode() * 397) ^ obj.Span.GetHashCode();
            }
        }
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all classes with LogClass attribute using ForAttributeWithMetadataName for better performance
        var classesWithLogClass = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                LogClassAttribute,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.TargetNode);

        // Find all methods with LogMethod attribute and get their containing classes
        var classesWithLogMethod = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                LogMethodAttribute,
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, _) =>
                {
                    // Get the containing class of the method
                    var method = (MethodDeclarationSyntax)ctx.TargetNode;
                    return method.Ancestors()
                        .OfType<ClassDeclarationSyntax>()
                        .FirstOrDefault();
                })
            .Where(static c => c is not null)
            .Select(static (c, _) => c!); // Convert to non-nullable after null filtering

        // Combine both sources
        // Note: Collect() materializes each IncrementalValuesProvider into an ImmutableArray
        // Classes appearing in both sources (having both LogClass and LogMethod) are deduplicated
        // later in Execute via HashSet with ClassDeclarationSyntaxComparer
        var allClasses = classesWithLogClass
            .Collect()
            .Combine(classesWithLogMethod.Collect());

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider.Combine(allClasses);

        // Generate the logging code
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right.Left, source.Right.Right, spc));
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classesWithLogClass,
        ImmutableArray<ClassDeclarationSyntax> classesWithLogMethod,
        SourceProductionContext context)
    {
        // Combine both sources and deduplicate using HashSet for O(n) performance
        // HashSet with custom comparer ensures structural equality
        var allClasses = new HashSet<ClassDeclarationSyntax>(ClassDeclarationSyntaxComparer.Instance);
        allClasses.UnionWith(classesWithLogClass);
        allClasses.UnionWith(classesWithLogMethod);

        // Generate code for each unique class
        foreach (var classDeclaration in allClasses)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol is null)
            {
                continue;
            }

            var source = GenerateLoggingCode(classSymbol, classDeclaration);
            context.AddSource($"{classSymbol.Name}_Logging.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateLoggingCode(INamedTypeSymbol classSymbol, ClassDeclarationSyntax classDeclaration)
    {
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var className = classSymbol.Name;

        // Get class-level logging configuration
        var classAttribute = classSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == LogClassAttribute);
        var hasLogClassAttribute = classAttribute != null;

        var classLogLevel = GetLogLevel(classAttribute, "LogLevel") ?? "Information";
        var classLogParameters = GetBoolProperty(classAttribute, "LogParameters", true);
        var classLogReturnValue = GetBoolProperty(classAttribute, "LogReturnValue", true);
        var classLogExecutionTime = GetBoolProperty(classAttribute, "LogExecutionTime", true);
        var classLogExceptions = GetBoolProperty(classAttribute, "LogExceptions", true);

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Diagnostics;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine("using AOP.Logging.Core.Interfaces;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName}");
        sb.AppendLine("{");
        sb.AppendLine($"    partial class {className}");
        sb.AppendLine("    {");

        // Add the method logger field
        sb.AppendLine("        private IMethodLogger? __methodLogger;");
        sb.AppendLine();

        // Add SetMethodLogger method
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Sets the method logger for AOP logging.");
        sb.AppendLine("        /// This is typically called by the DI container.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        public void SetMethodLogger(IMethodLogger methodLogger)");
        sb.AppendLine("        {");
        sb.AppendLine("            __methodLogger = methodLogger;");
        sb.AppendLine("        }");
        sb.AppendLine();

        // Generate wrapper methods for all eligible methods
        // Filter out properties, events, constructors, operators, and compiler-generated methods
        var methods = classSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary &&
                       !m.IsStatic &&
                       !m.IsImplicitlyDeclared); // Exclude compiler-generated methods

        // Collect all existing method names to detect collisions
        var existingMethodNames = new HashSet<string>(
            classSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Select(m => m.Name));

        foreach (var method in methods)
        {
            var methodAttribute = method.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == LogMethodAttribute);

            // Skip if method has [LogMethod(Skip = true)]
            if (methodAttribute != null && GetBoolProperty(methodAttribute, "Skip", false))
                continue;

            // Determine if method should be logged
            // Methods are logged if the class has [LogClass] OR the method has [LogMethod]
            var shouldLog = hasLogClassAttribute || methodAttribute != null;
            if (!shouldLog)
                continue;

            // Calculate wrapper name to check for collisions
            var originalMethodName = method.Name;
            var wrapperMethodName = originalMethodName.EndsWith("Core") && originalMethodName.Length > CoreSuffixLength
                ? originalMethodName.Substring(0, originalMethodName.Length - CoreSuffixLength)
                : originalMethodName + "Logged";

            // Skip if wrapper name would collide with existing method (except the original method itself)
            if (existingMethodNames.Contains(wrapperMethodName) && wrapperMethodName != originalMethodName)
            {
                // Note: In a real implementation, we would report a diagnostic here
                // For now, we simply skip generating the wrapper
                continue;
            }

            // Get method-level configuration (overrides class-level)
            var logLevel = GetLogLevel(methodAttribute, "LogLevel") ?? classLogLevel;
            var logParameters = methodAttribute != null
                ? GetBoolProperty(methodAttribute, "LogParameters", classLogParameters)
                : classLogParameters;
            var logReturnValue = methodAttribute != null
                ? GetBoolProperty(methodAttribute, "LogReturnValue", classLogReturnValue)
                : classLogReturnValue;
            var logExecutionTime = methodAttribute != null
                ? GetBoolProperty(methodAttribute, "LogExecutionTime", classLogExecutionTime)
                : classLogExecutionTime;
            var logExceptions = methodAttribute != null
                ? GetBoolProperty(methodAttribute, "LogExceptions", classLogExceptions)
                : classLogExceptions;

            GenerateMethodWrapper(sb, method, className, logLevel, logParameters, logReturnValue,
                logExecutionTime, logExceptions);
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void GenerateMethodWrapper(
        StringBuilder sb,
        IMethodSymbol method,
        string className,
        string logLevel,
        bool logParameters,
        bool logReturnValue,
        bool logExecutionTime,
        bool logExceptions)
    {
        // Smart naming strategy:
        // - If method ends with "Core": remove "Core" suffix (backward compatibility)
        // - Otherwise: add "Logged" suffix
        var originalMethodName = method.Name;
        var wrapperMethodName = originalMethodName.EndsWith("Core") && originalMethodName.Length > CoreSuffixLength
            ? originalMethodName.Substring(0, originalMethodName.Length - CoreSuffixLength)
            : originalMethodName + "Logged";

        var returnType = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var isAsync = method.IsAsync || returnType.Contains("System.Threading.Tasks.Task");
        var isVoid = method.ReturnsVoid;
        var hasReturnValue = !isVoid && returnType != "global::System.Threading.Tasks.Task";

        // Build parameter list
        var parameters = method.Parameters;
        var paramList = string.Join(", ", parameters.Select(p =>
            $"{p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {p.Name}"));
        var paramNames = string.Join(", ", parameters.Select(p => p.Name));

        // Generate method signature
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// Logging wrapper for {wrapperMethodName}.");
        sb.AppendLine($"        /// </summary>");

        var asyncModifier = isAsync ? "async " : "";
        sb.AppendLine($"        public {asyncModifier}{returnType} {wrapperMethodName}({paramList})");
        sb.AppendLine("        {");

        // Log entry
        if (logParameters && parameters.Length > 0)
        {
            sb.AppendLine("            if (__methodLogger != null)");
            sb.AppendLine("            {");
            sb.AppendLine("                var __parameters = new Dictionary<string, object?>");
            sb.AppendLine("                {");

            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var isLast = i == parameters.Length - 1;

                // Check for SensitiveData attribute (single enumeration)
                var sensitiveAttr = param.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.Name == "SensitiveDataAttribute");
                var isSensitive = sensitiveAttr != null;

                var comma = isLast ? "" : ",";

                if (isSensitive)
                {
                    var maskValue = GetSensitiveDataMaskValue(sensitiveAttr);
                    sb.AppendLine($"                    {{ \"{param.Name}\", \"{maskValue}\" }}{comma}");
                }
                else
                {
                    sb.AppendLine($"                    {{ \"{param.Name}\", {param.Name} }}{comma}");
                }
            }

            sb.AppendLine("                };");
            sb.AppendLine($"                __methodLogger.LogEntry(\"{className}\", \"{wrapperMethodName}\", __parameters, LogLevel.{logLevel});");
            sb.AppendLine("            }");
            sb.AppendLine();
        }
        else if (logParameters)
        {
            // No parameters, but we still want to log method entry
            sb.AppendLine("            if (__methodLogger != null)");
            sb.AppendLine("            {");
            sb.AppendLine("                var __parameters = new Dictionary<string, object?>();");
            sb.AppendLine($"                __methodLogger.LogEntry(\"{className}\", \"{wrapperMethodName}\", __parameters, LogLevel.{logLevel});");
            sb.AppendLine("            }");
            sb.AppendLine();
        }

        // Start stopwatch
        if (logExecutionTime)
        {
            sb.AppendLine("            var __stopwatch = Stopwatch.StartNew();");
        }

        // Try-catch block for exception logging
        if (logExceptions)
        {
            sb.AppendLine("            try");
            sb.AppendLine("            {");
        }

        // Call the original method
        var awaitKeyword = isAsync ? "await " : "";
        var methodCall = $"{awaitKeyword}{originalMethodName}({paramNames})";

        if (isVoid)
        {
            sb.AppendLine($"                {methodCall};");
        }
        else
        {
            sb.AppendLine($"                var __result = {methodCall};");
        }

        // Stop stopwatch and log exit
        if (logExecutionTime)
        {
            sb.AppendLine("                __stopwatch.Stop();");
        }

        if (logReturnValue || logExecutionTime)
        {
            sb.AppendLine("                if (__methodLogger != null)");
            sb.AppendLine("                {");
            var executionTime = logExecutionTime ? "__stopwatch.ElapsedMilliseconds" : "0";

            // Check if return value has SensitiveData attribute
            var returnValueSensitiveAttr = method.GetReturnTypeAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "SensitiveDataAttribute");
            var isReturnValueSensitive = returnValueSensitiveAttr != null;

            string returnValueExpr;
            if (!hasReturnValue)
            {
                returnValueExpr = "null";
            }
            else if (isReturnValueSensitive)
            {
                var maskValue = GetSensitiveDataMaskValue(returnValueSensitiveAttr);
                returnValueExpr = $"\"{maskValue}\"";
            }
            else
            {
                returnValueExpr = "__result";
            }

            sb.AppendLine($"                    __methodLogger.LogExit(\"{className}\", \"{wrapperMethodName}\", {returnValueExpr}, {executionTime}, LogLevel.{logLevel});");
            sb.AppendLine("                }");
        }

        if (!isVoid)
        {
            sb.AppendLine("                return __result;");
        }

        // Exception handling
        if (logExceptions)
        {
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception __ex)");
            sb.AppendLine("            {");

            if (logExecutionTime)
            {
                sb.AppendLine("                __stopwatch.Stop();");
            }

            sb.AppendLine("                if (__methodLogger != null)");
            sb.AppendLine("                {");
            var executionTime = logExecutionTime ? "__stopwatch.ElapsedMilliseconds" : "0";
            sb.AppendLine($"                    __methodLogger.LogException(\"{className}\", \"{wrapperMethodName}\", __ex, {executionTime}, LogLevel.Error);");
            sb.AppendLine("                }");
            sb.AppendLine("                throw;");
            sb.AppendLine("            }");
        }

        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static string? GetLogLevel(AttributeData? attribute, string propertyName)
    {
        if (attribute == null) return null;

        // Check constructor argument first
        if (attribute.ConstructorArguments.Length > 0)
        {
            var arg = attribute.ConstructorArguments[0];
            if (arg.Type?.Name == "LogLevel" && arg.Value != null)
            {
                // Convert the enum value (int) to the enum name (string)
                var enumValue = (int)arg.Value;
                return GetLogLevelName(enumValue);
            }
        }

        // Check named argument
        var namedArg = attribute.NamedArguments
            .FirstOrDefault(a => a.Key == propertyName);
        if (namedArg.Value.Value != null && namedArg.Value.Type?.Name == "LogLevel")
        {
            var enumValue = (int)namedArg.Value.Value;
            return GetLogLevelName(enumValue);
        }

        return null;
    }

    private static string GetLogLevelName(int logLevelValue)
    {
        // Map LogLevel enum values to their names
        // LogLevel: Trace=0, Debug=1, Information=2, Warning=3, Error=4, Critical=5, None=6
        return logLevelValue switch
        {
            0 => "Trace",
            1 => "Debug",
            2 => "Information",
            3 => "Warning",
            4 => "Error",
            5 => "Critical",
            6 => "None",
            _ => "Information" // Use 'Information' as a safe, neutral default for any unrecognized LogLevel value
        };
    }

    private static bool GetBoolProperty(AttributeData? attribute, string propertyName, bool defaultValue)
    {
        if (attribute == null) return defaultValue;

        var namedArg = attribute.NamedArguments
            .FirstOrDefault(a => a.Key == propertyName);

        if (namedArg.Value.Value is bool boolValue)
        {
            return boolValue;
        }

        return defaultValue;
    }

    private static string GetSensitiveDataMaskValue(AttributeData? attribute)
    {
        if (attribute == null) return DefaultSensitiveDataMask;

        // Check constructor argument (for SensitiveDataAttribute, the first arg is the mask value)
        if (attribute.ConstructorArguments.Length > 0)
        {
            var arg = attribute.ConstructorArguments[0];
            if (arg.Value is string strValue)
            {
                return strValue;
            }
        }

        // Check named argument
        var namedArg = attribute.NamedArguments
            .FirstOrDefault(a => a.Key == "MaskValue");

        return namedArg.Value.Value as string ?? DefaultSensitiveDataMask;
    }
}
