using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using FluentAssertions;
using System.Linq;
using System.Collections.Immutable;

namespace AOP.Logging.Tests.SourceGenerator;

/// <summary>
/// Tests for the LoggingSourceGenerator partial class validation.
/// </summary>
public class LoggingSourceGeneratorTests
{
    /// <summary>
    /// Test that a non-partial class with [LogClass] generates AOPLOG001 diagnostic.
    /// </summary>
    [Fact]
    public void ClassWithLogClassAttribute_NotPartial_ReportsDiagnostic()
    {
        var source = @"
using AOP.Logging.Core.Attributes;

namespace TestNamespace
{
    [LogClass]
    public class MyService
    {
        public void DoSomethingCore()
        {
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should have exactly one AOPLOG001 diagnostic
        diagnostics.Should().ContainSingle(d => d.Id == "AOPLOG001");

        var diagnostic = diagnostics.First(d => d.Id == "AOPLOG001");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.GetMessage().Should().Contain("MyService");
        diagnostic.GetMessage().Should().Contain("partial");

        // Should not generate any code for this class
        generatedSources.Should().NotContain(s => s.HintName == "MyService_Logging.g.cs");
    }

    /// <summary>
    /// Test that a non-partial class with [LogMethod] generates AOPLOG001 diagnostic.
    /// </summary>
    [Fact]
    public void ClassWithLogMethodAttribute_NotPartial_ReportsDiagnostic()
    {
        var source = @"
using AOP.Logging.Core.Attributes;

namespace TestNamespace
{
    public class MyService
    {
        [LogMethod]
        public void ProcessDataCore()
        {
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should have exactly one AOPLOG001 diagnostic
        diagnostics.Should().ContainSingle(d => d.Id == "AOPLOG001");

        var diagnostic = diagnostics.First(d => d.Id == "AOPLOG001");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.GetMessage().Should().Contain("MyService");

        // Should not generate any code for this class
        generatedSources.Should().NotContain(s => s.HintName == "MyService_Logging.g.cs");
    }

    /// <summary>
    /// Test that a partial class with [LogClass] does not generate diagnostic and generates code.
    /// </summary>
    [Fact]
    public void PartialClassWithLogClassAttribute_GeneratesCodeWithoutDiagnostic()
    {
        var source = @"
using AOP.Logging.Core.Attributes;

namespace TestNamespace
{
    [LogClass]
    public partial class MyService
    {
        public void DoSomethingCore()
        {
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should not have any AOPLOG001 diagnostic
        diagnostics.Should().NotContain(d => d.Id == "AOPLOG001");

        // Should generate code for this class
        generatedSources.Should().ContainSingle(s => s.HintName == "MyService_Logging.g.cs");

        var generatedSource = generatedSources.First(s => s.HintName == "MyService_Logging.g.cs");
        generatedSource.SourceText.ToString().Should().Contain("partial class MyService");
        generatedSource.SourceText.ToString().Should().Contain("SetMethodLogger");
    }

    /// <summary>
    /// Test that a partial class with [LogMethod] does not generate diagnostic and generates code.
    /// </summary>
    [Fact]
    public void PartialClassWithLogMethodAttribute_GeneratesCodeWithoutDiagnostic()
    {
        var source = @"
using AOP.Logging.Core.Attributes;

namespace TestNamespace
{
    public partial class MyService
    {
        [LogMethod]
        public void ProcessDataCore()
        {
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should not have any AOPLOG001 diagnostic
        diagnostics.Should().NotContain(d => d.Id == "AOPLOG001");

        // Should generate code for this class
        generatedSources.Should().ContainSingle(s => s.HintName == "MyService_Logging.g.cs");
    }

    /// <summary>
    /// Test that multiple non-partial classes generate multiple diagnostics.
    /// </summary>
    [Fact]
    public void MultipleNonPartialClasses_ReportMultipleDiagnostics()
    {
        var source = @"
using AOP.Logging.Core.Attributes;

namespace TestNamespace
{
    [LogClass]
    public class Service1
    {
        public void Method1Core()
        {
        }
    }

    [LogClass]
    public class Service2
    {
        public void Method2Core()
        {
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should have exactly two AOPLOG001 diagnostics
        var aopDiagnostics = diagnostics.Where(d => d.Id == "AOPLOG001").ToList();
        aopDiagnostics.Should().HaveCount(2);

        aopDiagnostics.Should().Contain(d => d.GetMessage().Contains("Service1"));
        aopDiagnostics.Should().Contain(d => d.GetMessage().Contains("Service2"));

        // Should not generate any code for these classes
        generatedSources.Should().NotContain(s => s.HintName == "Service1_Logging.g.cs");
        generatedSources.Should().NotContain(s => s.HintName == "Service2_Logging.g.cs");
    }

    /// <summary>
    /// Test that a class without logging attributes and not partial does not generate diagnostic.
    /// </summary>
    [Fact]
    public void ClassWithoutLoggingAttributes_NotPartial_NoDiagnostic()
    {
        var source = @"
namespace TestNamespace
{
    public class MyService
    {
        public void DoSomething()
        {
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should not have any AOPLOG001 diagnostic
        diagnostics.Should().NotContain(d => d.Id == "AOPLOG001");

        // Should not generate any code
        generatedSources.Should().BeEmpty();
    }

    /// <summary>
    /// Test that a non-partial class with both [LogClass] and [LogMethod] generates only one diagnostic.
    /// </summary>
    [Fact]
    public void ClassWithBothAttributes_NotPartial_ReportsSingleDiagnostic()
    {
        var source = @"
using AOP.Logging.Core.Attributes;

namespace TestNamespace
{
    [LogClass]
    public class MyService
    {
        [LogMethod]
        public void ProcessDataCore()
        {
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should have exactly one AOPLOG001 diagnostic (deduplication works)
        diagnostics.Where(d => d.Id == "AOPLOG001").Should().ContainSingle();

        var diagnostic = diagnostics.First(d => d.Id == "AOPLOG001");
        diagnostic.GetMessage().Should().Contain("MyService");

        // Should not generate any code for this class
        generatedSources.Should().NotContain(s => s.HintName == "MyService_Logging.g.cs");
    }

    /// <summary>
    /// Test that mixed partial and non-partial classes generate appropriate diagnostics and code.
    /// </summary>
    [Fact]
    public void MixedPartialAndNonPartialClasses_GeneratesAppropriateOutput()
    {
        var source = @"
using AOP.Logging.Core.Attributes;

namespace TestNamespace
{
    [LogClass]
    public partial class GoodService
    {
        public void DoGoodThingCore()
        {
        }
    }

    [LogClass]
    public class BadService
    {
        public void DoBadThingCore()
        {
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should have exactly one AOPLOG001 diagnostic for BadService
        var aopDiagnostics = diagnostics.Where(d => d.Id == "AOPLOG001").ToList();
        aopDiagnostics.Should().ContainSingle();
        aopDiagnostics.First().GetMessage().Should().Contain("BadService");

        // Should generate code only for GoodService
        generatedSources.Should().ContainSingle(s => s.HintName == "GoodService_Logging.g.cs");
        generatedSources.Should().NotContain(s => s.HintName == "BadService_Logging.g.cs");
    }

    /// <summary>
    /// Test that SensitiveData with simple mask value generates correct escaped literal.
    /// </summary>
    [Fact]
    public void SensitiveDataWithSimpleMaskValue_GeneratesCorrectEscapedLiteral()
    {
        var source = @"
using AOP.Logging.Core.Attributes;

namespace TestNamespace
{
    [LogClass]
    public partial class MyService
    {
        public void ProcessDataCore([SensitiveData(""***"")] string password)
        {
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should not have any errors
        diagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);

        // Should generate code
        generatedSources.Should().ContainSingle(s => s.HintName == "MyService_Logging.g.cs");

        var generatedSource = generatedSources.First(s => s.HintName == "MyService_Logging.g.cs");
        var generatedCode = generatedSource.SourceText.ToString();

        // Should contain properly escaped mask value
        generatedCode.Should().Contain("\"***\"");
        generatedCode.Should().Contain("{ \"password\", \"***\" }");
    }

    /// <summary>
    /// Test that SensitiveData with quotes in mask value generates correct escaped literal.
    /// </summary>
    [Fact]
    public void SensitiveDataWithQuotesInMaskValue_GeneratesCorrectEscapedLiteral()
    {
        var source = @"
using AOP.Logging.Core.Attributes;

namespace TestNamespace
{
    [LogClass]
    public partial class MyService
    {
        public void ProcessDataCore([SensitiveData(""foo\""bar"")] string data)
        {
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should not have any errors
        diagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);

        // Should generate code
        generatedSources.Should().ContainSingle(s => s.HintName == "MyService_Logging.g.cs");

        var generatedSource = generatedSources.First(s => s.HintName == "MyService_Logging.g.cs");
        var generatedCode = generatedSource.SourceText.ToString();

        // Should contain properly escaped quotes
        generatedCode.Should().Contain("\"foo\\\"bar\"");

        // Verify the generated code compiles
        VerifyGeneratedCodeCompiles(generatedCode, "MyService");
    }

    /// <summary>
    /// Test that SensitiveData with newline in mask value generates correct escaped literal.
    /// </summary>
    [Fact]
    public void SensitiveDataWithNewlineInMaskValue_GeneratesCorrectEscapedLiteral()
    {
        var source = @"
using AOP.Logging.Core.Attributes;

namespace TestNamespace
{
    [LogClass]
    public partial class MyService
    {
        public void ProcessDataCore([SensitiveData(""line1\nline2"")] string data)
        {
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should not have any errors
        diagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);

        // Should generate code
        generatedSources.Should().ContainSingle(s => s.HintName == "MyService_Logging.g.cs");

        var generatedSource = generatedSources.First(s => s.HintName == "MyService_Logging.g.cs");
        var generatedCode = generatedSource.SourceText.ToString();

        // Should contain properly escaped newline
        generatedCode.Should().Contain("\"line1\\nline2\"");

        // Verify the generated code compiles
        VerifyGeneratedCodeCompiles(generatedCode, "MyService");
    }

    /// <summary>
    /// Test that SensitiveData with backslash in mask value generates correct escaped literal.
    /// </summary>
    [Fact]
    public void SensitiveDataWithBackslashInMaskValue_GeneratesCorrectEscapedLiteral()
    {
        var source = @"
using AOP.Logging.Core.Attributes;

namespace TestNamespace
{
    [LogClass]
    public partial class MyService
    {
        public void ProcessDataCore([SensitiveData(""path\\to\\file"")] string path)
        {
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should not have any errors
        diagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);

        // Should generate code
        generatedSources.Should().ContainSingle(s => s.HintName == "MyService_Logging.g.cs");

        var generatedSource = generatedSources.First(s => s.HintName == "MyService_Logging.g.cs");
        var generatedCode = generatedSource.SourceText.ToString();

        // Should contain properly escaped backslashes
        generatedCode.Should().Contain("\"path\\\\to\\\\file\"");

        // Verify the generated code compiles
        VerifyGeneratedCodeCompiles(generatedCode, "MyService");
    }

    /// <summary>
    /// Test that a custom SensitiveDataAttribute in a different namespace is NOT recognized.
    /// This ensures we use fully qualified name matching.
    /// </summary>
    [Fact]
    public void CustomSensitiveDataAttributeInDifferentNamespace_IsNotRecognized()
    {
        var source = @"
using System;
using AOP.Logging.Core.Attributes;

namespace MyCustomNamespace
{
    // Custom attribute with same name but different namespace
    [AttributeUsage(AttributeTargets.Parameter)]
    public class SensitiveDataAttribute : Attribute
    {
        public SensitiveDataAttribute(string maskValue) { }
    }
}

namespace TestNamespace
{
    using MyCustomNamespace;

    [LogClass]
    public partial class MyService
    {
        // This uses the custom attribute, NOT AOP.Logging.Core.Attributes.SensitiveDataAttribute
        public void ProcessDataCore([SensitiveData(""CUSTOM_MASK"")] string data)
        {
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should not have any errors
        diagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);

        // Should generate code
        generatedSources.Should().ContainSingle(s => s.HintName == "MyService_Logging.g.cs");

        var generatedSource = generatedSources.First(s => s.HintName == "MyService_Logging.g.cs");
        var generatedCode = generatedSource.SourceText.ToString();

        // Should NOT mask the parameter (since it's not our SensitiveDataAttribute)
        // The parameter should be logged normally, not with CUSTOM_MASK
        generatedCode.Should().Contain("{ \"data\", data }");
        generatedCode.Should().NotContain("CUSTOM_MASK");
    }

    /// <summary>
    /// Test that the correct SensitiveDataAttribute from AOP.Logging.Core.Attributes IS recognized.
    /// </summary>
    [Fact]
    public void CorrectSensitiveDataAttributeFromAOPLoggingNamespace_IsRecognized()
    {
        var source = @"
using AOP.Logging.Core.Attributes;

namespace TestNamespace
{
    [LogClass]
    public partial class MyService
    {
        public void ProcessDataCore([SensitiveData(""CORRECT_MASK"")] string password)
        {
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should not have any errors
        diagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);

        // Should generate code
        generatedSources.Should().ContainSingle(s => s.HintName == "MyService_Logging.g.cs");

        var generatedSource = generatedSources.First(s => s.HintName == "MyService_Logging.g.cs");
        var generatedCode = generatedSource.SourceText.ToString();

        // Should mask the parameter with our mask value
        generatedCode.Should().Contain("{ \"password\", \"CORRECT_MASK\" }");
    }

    /// <summary>
    /// Test that SensitiveData on return value generates correct escaped literal.
    /// </summary>
    [Fact]
    public void SensitiveDataOnReturnValue_GeneratesCorrectEscapedLiteral()
    {
        var source = @"
using AOP.Logging.Core.Attributes;

namespace TestNamespace
{
    [LogClass]
    public partial class MyService
    {
        [return: SensitiveData(""<redacted>\""value\"""")]
        public string GetSecretCore()
        {
            return ""secret"";
        }
    }
}";

        var (diagnostics, generatedSources) = RunGenerator(source);

        // Should not have any errors
        diagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);

        // Should generate code
        generatedSources.Should().ContainSingle(s => s.HintName == "MyService_Logging.g.cs");

        var generatedSource = generatedSources.First(s => s.HintName == "MyService_Logging.g.cs");
        var generatedCode = generatedSource.SourceText.ToString();

        // Should contain properly escaped return value mask
        generatedCode.Should().Contain("\"<redacted>\\\"value\\\"\"");

        // Verify the generated code compiles
        VerifyGeneratedCodeCompiles(generatedCode, "MyService");
    }

    private static (ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<GeneratedSourceResult> GeneratedSources) RunGenerator(string source)
    {
        // Create the compilation with the source code
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Add only the minimal required references for compilation
        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Core.Attributes.LogClassAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.Logging.ILogger).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Core.Interfaces.IMethodLogger).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.IsExternalInit).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Create and run the generator
        var generator = new AOP.Logging.SourceGenerator.LoggingSourceGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult();

        return (diagnostics, runResult.GeneratedTrees.Length > 0
            ? runResult.Results[0].GeneratedSources
            : ImmutableArray<GeneratedSourceResult>.Empty);
    }

    /// <summary>
    /// Verifies that the generated code compiles without errors.
    /// </summary>
    private static void VerifyGeneratedCodeCompiles(string generatedCode, string className)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);

        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Core.Attributes.LogClassAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.Logging.ILogger).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Core.Interfaces.IMethodLogger).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.IsExternalInit).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Diagnostics.Stopwatch).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: $"{className}GeneratedTest",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

        errors.Should().BeEmpty($"Generated code for {className} should compile without errors. Errors: {string.Join(", ", errors.Select(e => e.GetMessage()))}");
    }
}
