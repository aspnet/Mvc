using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class RazorPagesOptionsSetup : IConfigureOptions<RazorPagesOptions>
    {
        public void Configure(RazorPagesOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Support for [TempData] on properties
            options.ConfigureFilter(new SaveTempDataPropertyFilter());
            // Always require an antiforgery token on post
            options.ConfigureFilter(new AutoValidateAntiforgeryTokenAttribute());
        }
    }
}
