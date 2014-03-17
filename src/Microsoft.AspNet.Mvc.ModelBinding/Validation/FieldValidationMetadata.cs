// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class FieldValidationMetadata
    {
        private readonly Collection<ModelClientValidationRule> _validationRules =
            new Collection<ModelClientValidationRule>();
        private string _fieldName = string.Empty;

        public string FieldName
        {
            get { return _fieldName; }
            set { _fieldName = value; }
        }

        public bool ReplaceValidationMessageContents { get; set; }

        public string ValidationMessageId { get; set; }

        public ICollection<ModelClientValidationRule> ValidationRules
        {
            get { return _validationRules; }
        }
    }
}
