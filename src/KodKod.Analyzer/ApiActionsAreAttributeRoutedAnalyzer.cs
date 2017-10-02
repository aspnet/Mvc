using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace KodKod.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ApiActionsAreAttributeRoutedAnalyzer : KodKodAnalyzerAnalyzerBase
    {
        public ApiActionsAreAttributeRoutedAnalyzer()
            : base(DiagnosticDescriptors.KK1000_ApiActionsMustBeAttributeRouted)
        {
        }

        protected override void InitializeWorker(KodKodContext kodKodContext)
        {
            kodKodContext.Context.RegisterSymbolAction(context =>
            {
                var method = (IMethodSymbol)context.Symbol;

                if (!kodKodContext.IsKodKod(method.ContainingType))
                {
                    return;
                }

                foreach (var attribute in method.GetAttributes())
                {
                    if (attribute.AttributeClass.AllInterfaces.Any(t => t == kodKodContext.RouteAttribute))
                    {
                        return;
                    }
                }

                string idParameterName = null;
                foreach (var parameter in method.Parameters)
                {
                    if (parameter.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                    {
                        idParameterName = parameter.Name;
                        break;
                    }
                }

                var properties = ImmutableDictionary.Create<string, string>()
                    .Add("MethodName", method.Name)
                    .Add("IdParameterName", idParameterName);

                context.ReportDiagnostic(Diagnostic.Create(
                    SupportedDiagnostic,
                    context.Symbol.Locations[0],
                    properties: properties));

            }, SymbolKind.Method);
        }
    }
}
