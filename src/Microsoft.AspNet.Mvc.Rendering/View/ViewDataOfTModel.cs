// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class ViewData<TModel> : ViewData
    {
        private readonly ModelMetadata _defaultModelMetadata;

        public ViewData([NotNull] IModelMetadataProvider metadataProvider)
            : base(metadataProvider)
        {
            _defaultModelMetadata = metadataProvider.GetMetadataForType(null, typeof(TModel));
        }

        public ViewData(ViewData source)
            : base(source)
        {
            var original = source as ViewData<TModel>;
            if (original != null)
            {
                _defaultModelMetadata = original._defaultModelMetadata;
            }
            else
            {
                _defaultModelMetadata = MetadataProvider.GetMetadataForType(null, typeof(TModel));
            }
        }

        public new TModel Model
        {
            get { return (TModel)base.Model; }
            set { SetModel(value); }
        }

        /// <summary>
        /// Fallback <see cref="ModelMetadata"/> based on <typeparamref name="TModel"/>. Base <see cref="ViewData"/>
        /// class will use this property when <see cref="Model"/> is <c>null</c> and it is thus unable to determine the
        /// correct metadata.
        /// </summary>
        protected override ModelMetadata DefaultModelMetadata
        {
            get
            {
                return _defaultModelMetadata;
            }
        }

        protected override void SetModel(object value)
        {
            // IsCompatibleWith verifies if the value is either an instance of TModel or if value happens to be null
            // that TModel is nullable type.
            var castWillSucceed = typeof(TModel).IsCompatibleWith(value);

            if (castWillSucceed)
            {
                base.SetModel(value);
            }
            else
            {
                string message;
                if (value == null)
                {
                    message = Resources.FormatViewData_ModelCannotBeNull(typeof(TModel));
                }
                else
                {
                    message = Resources.FormatViewData_WrongTModelType(value.GetType(), typeof(TModel));
                }

                throw new InvalidOperationException(message);
            }
        }
    }
}
