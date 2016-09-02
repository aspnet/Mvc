[Microsoft.AspNetCore.Razor.TagHelpers.HtmlTargetElementAttribute("foo")]
public class __Generated__FooViewComponentTagHelper : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
{
    private readonly Microsoft.AspNetCore.Mvc.IViewComponentHelper _viewComponentHelper = null;
    public __Generated__FooViewComponentTagHelper(Microsoft.AspNetCore.Mvc.IViewComponentHelper viewComponentHelper)
    {
        _viewComponentHelper = viewComponentHelper;
    }
    [Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeNotBoundAttribute, Microsoft.AspNetCore.Mvc.Rendering.ViewContext]
    public Microsoft.AspNetCore.Mvc.Rendering.ViewContext ViewContext { get; set; }
    public System.String Attribute { get; set; }
    public override async System.Threading.Tasks.Task ProcessAsync(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext context, Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput output)
    {
        ((Microsoft.AspNetCore.Mvc.ViewFeatures.IViewContextAware)_viewComponentHelper).Contextualize(ViewContext);
        var viewContent = await _viewComponentHelper.InvokeAsync("Foo",  new { Attribute });
        output.TagName = null;
        output.Content.SetHtmlContent(viewContent);
    }
}
[Microsoft.AspNetCore.Razor.TagHelpers.HtmlTargetElementAttribute("bar")]
public class __Generated__BarViewComponentTagHelper : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
{
    private readonly Microsoft.AspNetCore.Mvc.IViewComponentHelper _viewComponentHelper = null;
    public __Generated__BarViewComponentTagHelper(Microsoft.AspNetCore.Mvc.IViewComponentHelper viewComponentHelper)
    {
        _viewComponentHelper = viewComponentHelper;
    }
    [Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeNotBoundAttribute, Microsoft.AspNetCore.Mvc.Rendering.ViewContext]
    public Microsoft.AspNetCore.Mvc.Rendering.ViewContext ViewContext { get; set; }
    public System.String Attribute { get; set; }
    public override async System.Threading.Tasks.Task ProcessAsync(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext context, Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput output)
    {
        ((Microsoft.AspNetCore.Mvc.ViewFeatures.IViewContextAware)_viewComponentHelper).Contextualize(ViewContext);
        var viewContent = await _viewComponentHelper.InvokeAsync("Bar",  new { Attribute });
        output.TagName = null;
        output.Content.SetHtmlContent(viewContent);
    }
}
