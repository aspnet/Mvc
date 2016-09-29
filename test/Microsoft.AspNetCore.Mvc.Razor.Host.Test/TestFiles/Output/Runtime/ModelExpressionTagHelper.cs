#pragma checksum "TestFiles/Input/ModelExpressionTagHelper.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "faaab08eebb321aea098bd40df018e89cd247b6f"
namespace AspNetCore
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using System.Threading.Tasks;

    public class TestFiles_Input_ModelExpressionTagHelper_cshtml : Microsoft.AspNetCore.Mvc.Razor.RazorPage<DateTime>
    {
        #line hidden
        #pragma warning disable 0414
        private string __tagHelperStringValueBuffer = null;
        #pragma warning restore 0414
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Mvc.Razor.InputTestTagHelper __Microsoft_AspNetCore_Mvc_Razor_InputTestTagHelper = null;
        #line hidden
        public TestFiles_Input_ModelExpressionTagHelper_cshtml()
        {
        }
        #line hidden
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<DateTime> Html { get; private set; }

        #line hidden

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            __tagHelperRunner = __tagHelperRunner ?? new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
            __tagHelperScopeManager = __tagHelperScopeManager ?? new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
            BeginContext(244, 2, true);
            WriteLiteral("\r\n");
            EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input-test", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_InputTestTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.InputTestTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_InputTestTagHelper);
#line 6 "TestFiles/Input/ModelExpressionTagHelper.cshtml"
__Microsoft_AspNetCore_Mvc_Razor_InputTestTagHelper.For = ModelExpressionProvider.CreateModelExpression(ViewData, __model => __model.Now);

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("for", __Microsoft_AspNetCore_Mvc_Razor_InputTestTagHelper.For, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            BeginContext(246, 24, false);
            Write(__tagHelperExecutionContext.Output);
            EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            BeginContext(270, 2, true);
            WriteLiteral("\r\n");
            EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input-test", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_InputTestTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.InputTestTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_InputTestTagHelper);
#line 7 "TestFiles/Input/ModelExpressionTagHelper.cshtml"
__Microsoft_AspNetCore_Mvc_Razor_InputTestTagHelper.For = ModelExpressionProvider.CreateModelExpression(ViewData, __model => Model);

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("for", __Microsoft_AspNetCore_Mvc_Razor_InputTestTagHelper.For, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            BeginContext(272, 27, false);
            Write(__tagHelperExecutionContext.Output);
            EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            BeginContext(299, 8, true);
            WriteLiteral("\r\n\r\n<div");
            EndContext();
            BeginWriteAttribute("prefix-test", " prefix-test=\"", 307, "\"", 327, 1);
#line 9 "TestFiles/Input/ModelExpressionTagHelper.cshtml"
WriteAttributeValue("", 321, Model, 321, 6, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(328, 65, true);
            WriteLiteral(" ></div>\r\n<span prefix-hour=\"Hour\" prefix-minute=\"Minute\"></span>");
            EndContext();
        }
        #pragma warning restore 1998
    }
}
