using System;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelExplorer
    {
        private object _model;
        private Func<object> _modelAccessor;
        private Type _modelType;

        public ModelExplorer([NotNull] ModelMetadata metadata, object model)
        {
            Metadata = metadata;
            Model = model;
        }

        public ModelExplorer([NotNull] ModelMetadata metadata, Func<object> modelAccessor)
        {
            Metadata = metadata;
            _modelAccessor = modelAccessor;
        }

        public ModelExplorer([NotNull] ModelMetadata metadata, object model, ModelExplorer container)
        {
            Metadata = metadata;
            Model = model;
            Container = container;
        }

        public ModelExplorer([NotNull] ModelMetadata metadata, Func<object> modelAccessor, ModelExplorer container)
        {
            Metadata = metadata;
            _modelAccessor = modelAccessor;
            Container = container;
        }

        public ModelExplorer Container { get; }

        public ModelMetadata Metadata { get; }

        public object Model
        {
            get
            {
                if (_model == null && _modelAccessor != null)
                {
                    _model = _modelAccessor();
                    _modelAccessor = null;
                }

                return _model;
            }

            private set
            {
                Debug.Assert(_modelAccessor == null);
                _model = value;
            }
        }

        public ModelExplorer GetProperty(string name)
        {
            var propertyMetadata = Metadata.Properties[name];
            if (propertyMetadata == null)
            {
                return null;
            }

            if (Model == null)
            {
                return new ModelExplorer(propertyMetadata, model: null, container: this);
            }
            else
            {
                var propertyHelper = PropertyHelper.GetProperties(Model.GetType()).Where(p => p.Name == name).FirstOrDefault();
                if (propertyHelper == null)
                {
                    return new ModelExplorer(propertyMetadata, model: null, container: this);
                }

                var accessor = PropertyHelper.MakeFastPropertyGetter(propertyHelper.Property);
                return new ModelExplorer(propertyMetadata, model: accessor(Model), container: this);
            }
        }

        public Type ModelType
        {
            get
            {
                if (_modelType == null)
                {
                    if (Model == null)
                    {
                        // If the model is null, then use the declared model type;
                        _modelType = Metadata.ModelType;
                    }
                    else if (Metadata.IsNullableValueType)
                    {
                        // We have a model, but if it's a nullable value type, then Model.GetType() will return
                        // the non-nullable type (int? -> int). Since it's a value type, there's no subclassing,
                        // just go with the declared type.
                        _modelType = Metadata.ModelType;
                    }
                    else
                    {
                        // We have a model, and it's not a nullable so use the runtime type to handle
                        // cases where the model is a subclass of the declared type and has extra data.
                        _modelType = Model.GetType();
                    }
                }

                return _modelType;
            }
        }
    }
}