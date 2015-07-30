// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNet.JsonPatch.Helpers
{
    internal class ObjectTreeAnalysisResult
    {
        // either the property is part of the container dictionary,
        // or we have a direct reference to a JsonPatchProperty instance

        public bool UseDynamicLogic { get; private set; }

        public bool IsValidPathForAdd { get; private set; }

        public bool IsValidPathForRemove { get; private set; }

        public IDictionary<string, object> Container { get; private set; }

        public string PropertyPathInParent { get; private set; }

        public JsonPatchProperty JsonPatchProperty { get; private set; }

        public ObjectTreeAnalysisResult(object objectToSearch, string propertyPath
            , IContractResolver contractResolver)
        {
            // construct the analysis result.

            // split the propertypath, and if necessary, remove the first 
            // empty item (that's the case when it starts with a "/")
            var propertyPathTree = propertyPath.Split(
                new char[] { '/' }, 
                StringSplitOptions.RemoveEmptyEntries).ToList();
            object targetObject = objectToSearch;
             
            // we've now got a split up property tree "base/property/otherproperty/..."
            int lastPosition = 0;
            for (int i = 0; i < propertyPathTree.Count; i++)
            {
                // if the current target object is an ExpandoObject (IDictionary<string, object>),
                // we cannot use the ContractResolver.
                lastPosition = i;
                if (targetObject is IDictionary<string, object>)
                {
                    // find the value in the dictionary                   
                    if ((targetObject as IDictionary<string, object>)                  
                        .ContainsCaseInsensitiveKey(propertyPathTree[i]))
                    {
                        var possibleNewTargetObject = (targetObject as IDictionary<string, object>)
                       .GetValueForCaseInsensitiveKey(propertyPathTree[i]);

                        // unless we're at the last item, we should set the targetobject
                        // to the new object.  If we're at the last item, we need to stop
                        if (i != propertyPathTree.Count - 1)
                        {
                            targetObject = possibleNewTargetObject;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    // if the current part of the path is numeric, this means we're trying
                    // to get the propertyInfo of a specific object in an array.  To allow
                    // for this, the previous value (targetObject) must be an IEnumerable, and
                    // the position must exist.

                    int numericValue = -1;
                    if (int.TryParse(propertyPathTree[i], out numericValue))
                    {
                        var element = GetElementAtFromObject(targetObject, numericValue);
                        if (element != null)
                        {
                            targetObject = element;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        var jsonContract = (JsonObjectContract)contractResolver
                            .ResolveContract(targetObject.GetType());

                        // does the property exist?
                        var attemptedProperty = jsonContract.Properties.FirstOrDefault
                            (p => string.Equals(p.PropertyName, propertyPathTree[i]
                                , StringComparison.OrdinalIgnoreCase));

                        if (attemptedProperty != null)
                        {
                            // unless we're at the last item, we should continue searching.
                            // If we're at the last item, we need to stop
                            if (!(i == propertyPathTree.Count - 1))
                            {
                                targetObject = attemptedProperty.ValueProvider.GetValue(targetObject);
                            }
                        }
                        else
                        {
                            // property cannot be found, and we're not working with dynamics.  
                            // Stop, and return invalid path.
                            break;
                        }
                    }
                }
            }

            // two things can happen now.  The targetproperty can be an IDictionary - in that
            // case, it's valid for add if there's 1 item left in the propertyPathTree.
            //
            // it can also be a property info.  In that case, if there's nothing left in the path
            // tree we're at the end, if there's one left we can try and set that.  

            if (targetObject is IDictionary<string, object>)
            {
                var leftOverPath = propertyPathTree
                    .GetRange(lastPosition, propertyPathTree.Count - lastPosition);

                UseDynamicLogic = true;

                if (leftOverPath.Count == 1)
                {
                    Container = targetObject as IDictionary<string, object>;
                    IsValidPathForAdd = true;
                    PropertyPathInParent = leftOverPath.Last();

                    // to be able to remove this property, it must exist
                    IsValidPathForRemove = Container.ContainsCaseInsensitiveKey(PropertyPathInParent);
                }
                else
                {
                    IsValidPathForAdd = false;
                    IsValidPathForRemove = false;
                }
            }
            else
            {
                var leftOverPath = propertyPathTree
                    .GetRange(lastPosition, propertyPathTree.Count - lastPosition);

                UseDynamicLogic = false;

                if (leftOverPath.Count == 1)
                {                
                        var jsonContract = (JsonObjectContract)contractResolver.ResolveContract(targetObject.GetType());
                        var attemptedProperty = jsonContract.Properties.FirstOrDefault
                      (p => string.Equals(p.PropertyName, leftOverPath.Last()
                          , StringComparison.OrdinalIgnoreCase));                    

                        if (attemptedProperty == null)
                        {
                            IsValidPathForAdd = false;
                            IsValidPathForRemove = false;
                        }
                        else
                        {
                            IsValidPathForAdd = true;
                            IsValidPathForRemove = true;
                            JsonPatchProperty = new JsonPatchProperty(attemptedProperty, targetObject);
                            PropertyPathInParent = leftOverPath.Last();
                        }
                }
                else
                {
                    IsValidPathForAdd = false;
                    IsValidPathForRemove = false;
                }
            }
        }

        private object GetElementAtFromObject(object targetObject, int numericValue)
        {
            if (numericValue > -1)
            {
                // Check if the targetobject is an IEnumerable,
                // and if the position is valid.
                if (targetObject is IEnumerable)
                {
                    var indexable = ((IEnumerable)targetObject).Cast<object>();

                    if (indexable.Count() >= numericValue)
                    {
                        return indexable.ElementAt(numericValue);
                    }
                    else { return null; }
                }
                else { return null; }
            }
            else
            {
                return null;
            }
        }

    }
}
