// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Resources;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.Razor.Precompilation
{
    [TargetElement("img", Attributes = AppendVersionAttributeName + "," + SrcAttributeName)]
    [TargetElement("image", Attributes = SrcAttributeName)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ResponseCache(Location = ResponseCacheLocation.Client)]
    [CustomValidation(typeof(Validator), "ValidationMethod", ErrorMessageResourceType = typeof(ResourceManager))]
    [RestrictChildren("ol", "ul", "li", "dl", "dd")]
    [Bind("include1", "include2", Prefix = "include-prefix")]
    [AttributeWithArrayProperties(
        ArrayOfInts = new[] { 7, 8, 9 },
        ArrayOfTypes = new[] { typeof(ITagHelper), typeof(Guid) },
        Days = new[] { DayOfWeek.Saturday })]
    public class TypeWithAttributes
    {
        private const string AppendVersionAttributeName = "asp-append-version";
        private const string SrcAttributeName = "src";

        [HtmlAttributeName(SrcAttributeName)]
        [Required(AllowEmptyStrings = true)]
        public string Src { get; set; }

        [HtmlAttributeName(AppendVersionAttributeName, DictionaryAttributePrefix = "prefix")]
        [HtmlAttributeNotBound]
        public bool AppendVersion { get; set; }

        [ViewContext]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [AttributeWithArrayProperties(
            ArrayOfInts = new int[0],
            ArrayOfTypes = new[] { typeof(TypeWithAttributes) },
            Days = new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Sunday })]
        public ViewContext ViewContext { get; set; }

        [AttributesWithArrayConstructorArguments(new[] { 1, 2 }, new[] { typeof(Uri), typeof(IList<>) })]
        [AttributesWithArrayConstructorArguments(new[] { "Hello", "world" }, new[] { typeof(List<Guid>) }, new int[0])]
        [AttributesWithArrayConstructorArguments(
            new[] { "world", "Hello" },
            new[] { typeof(IDictionary<string, object>) },
            new[] { 1 })]
        protected IHostingEnvironment HostingEnvironment { get; }

        [Derived(BaseProperty = "BaseValue", DerivedProperty = "DerivedValue")]
        public string FormId { get; set; }
    }
}
