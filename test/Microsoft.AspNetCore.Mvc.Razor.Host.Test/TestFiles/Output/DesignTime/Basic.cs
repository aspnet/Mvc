namespace AspNetCore
{
    #line hidden
    using System;
    using System.Threading.Tasks;
#line 2 ""
using System.Linq;

#line default
#line hidden
#line 3 ""
using System.Collections.Generic;

#line default
#line hidden
#line 4 ""
using Microsoft.AspNetCore.Mvc;

#line default
#line hidden
#line 5 ""
using Microsoft.AspNetCore.Mvc.Rendering;

#line default
#line hidden
#line 6 ""
using Microsoft.AspNetCore.Mvc.ViewFeatures;

#line default
#line hidden
    public class TestFiles_Input_Basic_cshtml : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 219
        private void __RazorDirectiveTokenHelpers__() {
        ((System.Action)(() => {
global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<TModel> __typeHelper = null;
        }
        ))();
        ((System.Action)(() => {
System.Object Html = null;
        }
        ))();
        ((System.Action)(() => {
global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper __typeHelper = null;
        }
        ))();
        ((System.Action)(() => {
System.Object Json = null;
        }
        ))();
        ((System.Action)(() => {
global::Microsoft.AspNetCore.Mvc.IViewComponentHelper __typeHelper = null;
        }
        ))();
        ((System.Action)(() => {
System.Object Component = null;
        }
        ))();
        ((System.Action)(() => {
global::Microsoft.AspNetCore.Mvc.IUrlHelper __typeHelper = null;
        }
        ))();
        ((System.Action)(() => {
System.Object Url = null;
        }
        ))();
        ((System.Action)(() => {
global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider __typeHelper = null;
        }
        ))();
        ((System.Action)(() => {
System.Object ModelExpressionProvider = null;
        }
        ))();
        ((System.Action)(() => {
System.Object __typeHelper = "Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper, Microsoft.AspNetCore.Mvc.Razor";
        }
        ))();
        ((System.Action)(() => {
System.Object __typeHelper = "Microsoft.AspNetCore.Mvc.Razor.TagHelpers.HeadTagHelper, Microsoft.AspNetCore.Mvc.Razor";
        }
        ))();
        ((System.Action)(() => {
System.Object __typeHelper = "Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper, Microsoft.AspNetCore.Mvc.Razor";
        }
        ))();
        }
        #pragma warning restore 219
        private static System.Object __o = null;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 1 "TestFiles/Input/Basic.cshtml"
       __o = logo;

#line default
#line hidden
#line 3 "TestFiles/Input/Basic.cshtml"
__o = Html.Input("SomeKey");

#line default
#line hidden
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
