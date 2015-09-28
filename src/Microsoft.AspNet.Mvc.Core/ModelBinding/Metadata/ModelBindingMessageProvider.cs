// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    /// <summary>
    /// Read / write <see cref="IModelBindingMessageProvider"/> implementation.
    /// </summary>
    public class ModelBindingMessageProvider : IModelBindingMessageProvider
    {
        private Func<string, string> _missingBindRequiredValueResource;
        private Func<string> _missingKeyOrValueResource;
        private Func<object, string> _valueInvalid_MustNotBeNullResource;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBindingMessageProvider"/> class.
        /// </summary>
        public ModelBindingMessageProvider()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBindingMessageProvider"/> class based on
        /// <paramref name="originalProvider"/>.
        /// </summary>
        /// <param name="originalProvider">The <see cref="ModelBindingMessageProvider"/> to duplicate.</param>
        public ModelBindingMessageProvider(ModelBindingMessageProvider originalProvider)
        {
            if (originalProvider == null)
            {
                throw new ArgumentNullException(nameof(originalProvider));
            }

            MissingBindRequiredValueAccessor = originalProvider.MissingBindRequiredValueAccessor;
            MissingKeyOrValueAccessor = originalProvider.MissingKeyOrValueAccessor;
            ValueMustNotBeNullAccessor = originalProvider.ValueMustNotBeNullAccessor;
        }

        /// <inheritdoc/>
        public Func<string, string> MissingBindRequiredValueAccessor
        {
            get
            {
                return _missingBindRequiredValueResource;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _missingBindRequiredValueResource = value;
            }
        }

        /// <inheritdoc/>
        public Func<string> MissingKeyOrValueAccessor
        {
            get
            {
                return _missingKeyOrValueResource;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _missingKeyOrValueResource = value;
            }
        }

        /// <inheritdoc/>
        public Func<object, string> ValueMustNotBeNullAccessor
        {
            get
            {
                return _valueInvalid_MustNotBeNullResource;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _valueInvalid_MustNotBeNullResource = value;
            }
        }
    }
}
