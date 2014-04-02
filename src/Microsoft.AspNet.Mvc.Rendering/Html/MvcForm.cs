// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc.Properties;

namespace System.Web.Mvc.Html
{
    public class MvcForm : IDisposable
    {
        private readonly ViewContext _viewContext;
        private bool _disposed;

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "httpResponse", Justification = "This method existed in MVC 1.0 and has been deprecated.")]
        [Obsolete("This constructor is obsolete, because its functionality has been moved to MvcForm(ViewContext) now.", true /* error */)]
        public MvcForm(HttpResponseBase httpResponse)
        {
            throw new InvalidOperationException(MvcResources.MvcForm_ConstructorObsolete);
        }

        public MvcForm(ViewContext viewContext)
        {
            if (viewContext == null)
            {
                throw new ArgumentNullException("viewContext");
            }

            _viewContext = viewContext;

            // push the new FormContext
            _viewContext.FormContext = new FormContext();
        }

        public void Dispose()
        {
            Dispose(true /* disposing */);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                FormExtensions.EndForm(_viewContext);
            }
        }

        public void EndForm()
        {
            Dispose(true);
        }
    }
}
