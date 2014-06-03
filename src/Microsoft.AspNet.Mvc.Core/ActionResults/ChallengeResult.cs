using System.Collections.Generic;
using Microsoft.AspNet.Http.Security;

namespace Microsoft.AspNet.Mvc
{
    public class ChallengeResult : ActionResult
    {
        public ChallengeResult()
            :this(new string[] { })
        {
        }

        public ChallengeResult(string authenticationType)
            : this(new[] { authenticationType })
        {
        }

        public ChallengeResult(IList<string> authenticationTypes)
            : this(authenticationTypes, properties: null)
        {
        }

        public ChallengeResult(AuthenticationProperties properties)
            : this(new string[] { }, properties)
        {
        }

        public ChallengeResult(string authenticationType, AuthenticationProperties properties)
            : this(new[] { authenticationType }, properties)
        {
        }

        public ChallengeResult(IList<string> authenticationTypes, AuthenticationProperties properties)
        {
            AuthenticationTypes = authenticationTypes;
            Properties = properties;
        }

        public IList<string> AuthenticationTypes { get; set; }

        public AuthenticationProperties Properties { get; set; }

        public override void ExecuteResult([NotNull] ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.Challenge(AuthenticationTypes, Properties);
        }
    }
}
