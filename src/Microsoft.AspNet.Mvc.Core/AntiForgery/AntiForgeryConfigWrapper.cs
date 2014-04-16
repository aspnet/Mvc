
namespace Microsoft.AspNet.Mvc
{
    public sealed class AntiForgeryConfigWrapper : IAntiForgeryConfig
    {
        public IAntiForgeryAdditionalDataProvider AdditionalDataProvider
        {
            get
            {
                return AntiForgeryConfig.AdditionalDataProvider;
            }
        }

        public string CookieName
        {
            get { return AntiForgeryConfig.CookieName; }
        }

        public string FormFieldName
        {
            get { return AntiForgeryConfig.AntiForgeryTokenFieldName; }
        }

        public bool RequireSSL
        {
            get { return AntiForgeryConfig.RequireSsl; }
        }

        public bool SuppressIdentityHeuristicChecks
        {
            get { return AntiForgeryConfig.SuppressIdentityHeuristicChecks; }
        }

        public bool SuppressXFrameOptionsHeader
        {
            get { return AntiForgeryConfig.SuppressXFrameOptionsHeader; }
        }
    }
}