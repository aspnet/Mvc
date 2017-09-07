// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ProblemDescriptionApplicationModelProvider : IApplicationModelProvider
    {
        public static readonly object ProblemDescriptionAttributeKey = new object();

        /// <remarks>
        /// The order is set to execute after the <see cref="DefaultApplicationModelProvider"/>.
        /// </remarks>
        public int Order => -1000 + 10;

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            foreach (var controllerModel in context.Result.Controllers)
            {
                var controllerProblemDescriptionAttribute = controllerModel.Attributes.OfType<ProblemDescriptionAttribute>().FirstOrDefault();
                foreach (var actionModel in controllerModel.Actions)
                {
                    var actionProblemDescriptionAttribute = actionModel.Attributes.OfType<ProblemDescriptionAttribute>().FirstOrDefault();
                    var effectiveProblemDescriptionAttribute = (actionProblemDescriptionAttribute ?? controllerProblemDescriptionAttribute);
                    if (effectiveProblemDescriptionAttribute != null)
                    {
                        actionModel.Properties.Add(ProblemDescriptionAttributeKey, effectiveProblemDescriptionAttribute);
                    }
                }
            }
        }
    }
}
