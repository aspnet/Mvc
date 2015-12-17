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
        private Func<string, string> _missingBindRequiredValueAccessor;
        private Func<string> _missingKeyOrValueAccessor;
        private Func<string, string> _valueMustNotBeNullAccessor;
        private Func<string, string> _valueInvalid_UnknownErrorResource;
        private Func<string, string, string> _valueInvalid_WithValueResource;
        private Func<string, string> _valueInvalid_WithoutValueResource;
        private Func<string, string> _noEncodingFoundOnInputFormatter;
        private Func<string, string> _unsupportedContentType;

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
            ValueInvalid_UnknownErrorResource = originalProvider.ValueInvalid_UnknownErrorResource;
            ValueInvalid_WithoutValueResource = originalProvider.ValueInvalid_WithoutValueResource;
            ValueInvalid_WithValueResource = originalProvider.ValueInvalid_WithValueResource;
            UnsupportedContentType = originalProvider.UnsupportedContentType;
            NoEncodingFoundOnInputFormatter = originalProvider.NoEncodingFoundOnInputFormatter;
        }

        /// <inheritdoc/>
        public Func<string, string> MissingBindRequiredValueAccessor
        {
            get
            {
                return _missingBindRequiredValueAccessor;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _missingBindRequiredValueAccessor = value;
            }
        }

        /// <inheritdoc/>
        public Func<string> MissingKeyOrValueAccessor
        {
            get
            {
                return _missingKeyOrValueAccessor;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _missingKeyOrValueAccessor = value;
            }
        }

        /// <inheritdoc/>
        public Func<string, string> ValueInvalid_UnknownErrorResource
        {
            get
            {
                return _valueInvalid_UnknownErrorResource;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _valueInvalid_UnknownErrorResource = value;
            }
        }

        /// <inheritdoc/>
        public Func<string, string> ValueInvalid_WithoutValueResource
        {
            get
            {
                return _valueInvalid_WithoutValueResource;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _valueInvalid_WithoutValueResource = value;
            }
        }

        /// <inheritdoc/>
        public Func<string, string, string> ValueInvalid_WithValueResource
        {
            get
            {
                return _valueInvalid_WithValueResource;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _valueInvalid_WithValueResource = value;
            }
        }

        /// <inheritdoc/>
        public Func<string, string> ValueMustNotBeNullAccessor
        {
            get
            {
                return _valueMustNotBeNullAccessor;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _valueMustNotBeNullAccessor = value;
            }
        }

        /// <inheritdoc/>
        public Func<string, string> NoEncodingFoundOnInputFormatter
        {
            get
            {
                return _noEncodingFoundOnInputFormatter;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _noEncodingFoundOnInputFormatter = value;
            }
        }

        /// <inheritdoc/>
        public Func<string, string> UnsupportedContentType
        {
            get
            {
                return _unsupportedContentType;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _unsupportedContentType = value;
            }
        }
    }
}
