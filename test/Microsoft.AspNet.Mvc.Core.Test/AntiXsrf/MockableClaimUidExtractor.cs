using System.Security.Claims;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    // An IClaimUidExtractor that can be passed to MoQ
    public abstract class MockableClaimUidExtractor : IClaimUidExtractor
    {
        public abstract object ExtractClaimUid(ClaimsIdentity identity);

        string IClaimUidExtractor.ExtractClaimUid(ClaimsIdentity identity)
        {
            return (string)ExtractClaimUid(identity);
        }
    }
}