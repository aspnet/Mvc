
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Security.Principal;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.Rendering;
using System.Security.Claims;

namespace Microsoft.AspNet.Mvc
{
    internal sealed class AntiForgeryWorker
    {
        private readonly IAntiForgeryConfig _config;
        private readonly IAntiForgeryTokenSerializer _serializer;
        private readonly ITokenStore _tokenStore;
        private readonly ITokenValidator _validator;
        private readonly ITokenGenerator _generator;

        internal AntiForgeryWorker(IAntiForgeryTokenSerializer serializer, IAntiForgeryConfig config, ITokenStore tokenStore, ITokenGenerator generator, ITokenValidator validator)
        {
            _serializer = serializer;
            _config = config;
            _tokenStore = tokenStore;
            _generator = generator;
            _validator = validator;
        }

        private void CheckSSLConfig(HttpContext httpContext)
        {
            if (_config.RequireSSL && !httpContext.Request.IsSecure)
            {
                throw new InvalidOperationException(Resources.AntiForgeryWorker_RequireSSL);
            }
        }

        private AntiForgeryToken DeserializeToken(string serializedToken)
        {
            return (!String.IsNullOrEmpty(serializedToken))
                ? _serializer.Deserialize(serializedToken)
                : null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Caller will just regenerate token in case of failure.")]
        private AntiForgeryToken DeserializeTokenNoThrow(string serializedToken)
        {
            try
            {
                return DeserializeToken(serializedToken);
            }
            catch
            {
                // ignore failures since we'll just generate a new token
                return null;
            }
        }

        private static IIdentity ExtractIdentity(HttpContext httpContext)
        {
            if (httpContext != null)
            {
                ClaimsPrincipal user = httpContext.User;
             
                if (user != null)
                {
                   return user.Identity;
                }
            }

            return null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Caller will just regenerate token in case of failure.")]
        private AntiForgeryToken GetCookieTokenNoThrow(HttpContext httpContext)
        {
            try
            {
                return _tokenStore.GetCookieToken(httpContext);
            }
            catch
            {
                // ignore failures since we'll just generate a new token
                return null;
            }
        }

        // [ ENTRY POINT ]
        // Generates an anti-XSRF token pair for the current user. The return
        // value is the hidden input form element that should be rendered in
        // the <form>. This method has a side effect: it may set a response
        // cookie.
        public TagBuilder GetFormInputElement(HttpContext httpContext)
        {
            CheckSSLConfig(httpContext);

            AntiForgeryToken oldCookieToken = GetCookieTokenNoThrow(httpContext);
            AntiForgeryToken newCookieToken, formToken;
            GetTokens(httpContext, oldCookieToken, out newCookieToken, out formToken);

            if (newCookieToken != null)
            {
                // If a new cookie was generated, persist it.
                _tokenStore.SaveCookieToken(httpContext, newCookieToken);
            }

            if (!_config.SuppressXFrameOptionsHeader)
            {
                // Adding X-Frame-Options header to prevent ClickJacking. See
                // http://tools.ietf.org/html/draft-ietf-websec-x-frame-options-10
                // for more information.
                httpContext.Response.Headers.Add("X-Frame-Options", new[] { "SAMEORIGIN" });
            }

            // <input type="hidden" name="__AntiForgeryToken" value="..." />
            var retVal = new TagBuilder("input");
            retVal.Attributes["type"] = "hidden";
            retVal.Attributes["name"] = _config.FormFieldName;
            retVal.Attributes["value"] = _serializer.Serialize(formToken);
            return retVal;
        }

        // [ ENTRY POINT ]
        // Generates a (cookie, form) serialized token pair for the current user.
        // The caller may specify an existing cookie value if one exists. If the
        // 'new cookie value' out param is non-null, the caller *must* persist
        // the new value to cookie storage since the original value was null or
        // invalid. This method is side-effect free.
        public void GetTokens(HttpContext httpContext, string serializedOldCookieToken, out string serializedNewCookieToken, out string serializedFormToken)
        {
            CheckSSLConfig(httpContext);

            AntiForgeryToken oldCookieToken = DeserializeTokenNoThrow(serializedOldCookieToken);
            AntiForgeryToken newCookieToken, formToken;
            GetTokens(httpContext, oldCookieToken, out newCookieToken, out formToken);

            serializedNewCookieToken = Serialize(newCookieToken);
            serializedFormToken = Serialize(formToken);
        }

        private void GetTokens(HttpContext httpContext, AntiForgeryToken oldCookieToken, out AntiForgeryToken newCookieToken, out AntiForgeryToken formToken)
        {
            newCookieToken = null;
            if (!_validator.IsCookieTokenValid(oldCookieToken))
            {
                // Need to make sure we're always operating with a good cookie token.
                oldCookieToken = newCookieToken = _generator.GenerateCookieToken();
            }

            Contract.Assert(_validator.IsCookieTokenValid(oldCookieToken));
            formToken = _generator.GenerateFormToken(httpContext, ExtractIdentity(httpContext), oldCookieToken);
        }

        private string Serialize(AntiForgeryToken token)
        {
            return (token != null) ? _serializer.Serialize(token) : null;
        }

        // [ ENTRY POINT ]
        // Given an HttpContext, validates that the anti-XSRF tokens contained
        // in the cookies & form are OK for this request.
        public void Validate(HttpContext httpContext)
        {
            CheckSSLConfig(httpContext);

            // Extract cookie & form tokens
            AntiForgeryToken cookieToken = _tokenStore.GetCookieToken(httpContext);
            AntiForgeryToken formToken = _tokenStore.GetFormToken(httpContext);

            // Validate
            _validator.ValidateTokens(httpContext, ExtractIdentity(httpContext), cookieToken, formToken);
        }

        // [ ENTRY POINT ]
        // Given the serialized string representations of a cookie & form token,
        // validates that the pair is OK for this request.
        public void Validate(HttpContext httpContext, string cookieToken, string formToken)
        {
            CheckSSLConfig(httpContext);

            // Extract cookie & form tokens
            AntiForgeryToken deserializedCookieToken = DeserializeToken(cookieToken);
            AntiForgeryToken deserializedFormToken = DeserializeToken(formToken);

            // Validate
            _validator.ValidateTokens(httpContext, ExtractIdentity(httpContext), deserializedCookieToken, deserializedFormToken);
        }
    }
}