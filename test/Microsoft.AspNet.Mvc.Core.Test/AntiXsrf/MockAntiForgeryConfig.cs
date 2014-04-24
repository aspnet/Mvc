namespace Microsoft.AspNet.Mvc.Core.Test
{
    public sealed class MockAntiForgeryConfig : IAntiForgeryConfig
    {
        public string CookieName
        {
            get;
            set;
        }

        public string FormFieldName
        {
            get;
            set;
        }

        public bool RequireSSL
        {
            get;
            set;
        }

        public bool SuppressXFrameOptionsHeader
        {
            get;
            set;
        }
    }
}