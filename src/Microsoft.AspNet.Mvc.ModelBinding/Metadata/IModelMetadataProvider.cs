﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public interface IModelMetadataProvider
    {
        IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType);

        ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName);

        ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType);

        ModelMetadata GetMetadataForParameter(ParameterInfo parameterInfo);
    }
}
