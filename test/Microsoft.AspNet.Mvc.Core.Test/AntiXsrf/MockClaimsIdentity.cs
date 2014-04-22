
using System.Collections.Generic;
using System.Security.Claims;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    // Convenient class for mocking a ClaimsIdentity instance given some
    // prefabricated Claim instances.
    internal sealed class MockClaimsIdentity : ClaimsIdentity
    {
        private readonly List<Claim> _claims = new List<Claim>();

        public void AddClaim(string claimType, string value)
        {
            _claims.Add(new Claim(claimType, value));
        }

        public override IEnumerable<Claim> Claims
        {
            get { return _claims; }
        }
    }
}
