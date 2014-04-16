using System;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc
{
    public class AntiForgeryTokenSet
    {
        public AntiForgeryTokenSet(string formToken, string cookieToken)
        {
            if (string.IsNullOrEmpty(formToken))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, formToken);
            }

            if (string.IsNullOrEmpty(cookieToken))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, cookieToken);
            }

            FormToken = formToken;
            CookieToken = cookieToken;
        }

        public string FormToken { get; private set; }

        public string CookieToken { get; private set; }
    }
}