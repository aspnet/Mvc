[Microsoft.AspNetCore.Razor.TagHelpers.HtmlTargetElementAttribute("foo")]
public class __Generated__FooViewComponentTagHelper : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
{
    private readonly global::Microsoft.AspNetCore.Mvc.IViewComponentHelper _viewComponentHelper = null;
    public __Generated__FooViewComponentTagHelper(global::Microsoft.AspNetCore.Mvc.IViewComponentHelper viewComponentHelper)
    {
        _viewComponentHelper = viewComponentHelper;
    }
    [Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeNotBoundAttribute, global::Microsoft.AspNetCore.Mvc.Rendering.ViewContext]
    public global::Microsoft.AspNetCore.Mvc.Rendering.ViewContext ViewContext { get; set; }
    public System.String Attribute { get; set; }
    public override async global::System.Threading.Tasks.Task ProcessAsync(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext context, Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput output)
    {
        (_viewComponentHelper as global::Microsoft.AspNetCore.Mvc.ViewFeatures.IViewContextAware)?.Contextualize(ViewContext);
        var viewContent = await _viewComponentHelper.InvokeAsync("Foo",  new { Attribute });
        output.TagName = null;
        output.Content.SetHtmlContent(viewContent);
    }
}
[Microsoft.AspNetCore.Razor.TagHelpers.HtmlTargetElementAttribute("bar")]
public class __Generated__BarViewComponentTagHelper : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
{
    private readonly global::Microsoft.AspNetCore.Mvc.IViewComponentHelper _viewComponentHelper = null;
    public __Generated__BarViewComponentTagHelper(global::Microsoft.AspNetCore.Mvc.IViewComponentHelper viewComponentHelper)
    {
        _viewComponentHelper = viewComponentHelper;
    }
    [Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeNotBoundAttribute, global::Microsoft.AspNetCore.Mvc.Rendering.ViewContext]
    public global::Microsoft.AspNetCore.Mvc.Rendering.ViewContext ViewContext { get; set; }
    public System.String Attribute { get; set; }
    public override async global::System.Threading.Tasks.Task ProcessAsync(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext context, Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput output)
    {
        (_viewComponentHelper as global::Microsoft.AspNetCore.Mvc.ViewFeatures.IViewContextAware)?.Contextualize(ViewContext);
        var viewContent = await _viewComponentHelper.InvokeAsync("Bar",  new { Attribute });
        output.TagName = null;
        output.Content.SetHtmlContent(viewContent);
    }
}
