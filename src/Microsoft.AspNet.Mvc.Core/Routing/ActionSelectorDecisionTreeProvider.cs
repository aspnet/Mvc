﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc.Routing
{
    /// <summary>
    /// Stores an <see cref="ActionSelectionDecisionTree"/> for the current value of 
    /// <see cref="IActionDescriptorsCollectionProvider.ActionDescriptors"/>.
    /// </summary>
    public class ActionSelectorDecisionTreeProvider
    {
        private readonly IActionDescriptorsCollectionProvider _actionDescriptorsCollectionProvider;
        private ActionSelectionDecisionTree _decisionTree;

        /// <summary>
        /// Creates a new <see cref="ActionSelectorDecisionTreeProvider"/>.
        /// </summary>
        /// <param name="actionDescriptorsCollectionProvider">
        /// The <see cref="IActionDescriptorsCollectionProvider"/>.
        /// </param>
        public ActionSelectorDecisionTreeProvider(
            IActionDescriptorsCollectionProvider actionDescriptorsCollectionProvider)
        {
            _actionDescriptorsCollectionProvider = actionDescriptorsCollectionProvider;
        }

        /// <summary>
        /// Gets the <see cref="IActionDescriptorsCollectionProvider"/>.
        /// </summary>
        public ActionSelectionDecisionTree DecisionTree
        {
            get
            {
                var descriptors = _actionDescriptorsCollectionProvider.ActionDescriptors;
                if (descriptors == null)
                {
                    throw new InvalidOperationException(
                        Resources.FormatPropertyOfTypeCannotBeNull(
                            "ActionDescriptors",
                            _actionDescriptorsCollectionProvider.GetType()));
                }

                if (_decisionTree == null || descriptors.Version != _decisionTree.Version)
                {
                    _decisionTree = new ActionSelectionDecisionTree(descriptors);
                }

                return _decisionTree;
            }
        }
    }
}