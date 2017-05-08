// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Core;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Metadata
{
    /// <summary>
    /// Read / write <see cref="ModelBindingMessageProvider"/> implementation.
    /// </summary>
    public class DefaultModelBindingMessageProvider : ModelBindingMessageProvider
    {
        private Func<string, string> _missingBindRequiredValueAccessor;
        private Func<string> _missingKeyOrValueAccessor;
        private Func<string> _missingRequestBodyRequiredValueAccessor;
        private Func<string, string> _valueMustNotBeNullAccessor;
        private Func<string, string, string> _attemptedValueIsInvalidAccessor;
        private Func<string, string> _unknownValueIsInvalidAccessor;
        private Func<string, string> _valueIsInvalidAccessor;
        private Func<string, string> _valueMustBeANumberAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultModelBindingMessageProvider"/> class.
        /// </summary>
        public DefaultModelBindingMessageProvider()
        {
            SetMissingBindRequiredValueAccessor(Resources.FormatModelBinding_MissingBindRequiredMember);
            SetMissingKeyOrValueAccessor(Resources.FormatKeyValuePair_BothKeyAndValueMustBePresent);
            SetMissingRequestBodyRequiredValueAccessor(Resources.FormatModelBinding_MissingRequestBodyRequiredMember);
            SetValueMustNotBeNullAccessor(Resources.FormatModelBinding_NullValueNotValid);
            SetAttemptedValueIsInvalidAccessor(Resources.FormatModelState_AttemptedValueIsInvalid);
            SetUnknownValueIsInvalidAccessor(Resources.FormatModelState_UnknownValueIsInvalid);
            SetValueIsInvalidAccessor(Resources.FormatHtmlGeneration_ValueIsInvalid);
            SetValueMustBeANumberAccessor(Resources.FormatHtmlGeneration_ValueMustBeNumber);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultModelBindingMessageProvider"/> class based on
        /// <paramref name="originalProvider"/>.
        /// </summary>
        /// <param name="originalProvider">The <see cref="DefaultModelBindingMessageProvider"/> to duplicate.</param>
        public DefaultModelBindingMessageProvider(DefaultModelBindingMessageProvider originalProvider)
        {
            if (originalProvider == null)
            {
                throw new ArgumentNullException(nameof(originalProvider));
            }

            SetMissingBindRequiredValueAccessor(originalProvider.MissingBindRequiredValueAccessor);
            SetMissingKeyOrValueAccessor(originalProvider.MissingKeyOrValueAccessor);
            SetMissingRequestBodyRequiredValueAccessor(originalProvider.MissingRequestBodyRequiredValueAccessor);
            SetValueMustNotBeNullAccessor(originalProvider.ValueMustNotBeNullAccessor);
            SetAttemptedValueIsInvalidAccessor(originalProvider.AttemptedValueIsInvalidAccessor);
            SetUnknownValueIsInvalidAccessor(originalProvider.UnknownValueIsInvalidAccessor);
            SetValueIsInvalidAccessor(originalProvider.ValueIsInvalidAccessor);
            SetValueMustBeANumberAccessor(originalProvider.ValueMustBeANumberAccessor);
        }

        /// <inheritdoc/>
        public override Func<string, string> MissingBindRequiredValueAccessor => _missingBindRequiredValueAccessor;

        /// <summary>
        /// Sets a value for the <see cref="MissingBindRequiredValueAccessor"/> property.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void SetMissingBindRequiredValueAccessor(Func<string, string> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _missingBindRequiredValueAccessor = value;
        }

        /// <inheritdoc/>
        public override Func<string> MissingKeyOrValueAccessor => _missingKeyOrValueAccessor;

        /// <summary>
        /// Sets a value for the <see cref="MissingKeyOrValueAccessor"/> property.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void SetMissingKeyOrValueAccessor(Func<string> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _missingKeyOrValueAccessor = value;
        }

        /// <inheritdoc/>
        public override Func<string> MissingRequestBodyRequiredValueAccessor => _missingRequestBodyRequiredValueAccessor;

        /// <summary>
        /// Sets a value for the <see cref="MissingRequestBodyRequiredValueAccessor"/> property.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void SetMissingRequestBodyRequiredValueAccessor(Func<string> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _missingRequestBodyRequiredValueAccessor = value;
        }

        /// <inheritdoc/>
        public override Func<string, string> ValueMustNotBeNullAccessor => _valueMustNotBeNullAccessor;

        /// <summary>
        /// Sets a value for the <see cref="ValueMustNotBeNullAccessor"/> property.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void SetValueMustNotBeNullAccessor(Func<string, string> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _valueMustNotBeNullAccessor = value;
        }

        /// <inheritdoc/>
        public override Func<string, string, string> AttemptedValueIsInvalidAccessor => _attemptedValueIsInvalidAccessor;

        /// <summary>
        /// Sets a value for the <see cref="AttemptedValueIsInvalidAccessor"/> property.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void SetAttemptedValueIsInvalidAccessor(Func<string, string, string> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _attemptedValueIsInvalidAccessor = value;
        }

        /// <inheritdoc/>
        public override Func<string, string> UnknownValueIsInvalidAccessor => _unknownValueIsInvalidAccessor;

        /// <summary>
        /// Sets a value for the <see cref="UnknownValueIsInvalidAccessor"/> property.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void SetUnknownValueIsInvalidAccessor(Func<string, string> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _unknownValueIsInvalidAccessor = value;
        }

        /// <inheritdoc/>
        public override Func<string, string> ValueIsInvalidAccessor => _valueIsInvalidAccessor;

        /// <summary>
        /// Sets a value for the <see cref="ValueIsInvalidAccessor"/> property.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void SetValueIsInvalidAccessor(Func<string, string> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _valueIsInvalidAccessor = value;
        }

        /// <inheritdoc/>
        public override Func<string, string> ValueMustBeANumberAccessor => _valueMustBeANumberAccessor;

        /// <summary>
        /// Sets a value for the <see cref="ValueMustBeANumberAccessor"/> property.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void SetValueMustBeANumberAccessor(Func<string, string> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _valueMustBeANumberAccessor = value;
        }
    }
}
