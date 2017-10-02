using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace KodKod.Analyzer
{
    public class KodKodContext
    {
#pragma warning disable RS1012 // Start action has no registered actions.
        public KodKodContext(CompilationStartAnalysisContext context)
#pragma warning restore RS1012 // Start action has no registered actions.
        {
            Context = context;
            ApiControllerAttribute = context.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.ApiControllerAttribute");
        }

        public CompilationStartAnalysisContext Context { get; }

        public INamedTypeSymbol ApiControllerAttribute { get; }

        private INamedTypeSymbol _routeAttribute;
        public INamedTypeSymbol RouteAttribute => GetType("Microsoft.AspNetCore.Mvc.Routing.IRouteTemplateProvider", ref _routeAttribute);

        private INamedTypeSymbol _actionResultOfT;
        public INamedTypeSymbol ActionResultOfT => GetType("Microsoft.AspNetCore.Mvc.ActionResult`1", ref _actionResultOfT);

        private INamedTypeSymbol _systemThreadingTask;
        public INamedTypeSymbol SystemThreadingTask => GetType("System.Threading.Tasks.Task", ref _systemThreadingTask);

        private INamedTypeSymbol _objectResult;
        public INamedTypeSymbol ObjectResult => GetType("Microsoft.AspNetCore.Mvc.ObjectResult", ref _objectResult);

        private INamedTypeSymbol _iActionResult;
        public INamedTypeSymbol IActionResult => GetType("Microsoft.AspNetCore.Mvc.IActionResult", ref _iActionResult);

        public INamedTypeSymbol _modelState;
        public INamedTypeSymbol ModelStateDictionary => GetType("Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary", ref _modelState);

        public bool IsKodKod(INamedTypeSymbol type) => type.GetAttributes().Any(a => a.AttributeClass == ApiControllerAttribute);

        public bool IsKodKod(IMethodSymbol method) => KodKodFacts.IsKodKod(ApiControllerAttribute, method);

        private INamedTypeSymbol GetType(string name, ref INamedTypeSymbol cache) =>
            cache = cache ?? Context.Compilation.GetTypeByMetadataName(name);
    }
}
