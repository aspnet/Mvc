﻿using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultParameterDescriptorFactory : IParameterDescriptorFactory
    {
        public ParameterDescriptor GetDescriptor(ParameterInfo parameter)
        {
            bool isFromBody = IsFromBody(parameter);

            return new ParameterDescriptor
            {
                Name = parameter.Name,
                IsOptional = parameter.IsOptional,
                ParameterBindingInfo = isFromBody ? null : GetParameterBindingInfo(parameter),
                BodyParameterInfo = isFromBody ? GetBodyParameterInfo(parameter) : null
            };
        }
        public virtual bool IsFromBody(ParameterInfo parameter)
        {
            return parameter.GetCustomAttribute<FromBodyAttribute>() != null;
        }

        private ParameterBindingInfo GetParameterBindingInfo(ParameterInfo parameter)
        {
            return new ParameterBindingInfo(parameter.Name, parameter.ParameterType);
        }

        private BodyParameterInfo GetBodyParameterInfo(ParameterInfo parameter)
        {
            return new BodyParameterInfo(parameter.ParameterType);
        }
    }
}
