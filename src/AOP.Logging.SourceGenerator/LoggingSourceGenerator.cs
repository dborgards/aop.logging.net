using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
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

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the attribute source
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "LoggingAttributes.g.cs",
            SourceText.From(SourceGenerationHelper.AttributeSource, Encoding.UTF8)));

        // Find all classes with LogClass attribute
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsCandidateClass(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        // Generate the logging code
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsCandidateClass(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDecl && classDecl.AttributeLists.Count > 0;
    }

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(attribute);
                if (symbolInfo.Symbol is not IMethodSymbol attributeSymbol)
                {
                    continue;
                }

                var attributeType = attributeSymbol.ContainingType.ToDisplayString();
                if (attributeType == LogClassAttribute)
                {
                    return classDeclaration;
                }
            }
        }

        // Check if any methods have LogMethod attribute
        foreach (var member in classDeclaration.Members)
        {
            if (member is MethodDeclarationSyntax methodDecl && methodDecl.AttributeLists.Count > 0)
            {
                foreach (var attributeList in methodDecl.AttributeLists)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        var symbolInfo = context.SemanticModel.GetSymbolInfo(attribute);
                        if (symbolInfo.Symbol is not IMethodSymbol attributeSymbol)
                        {
                            continue;
                        }

                        var attributeType = attributeSymbol.ContainingType.ToDisplayString();
                        if (attributeType == LogMethodAttribute)
                        {
                            return classDeclaration;
                        }
                    }
                }
            }
        }

        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        var distinctClasses = classes.Where(c => c is not null).Distinct();

        foreach (var classDeclaration in distinctClasses)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var semanticModel = compilation.GetSemanticModel(classDeclaration!.SyntaxTree);
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
        var hasLogClassAttribute = classSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString() == LogClassAttribute);

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Diagnostics;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine("using AOP.Logging.Core.Interfaces;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName}");
        sb.AppendLine("{");
        sb.AppendLine($"    partial class {className}");
        sb.AppendLine("    {");
        sb.AppendLine("        private IMethodLogger? __methodLogger;");
        sb.AppendLine();
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Sets the method logger for AOP logging.");
        sb.AppendLine("        /// This is typically called by the DI container.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        public void SetMethodLogger(IMethodLogger methodLogger)");
        sb.AppendLine("        {");
        sb.AppendLine("            __methodLogger = methodLogger;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
