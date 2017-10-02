using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace KodKod.Analyzer
{
    public abstract class KodKodAnalyzerAnalyzerBase : DiagnosticAnalyzer
    {
        public KodKodAnalyzerAnalyzerBase(DiagnosticDescriptor diagnostic)
        {
            SupportedDiagnostics = ImmutableArray.Create(diagnostic);
        }

        protected DiagnosticDescriptor SupportedDiagnostic => SupportedDiagnostics[0];

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var kodKodContext = new KodKodContext(compilationContext);

                // Only do work if ApiControllerAttribute is defined.
                if (kodKodContext.ApiControllerAttribute == null)
                {
                    return;
                }

                InitializeWorker(kodKodContext);
            });
        }

        protected abstract void InitializeWorker(KodKodContext kodKodContext);
    }
}
