// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding.Metadata;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class EmptyModelMetadataProvider : DefaultModelMetadataProvider
    {
        public EmptyModelMetadataProvider()
            : base(new DefaultCompositeMetadataDetailsProvider(new IMetadataDetailsProvider[]
            {
                new MessageOnlyBindingProvider()
            }))
        {
        }

        private class MessageOnlyBindingProvider : IBindingMetadataProvider
        {
            private readonly ModelBindingMessages _modelBindingMessages = CreateBindingMessages();

            public void GetBindingMetadata(BindingMetadataProviderContext context)
            {
                // Don't bother with ModelBindingMessages copy constructor. No other provider can change the messages.
                context.BindingMetadata.ModelBindingMessages = _modelBindingMessages;
            }

            private static ModelBindingMessages CreateBindingMessages()
            {
                return new ModelBindingMessages
                {
                    MissingBindRequiredValueResource = Resources.FormatModelBinding_MissingBindRequiredMember,
                    MissingKeyOrValueResource = Resources.FormatKeyValuePair_BothKeyAndValueMustBePresent,
                    ValueInvalid_MustNotBeNullResource = Resources.FormatModelBinding_NullValueNotValid,
                };
            }
        }
    }
}