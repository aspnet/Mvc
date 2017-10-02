using System.Linq;
using Microsoft.CodeAnalysis;

namespace KodKod.Analyzer
{
    static class KodKodFacts
    {
        public static bool IsKodKod(INamedTypeSymbol apiControllerAttribute, INamedTypeSymbol type) =>
            type.GetAttributes().Any(a => a.AttributeClass == apiControllerAttribute);

        public static bool IsKodKod(INamedTypeSymbol apiControllerAttribute, IMethodSymbol method)
        {
            return
                IsKodKod(apiControllerAttribute, method.ContainingType) &&
                method.DeclaredAccessibility == Accessibility.Public &&
                !method.IsAbstract &&
                !method.IsStatic;
        }
    }
}
