[Microsoft.AspNetCore.Razor.TagHelpers.HtmlTargetElementAttribute("foo")]
public class __Generated__FooViewComponentTagHelper : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
{
    private readonly IViewComponentHelper _viewComponentHelper = null;
    public __Generated__FooViewComponentTagHelper(IViewComponentHelper viewComponentHelper)
    {
        _viewComponentHelper = viewComponentHelper;
    }
    [Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeNotBoundAttribute, ViewContext]
    public Microsoft.AspNetCore.Mvc.Rendering.ViewContext ViewContext { get; set; }
    public string Attribute { get; set; }
    public override async System.Threading.Tasks.Task ProcessAsync(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext context, Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput output)
    {
        ((IViewContextAware)_viewComponentHelper).Contextualize(ViewContext);
        var viewContent = await _viewComponentHelper.InvokeAsync("Foo",  new { Attribute });
        output.TagName = null;
        output.Content.SetHtmlContent(viewContent);
    }
}
[Microsoft.AspNetCore.Razor.TagHelpers.HtmlTargetElementAttribute("bar")]
public class __Generated__BarViewComponentTagHelper : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
{
    private readonly IViewComponentHelper _viewComponentHelper = null;
    public __Generated__BarViewComponentTagHelper(IViewComponentHelper viewComponentHelper)
    {
        _viewComponentHelper = viewComponentHelper;
    }
    [Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeNotBoundAttribute, ViewContext]
    public Microsoft.AspNetCore.Mvc.Rendering.ViewContext ViewContext { get; set; }
    public string Attribute { get; set; }
    public override async System.Threading.Tasks.Task ProcessAsync(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext context, Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput output)
    {
        ((IViewContextAware)_viewComponentHelper).Contextualize(ViewContext);
        var viewContent = await _viewComponentHelper.InvokeAsync("Bar",  new { Attribute });
        output.TagName = null;
        output.Content.SetHtmlContent(viewContent);
    }
}
