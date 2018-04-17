using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MvcSandbox.Pages
{
    [RuntimePageModelConvention]
    public class IndexModel : PageModel
    {
        public virtual void OnGet()
        {
        }
    }

    public class RuntimeIndexModel<T> : IndexModel where T : class
    {
        public override void OnGet()
        {
            base.OnGet();
        }
    }

    public class RuntimePageModelConventionAttribute : Attribute, IPageApplicationModelConvention
    {
        public void Apply(PageApplicationModel model)
        {
            model.ModelType = typeof(RuntimeIndexModel<string>).GetTypeInfo();
        }
    }
}