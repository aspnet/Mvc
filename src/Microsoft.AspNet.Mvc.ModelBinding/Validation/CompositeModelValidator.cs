using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class CompositeModelValidator : IModelValidator
    {
        private readonly IEnumerable<IModelValidator> _validators;

        public CompositeModelValidator(IEnumerable<IModelValidator> validators)
        {
            _validators = validators;
        }

        public bool IsRequired
        {
            get { return false; }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may perform non-trivial work.")]
        public virtual IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return Enumerable.Empty<ModelClientValidationRule>();
        }

        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            var propertiesValid = true;
            var metadata = context.ModelMetadata;

            foreach (var propertyMetadata in metadata.Properties)
            {
                var propertyContext = new ModelValidationContext(context, propertyMetadata);

                foreach (var propertyValidator in _validators)
                {
                    foreach (var validationResult in propertyValidator.Validate(propertyContext))
                    {
                        propertiesValid = false;
                        yield return CreateSubPropertyResult(propertyMetadata, validationResult);
                    }
                }
            }

            if (propertiesValid)
            {
                foreach (var typeValidator in _validators)
                {
                    foreach (var typeResult in typeValidator.Validate(context))
                    {
                        yield return typeResult;
                    }
                }
            }
        }

        private static ModelValidationResult CreateSubPropertyResult(ModelMetadata propertyMetadata, ModelValidationResult propertyResult)
        {
            return new ModelValidationResult(propertyMetadata.PropertyName + '.' + propertyResult.MemberName,
                                             propertyResult.Message);
        }
    }
}
