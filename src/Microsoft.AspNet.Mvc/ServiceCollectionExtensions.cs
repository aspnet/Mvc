
using Microsoft.AspNet.ConfigurationModel;
using Microsoft.AspNet.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMvc(this IServiceCollection services)
        {
            return services.Add(MvcServices.GetDefaultServices());
        }

        public static IServiceCollection AddMvc(this IServiceCollection services, IConfiguration configuration)
        {
            return services.Add(MvcServices.GetDefaultServices(configuration));
        }
    }
}
