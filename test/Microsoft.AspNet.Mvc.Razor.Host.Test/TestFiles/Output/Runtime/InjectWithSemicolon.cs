﻿#pragma checksum "TestFiles/Input/InjectWithSemicolon.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "b753615982659a9805e6213ceced76ba06782038"
namespace Asp
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Microsoft.AspNet.Mvc;
    using Microsoft.AspNet.Mvc.Rendering;
    using System.Threading.Tasks;

    public class ASPV_TestFiles_Input_InjectWithSemicolon_cshtml : Microsoft.AspNet.Mvc.Razor.RazorPage<
#line 1 "TestFiles/Input/InjectWithSemicolon.cshtml"
       MyModel

#line default
#line hidden
    >
    {
        #line hidden
        public ASPV_TestFiles_Input_InjectWithSemicolon_cshtml()
        {
        }
        #line hidden
        [Microsoft.AspNet.Mvc.ActivateAttribute]
        public MyApp MyPropertyName { get; private set; }
        [Microsoft.AspNet.Mvc.ActivateAttribute]
        public MyService<MyModel> Html { get; private set; }
        [Microsoft.AspNet.Mvc.ActivateAttribute]
        public MyApp MyPropertyName2 { get; private set; }
        [Microsoft.AspNet.Mvc.ActivateAttribute]
        public MyService<MyModel> Html2 { get; private set; }
        [Microsoft.AspNet.Mvc.ActivateAttribute]
        public Microsoft.AspNet.Mvc.IViewComponentHelper Component { get; private set; }
        [Microsoft.AspNet.Mvc.ActivateAttribute]
        public Microsoft.AspNet.Mvc.IUrlHelper Url { get; private set; }

        #line hidden

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
        }
        #pragma warning restore 1998
    }
}
