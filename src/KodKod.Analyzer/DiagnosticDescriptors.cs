using Microsoft.CodeAnalysis;

namespace KodKod.Analyzer
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor KK1000_ApiActionsMustBeAttributeRouted =
            new DiagnosticDescriptor(
                "KK1000",
                "Actions on types annotated with ApiControllerAttribute must be attribute routed.",
                "Actions on types annotated with ApiControllerAttribute must be attribute routed.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor KK1001_ApiActionsHaveBadModelStateFilter =
            new DiagnosticDescriptor(
                "KK1001",
                "Actions on types annotated with ApiControllerAttribute do not require explicit ModelState validity check.",
                "Actions on types annotated with ApiControllerAttribute do not require explicit ModelState validity check.",
                "Usage",
                DiagnosticSeverity.Info,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor KK1002_ApiActionsShouldReturnActionResultOf =
            new DiagnosticDescriptor(
                "KK1002",
                "Actions on types annotated with ApiControllerAttribute should return ActionResult<T> when allowed.",
                "Actions on types annotated with ApiControllerAttribute should return ActionResult<T> when allowed.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);
    }
}
