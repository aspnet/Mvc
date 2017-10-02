using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace KodKod.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ApiActionsDoNotRequireExplicitModelValidationCheckAnalyzer : KodKodAnalyzerAnalyzerBase
    {
        public ApiActionsDoNotRequireExplicitModelValidationCheckAnalyzer()
            : base(DiagnosticDescriptors.KK1001_ApiActionsHaveBadModelStateFilter)
        {
        }

        protected override void InitializeWorker(KodKodContext kodKodContext)
        {
            kodKodContext.Context.RegisterSyntaxNodeAction(context =>
            {
                var methodSyntax = (MethodDeclarationSyntax)context.Node;
                var method = context.SemanticModel.GetDeclaredSymbol(methodSyntax, context.CancellationToken);
                if (!kodKodContext.IsKodKod(method))
                {
                    return;
                }

                if (method.ReturnsVoid || method.ReturnType == kodKodContext.SystemThreadingTask)
                {
                    // Void or Task returning methods. We don't have to check anything here.
                    return;
                }

                // Only look for top level statements that look like "if (!ModelState.IsValid)"
                foreach (var ifStatement in methodSyntax.Body.ChildNodes().OfType<IfStatementSyntax>())
                {
                    if (ifStatement.Condition is PrefixUnaryExpressionSyntax prefixUnaryExpression &&
                        prefixUnaryExpression.OperatorToken.Kind() == SyntaxKind.ExclamationToken &&
                        prefixUnaryExpression.Operand is MemberAccessExpressionSyntax memberAccess)
                    {
                        var memberAccessSymbol = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken);
                        if (memberAccessSymbol.Symbol is IPropertySymbol property &&
                            property.ContainingType == kodKodContext.ModelStateDictionary &&
                            property.Name == "IsValid")
                        {
                            context.ReportDiagnostic(Diagnostic.Create(SupportedDiagnostic, ifStatement.GetLocation()));
                            return;
                        }
                    }
                }
            }, SyntaxKind.MethodDeclaration);
        }
    }
}
