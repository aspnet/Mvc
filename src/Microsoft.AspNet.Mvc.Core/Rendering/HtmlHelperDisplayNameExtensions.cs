namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Display Name-related extensions for <see cref="HtmlHelper"/> and <see cref="HtmlHelper{T}"/>.
    /// </summary>
    public static class HtmlHelperDisplayNameExtensions
    {
        /// <summary>
        /// Gets the display name for the current model.
        /// </summary>
        /// <param name="htmlHelper">The <see cref="HtmlHelper"/> instance that this method extends.</param>
        /// <returns>An <see cref="HtmlString"/> that represents HTML markup.</returns>
        public static HtmlString DisplayNameForModel<TModel>([NotNull] this IHtmlHelper<TModel> htmlHelper)
        {
            return htmlHelper.DisplayName(string.Empty);
        }
    }
}