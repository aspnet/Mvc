namespace Asp
{
    using System.Threading.Tasks;

    public class testfiles_input_injectwithsemicolon_cshtml : Microsoft.AspNetCore.Mvc.Razor.RazorPage<MyModel>
    {
        private static object @__o;
        private void @__RazorDesignTimeHelpers__()
        {
            #pragma warning disable 219
#line 1 "testfiles/input/injectwithsemicolon.cshtml"
var __modelHelper = default(MyModel);

#line default
#line hidden
            #pragma warning restore 219
        }
        #line hidden
        public testfiles_input_injectwithsemicolon_cshtml()
        {
        }
        #line hidden
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public
#line 2 "testfiles/input/injectwithsemicolon.cshtml"
        MyApp MyPropertyName

#line default
#line hidden
        { get; private set; }
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public
#line 3 "testfiles/input/injectwithsemicolon.cshtml"
        MyService<MyModel> Html

#line default
#line hidden
        { get; private set; }
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public
#line 4 "testfiles/input/injectwithsemicolon.cshtml"
        MyApp MyPropertyName2

#line default
#line hidden
        { get; private set; }
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public
#line 5 "testfiles/input/injectwithsemicolon.cshtml"
        MyService<MyModel> Html2

#line default
#line hidden
        { get; private set; }
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }

        #line hidden

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
        }
        #pragma warning restore 1998
    }
}
