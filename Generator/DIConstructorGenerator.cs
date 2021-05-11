using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Generator
{
    [Generator]
    public class DIConstructorGenerator : ISourceGenerator
    {
        private const string k_injectDependenciesAttribute = @"using System;
namespace Generator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class InjectDependenciesAttribute : Attribute
    {
    }
}";

        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("InjectDependenciesAttribute.g", k_injectDependenciesAttribute);

            foreach (var symbol in GetSymbols(context))
            {
                var privateMembers = symbol.GetMembers()
                    .OfType<IFieldSymbol>()
                    .Where(member => member.DeclaredAccessibility == Accessibility.Private)
                    .ToList();

                StringBuilder sb = new StringBuilder($@"namespace {symbol.ContainingNamespace}
{{
    partial class {symbol.Name}
    {{
        public {symbol.Name}(");
                foreach (var member in privateMembers)
                {
                    sb.Append($"{member.Type} g_{member.Name},");
                }
                sb.Length -= 1;
                sb.Append("){");

                foreach (var member in privateMembers)
                {
                    sb.Append($"this.{member.Name} = g_{member.Name};");
                }

                sb.Append("}}}");

                context.AddSource($"{symbol.Name}.DIConstructor", sb.ToString());
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public static bool HasAttribute(ISymbol c, string attributeName)
            => c.GetAttributes().Any(attr => attr.AttributeClass?.Name.StartsWith(attributeName) ?? false);

        private IEnumerable<INamedTypeSymbol> GetSymbols(GeneratorExecutionContext context)
        {
            var candidates = ((SyntaxReceiver)context.SyntaxReceiver).Candidates;

            foreach (var candidate in candidates)
            {
                var model = context.Compilation.GetSemanticModel(candidate.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(candidate);

                if (HasAttribute(symbol, "InjectDependencies"))
                {
                    yield return symbol;
                }
            }
        }
    }

    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public readonly List<ClassDeclarationSyntax> Candidates = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode node)
        {
            if (node is ClassDeclarationSyntax syntax &&
                syntax.AttributeLists.Count > 0 &&
                syntax.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                Candidates.Add(syntax);
            }
        }
    }
}
