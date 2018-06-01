using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesWebSite.Pages.ViewDataDictionary
{
    public class Index : PageModel
    {
        public IActionResult OnGet() => Page();

        public override void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            var pageResult = (PageResult)context.Result;
            pageResult.ViewData["Value"] = "Value1";
        }
    }
}
