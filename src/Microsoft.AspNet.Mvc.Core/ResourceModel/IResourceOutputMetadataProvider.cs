using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc.ResourceModel
{
    public interface IResourceOutputMetadataProvider
    {
        IEnumerable<MediaTypeHeaderValue> GetAllPossibleContentTypes(Type dataType, MediaTypeHeaderValue contentType);
    }
}