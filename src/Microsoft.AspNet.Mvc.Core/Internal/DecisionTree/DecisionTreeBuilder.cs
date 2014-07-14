// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.Mvc.Internal.DecisionTree
{
    // This code generates a minimal tree of decision criteria that map known categorical data 
    // (key-value-pairs) to a set of inputs. Action Selection is the best example of how this 
    // can be used, so the comments  here will describe the process from the point-of-view, 
    // though the decision tree is generally applicable to like-problems.
    //
    // Care has been taken here to keep the performance of building the data-structure at a
    // reasonable level, as this has an impact on startup cost for action selection. Additionally
    // we want to hold on to the minimal amount of memory needed once we've built the tree.
    //
    // Ex:
    //  Given actions like the following, create a decision tree that will help action
    //  selection work efficiently.
    //
    //  Given any set of route data it should be possible to traverse the tree using the
    //  presence our route data keys (like action), and whether or not they match any of
    //  the known values for that route data key, to find the set of actions that match
    //  the route data.
    //
    // Actions:
    //
    //  { controller = "Home", action = "Index" }
    //  { controller = "Products", action = "Index" }
    //  { controller = "Products", action = "Buy" }
    //  { area = "Admin", controller = "Users", action = "AddUser" }
    //
    // The generated tree looks like this (json-like-notation):
    //
    //  { 
    //      action : {
    //          "AddUser" : {
    //              controller : {
    //                  "Users" : {
    //                      area : {
    //                          "Admin" : match { area = "Admin", controller = "Users", action = "AddUser" }
    //                      }
    //                  }
    //              }
    //          },
    //          "Buy" : {
    //              controller : {
    //                  "Products" : {
    //                      area : {
    //                          null : match { controller = "Products", action = "Buy" }
    //                      }
    //                  }
    //              }
    //          },
    //          "Index" : {
    //              controller : {
    //                  "Home" : {
    //                      area : {
    //                          null : match { controller = "Home", action = "Index" }
    //                      }
    //                  }
    //                  "Products" : {
    //                      area : {
    //                          "null" : match { controller = "Products", action = "Index" }
    //                      }
    //                  }
    //              }
    //          }
    //      }
    //  }
    public static class DecisionTreeBuilder<T, U>
    {
        public static DecisionTreeNode<T, U> GenerateTree(IReadOnlyList<T> items, IClassifier<T, U> classifier)
        {
            var itemDescriptors = new List<ItemDescriptor<T, U>>();
            for (var i = 0; i < items.Count; i++)
            {
                itemDescriptors.Add(new ItemDescriptor<T, U>()
                {
                    Criteria = classifier.GetCriteria(items[i]),
                    Index = i,
                    Item = items[i],
                });
            }

            var comparer = new DecisionCriterionValueEqualityComparer<U>(classifier.ValueComparer);
            return GenerateNode(
                new TreeBuilderContext(), 
                comparer, 
                itemDescriptors);
        }

        private static DecisionTreeNode<T, U> GenerateNode(
            TreeBuilderContext context,
            DecisionCriterionValueEqualityComparer<U> comparer,
            IList<ItemDescriptor<T, U>> items)
        {
            // The extreme use of generics here is intended to reduce the number of intermediate 
            // allocations of wrapper classes. Performance testing found that building these trees allocates
            // significant memory that we can avoid and that it has a real impact on startup.
            //
            // Explanation:
            //  This is a dictionary of string-key -> criteria.
            //  Each criterion is a dictionary of value-key -> branch.
            //  Each branch is a list of items that match these criteria
            var criteria = 
                new Dictionary<string, Dictionary<DecisionCriterionValue<U>, List<ItemDescriptor<T, U>>>>(StringComparer.OrdinalIgnoreCase);

            // Matches are items that have no remaining criteria - at this point in the tree
            // they are considered accepted.
            var matches = new List<T>();

            // For each item in the working set, we want to map it to it's possible criteria-branch
            // pairings, then reduce that tree to the minimal set.
            foreach (var item in items)
            {
                var unsatisfiedCriteria = 0;

                foreach (var kvp in item.Criteria)
                {
                    // context.CurrentCriteria is the logical 'stack' of criteria that we've already processed
                    // on this branch of the tree.
                    if (context.CurrentCriteria.Contains(kvp.Key))
                    {
                        continue;
                    }

                    unsatisfiedCriteria++;

                    Dictionary<DecisionCriterionValue<U>, List<ItemDescriptor<T, U>>> criterion;
                    if (!criteria.TryGetValue(kvp.Key, out criterion))
                    {
                        criterion = new Dictionary<DecisionCriterionValue<U>, List<ItemDescriptor<T, U>>>(comparer);
                        criteria.Add(kvp.Key, criterion);
                    }

                    List<ItemDescriptor<T, U>> branch;
                    if (!criterion.TryGetValue(kvp.Value, out branch))
                    {
                        branch = new List<ItemDescriptor<T, U>>();
                        criterion.Add(kvp.Value, branch);
                    }

                    branch.Add(item);
                }

                // If all of the criteria on item are satisfied by the 'stack' then this item is a match.
                if (unsatisfiedCriteria == 0)
                {
                    matches.Add(item.Item);
                }
            }

            // Iterate criteria in order of branchiness to determine which one to explore next. If a criterion
            // has no 'new' matches under it then we can just eliminate that part of the tree.
            var reducedCriteria = new List<DecisionCriterion<T, U>>();
            foreach (var criterion in criteria.OrderByDescending(c => c.Value.Count))
            {
                var reducedBranches = new Dictionary<U, DecisionTreeNode<T, U>>(comparer.InnerComparer);
                DecisionTreeNode<T, U> fallback = null;

                foreach (var branch in criterion.Value)
                {
                    var reducedItems = new List<ItemDescriptor<T, U>>();
                    foreach (var item in branch.Value)
                    {
                        if (context.MatchedItems.Add(item))
                        {
                            reducedItems.Add(item);
                        }
                    }

                    if (reducedItems.Count > 0)
                    {
                        var childContext = new TreeBuilderContext(context);
                        childContext.CurrentCriteria.Add(criterion.Key);

                        var newBranch = GenerateNode(childContext, comparer, branch.Value);
                        if (branch.Key.IsCatchAll)
                        {
                            fallback = newBranch;
                        }
                        else
                        {
                            reducedBranches.Add(branch.Key.Value, newBranch);
                        }
                    }
                }

                if (reducedBranches.Count > 0 || fallback != null)
                {
                    var newCriterion = new DecisionCriterion<T, U>()
                    {
                        Key = criterion.Key,
                        Branches = reducedBranches,
                        Fallback = fallback,
                    };

                    reducedCriteria.Add(newCriterion);
                }
            }

            return new DecisionTreeNode<T, U>()
            {
                Criteria = reducedCriteria.ToList(),
                Matches = matches,
            };
        }

        private class TreeBuilderContext
        {
            public TreeBuilderContext()
            {
                CurrentCriteria = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                MatchedItems = new HashSet<ItemDescriptor<T, U>>();
            }

            public TreeBuilderContext(TreeBuilderContext other)
            {
                CurrentCriteria = new HashSet<string>(other.CurrentCriteria, StringComparer.OrdinalIgnoreCase);
                MatchedItems = new HashSet<ItemDescriptor<T, U>>();
            }

            public HashSet<string> CurrentCriteria { get; private set; }

            public HashSet<ItemDescriptor<T, U>> MatchedItems { get; private set; }
        }
    }
}
