using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesWebSite
{
    public class PageWithValidation : PageModel
    {
        [BindProperty(SupportsGet = true)]
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [BindProperty(SupportsGet = true)]
        [Range(18, 60, ErrorMessage = "18 ≤ Age ≤ 60")]
        public int Age { get; set; }
    }
}
