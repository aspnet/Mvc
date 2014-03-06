using System;
using System.Collections.ObjectModel;
using Microsoft.AspNet.Mvc.Common;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelErrorCollection : Collection<ModelError>
    {
        public void Add([NotNull]Exception exception)
        {
            Add(new ModelError(exception));
        }

        public void Add([NotNull]string errorMessage)
        {
            Add(new ModelError(errorMessage));
        }
    }
}
