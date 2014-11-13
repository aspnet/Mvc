namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// Provides programmatic configuration for the <see cref="ScriptTagHelper"/>.
    /// </summary>
    public class ScriptTagHelperOptions
    {
        public ScriptTagHelperOptions()
        {
            MinExtension = ".min.js";
        }

        /// <summary>
        /// The file extension of minified JavaScript files.
        /// Defaults to ".min.js".
        /// </summary>
        public string MinExtension { get; set; }
    }
}