using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesWebSite.Pages.WithViewImportSpecifyingInherits.Page
{
    public class Index : PageModel
    {
        public string Title => "Title set via custom base in a page with model";
    }
}
