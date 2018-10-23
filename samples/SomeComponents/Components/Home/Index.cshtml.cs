
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SomeComponents.Components.Home
{
    public class IndexModel : PageModel
    {
        public string Message { get; set; }

        public void OnGet(string query)
        {
            Message = "IndexModel " + query;
        }
    }
}
