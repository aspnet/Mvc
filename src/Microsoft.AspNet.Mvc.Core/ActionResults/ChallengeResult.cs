using System.Collections.Generic;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.Abstractions.Security;

namespace Microsoft.AspNet.Mvc
{
    public class ChallengeResult : ActionResult
    {
        public IList<string> AuthenticationTypes;
        public AuthenticationProperties Properties;

        public ChallengeResult()
            :this(new string[0])
        {
        }

        public ChallengeResult(AuthenticationProperties properties)
            : this(new string[0], properties)
        {
        }

        public ChallengeResult(string authenticationType)
            : this(new[] { authenticationType })
        {
        }

        public ChallengeResult(string authenticationType, AuthenticationProperties properties)
            : this(new[] { authenticationType }, properties)
        {
        }

        public ChallengeResult(IList<string> authenticationTypes)
            : this(authenticationTypes, properties: null)
        {
        }

        public ChallengeResult(IList<string> authenticationTypes, AuthenticationProperties properties)
        {
            AuthenticationTypes = authenticationTypes;
            Properties = properties;
        }

        public override void ExecuteResult([NotNull] ActionContext context)
        {
            HttpResponse response = context.HttpContext.Response;
            response.Challenge(AuthenticationTypes, Properties);
        }
    }
}
