// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    /// <summary>
    /// Read / write <see cref="IModelBindingMessages"/> implementation.
    /// </summary>
    public class ModelBindingMessages : IModelBindingMessages
    {
        private Func<object, string> _missingBindRequiredValueResource;
        private Func<object, string> _missingKeyOrValueResource;
        private Func<object, string> _valueInvalid_MustNotBeNullResource;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBindingMessages"/> class.
        /// </summary>
        public ModelBindingMessages()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBindingMessages"/> class based on
        /// <paramref name="originalMessages"/>.
        /// </summary>
        /// <param name="originalMessages">The <see cref="ModelBindingMessages"/> to duplicate.</param>
        public ModelBindingMessages(ModelBindingMessages originalMessages)
        {
            if (originalMessages == null)
            {
                throw new ArgumentNullException(nameof(originalMessages));
            }

            MissingBindRequiredValueResource = originalMessages.MissingBindRequiredValueResource;
            MissingKeyOrValueResource = originalMessages.MissingKeyOrValueResource;
            ValueInvalid_MustNotBeNullResource = originalMessages.ValueInvalid_MustNotBeNullResource;
        }

        /// <inheritdoc/>
        public Func<object, string> MissingBindRequiredValueResource
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
        public Func<object, string> MissingKeyOrValueResource
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
        public Func<object, string> ValueInvalid_MustNotBeNullResource
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
