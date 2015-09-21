// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNet.JsonPatch.Adapters;
using Microsoft.AspNet.JsonPatch.Converters;
using Microsoft.AspNet.JsonPatch.Helpers;
using Microsoft.AspNet.JsonPatch.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNet.JsonPatch
{
    // Implementation details: the purpose of this type of patch document is to ensure we can do type-checking
    // when producing a JsonPatchDocument.  However, we cannot send this "typed" over the wire, as that would require
    // including type data in the JsonPatchDocument serialized as JSON (to allow for correct deserialization) - that's
    // not according to RFC 6902, and would thus break cross-platform compatibility.
    [JsonConverter(typeof(TypedJsonPatchDocumentConverter))]
    public class JsonPatchDocument<TModel> : IJsonPatchDocument where TModel : class
    {
        public List<Operation<TModel>> Operations { get; private set; }

        [JsonIgnore]
        public IContractResolver ContractResolver { get; set; }

        public JsonPatchDocument()
        {
            Operations = new List<Operation<TModel>>();
            ContractResolver = new DefaultContractResolver();
        }

        // Create from list of operations
        public JsonPatchDocument(List<Operation<TModel>> operations, IContractResolver contractResolver)
        {
            if (operations == null)
            {
                throw new ArgumentNullException(nameof(operations));
            }

            if (contractResolver == null)
            {
                throw new ArgumentNullException(nameof(contractResolver));
            }

            Operations = operations;
            ContractResolver = contractResolver;
        }

        /// <summary>
        /// Add operation.  Will result in, for example,
        /// { "op": "add", "path": "/a/b/c", "value": [ "foo", "bar" ] }
        /// </summary>
        /// <typeparam name="TProp">value type</typeparam>
        /// <param name="path">target location</param>
        /// <param name="value">value</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Add<TProp>(Expression<Func<TModel, TProp>> path, TProp value)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "add",
                ExpressionHelpers.GetPath(path).ToLowerInvariant(),
                from: null,
                value: value));

            return this;
        }

        /// <summary>
        /// Add value to list at given position
        /// </summary>
        /// <typeparam name="TProp">value type</typeparam>
        /// <param name="path">target location</param>
        /// <param name="value">value</param>
        /// <param name="position">position</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Add<TProp>(
            Expression<Func<TModel, IList<TProp>>> path,
            TProp value,
            int position)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "add",
                ExpressionHelpers.GetPath(path).ToLowerInvariant() + "/" + position,
                from: null,
                value: value));

            return this;
        }

        /// <summary>
        /// At value at end of list
        /// </summary>
        /// <typeparam name="TProp">value type</typeparam>
        /// <param name="path">target location</param>
        /// <param name="value">value</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Add<TProp>(Expression<Func<TModel, IList<TProp>>> path, TProp value)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "add",
                ExpressionHelpers.GetPath(path).ToLowerInvariant() + "/-",
                from: null,
                value: value));

            return this;
        }

        /// <summary>
        /// Remove value at target location.  Will result in, for example,
        /// { "op": "remove", "path": "/a/b/c" }
        /// </summary>
        /// <param name="path">target location</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Remove<TProp>(Expression<Func<TModel, TProp>> path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>("remove", ExpressionHelpers.GetPath(path).ToLowerInvariant(), from: null));

            return this;
        }

        /// <summary>
        /// Remove value from list at given position
        /// </summary>
        /// <typeparam name="TProp">value type</typeparam>
        /// <param name="path">target location</param>
        /// <param name="position">position</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Remove<TProp>(Expression<Func<TModel, IList<TProp>>> path, int position)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "remove",
                ExpressionHelpers.GetPath(path).ToLowerInvariant() + "/" + position,
                from: null));

            return this;
        }

        /// <summary>
        /// Remove value from end of list
        /// </summary>
        /// <typeparam name="TProp">value type</typeparam>
        /// <param name="path">target location</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Remove<TProp>(Expression<Func<TModel, IList<TProp>>> path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "remove",
                ExpressionHelpers.GetPath(path).ToLowerInvariant() + "/-",
                from: null));

            return this;
        }

        /// <summary>
        /// Replace value.  Will result in, for example,
        /// { "op": "replace", "path": "/a/b/c", "value": 42 }
        /// </summary>
        /// <param name="path">target location</param>
        /// <param name="value">value</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Replace<TProp>(Expression<Func<TModel, TProp>> path, TProp value)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "replace",
                ExpressionHelpers.GetPath(path).ToLowerInvariant(),
                from: null,
                value: value));

            return this;
        }

        /// <summary>
        /// Replace value in a list at given position
        /// </summary>
        /// <typeparam name="TProp">value type</typeparam>
        /// <param name="path">target location</param>
        /// <param name="value">value</param>
        /// <param name="position">position</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Replace<TProp>(Expression<Func<TModel, IList<TProp>>> path,
            TProp value, int position)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "replace",
                ExpressionHelpers.GetPath(path).ToLowerInvariant() + "/" + position,
                from: null,
                value: value));

            return this;
        }

        /// <summary>
        /// Replace value at end of a list
        /// </summary>
        /// <typeparam name="TProp">value type</typeparam>
        /// <param name="path">target location</param>
        /// <param name="value">value</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Replace<TProp>(Expression<Func<TModel, IList<TProp>>> path, TProp value)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "replace",
                ExpressionHelpers.GetPath(path).ToLowerInvariant() + "/-",
                from: null,
                value: value));

            return this;
        }

        /// <summary>
        /// Removes value at specified location and add it to the target location.  Will result in, for example:
        /// { "op": "move", "from": "/a/b/c", "path": "/a/b/d" }
        /// </summary>
        /// <param name="from">source location</param>
        /// <param name="path">target location</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Move<TProp>(
            Expression<Func<TModel, TProp>> from,
            Expression<Func<TModel, TProp>> path)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "move",
                ExpressionHelpers.GetPath(path).ToLowerInvariant(),
                ExpressionHelpers.GetPath(from).ToLowerInvariant()));

            return this;
        }

        /// <summary>
        /// Move from a position in a list to a new location
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="from">source location</param>
        /// <param name="positionFrom">position</param>
        /// <param name="path">target location</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Move<TProp>(
            Expression<Func<TModel, IList<TProp>>> from,
            int positionFrom,
            Expression<Func<TModel, TProp>> path)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "move",
                ExpressionHelpers.GetPath(path).ToLowerInvariant(),
                ExpressionHelpers.GetPath(from).ToLowerInvariant() + "/" + positionFrom));

            return this;
        }

        /// <summary>
        /// Move from a property to a location in a list
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="from">source location</param>
        /// <param name="path">target location</param>
        /// <param name="positionTo">position</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Move<TProp>(
            Expression<Func<TModel, TProp>> from,
            Expression<Func<TModel, IList<TProp>>> path,
            int positionTo)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "move",
                ExpressionHelpers.GetPath(path).ToLowerInvariant() + "/" + positionTo,
                ExpressionHelpers.GetPath(from).ToLowerInvariant()));

            return this;
        }

        /// <summary>
        /// Move from a position in a list to another location in a list
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="from">source location</param>
        /// <param name="positionFrom">position (source)</param>
        /// <param name="path">target location</param>
        /// <param name="positionTo">position (target)</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Move<TProp>(
            Expression<Func<TModel, IList<TProp>>> from,
            int positionFrom,
            Expression<Func<TModel, IList<TProp>>> path,
            int positionTo)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "move",
                ExpressionHelpers.GetPath(path).ToLowerInvariant() + "/" + positionTo,
                ExpressionHelpers.GetPath(from).ToLowerInvariant() + "/" + positionFrom));

            return this;
        }

        /// <summary>
        /// Move from a position in a list to the end of another list
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="from">source location</param>
        /// <param name="positionFrom">position</param>
        /// <param name="path">target location</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Move<TProp>(
            Expression<Func<TModel, IList<TProp>>> from,
            int positionFrom,
            Expression<Func<TModel, IList<TProp>>> path)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "move",
                ExpressionHelpers.GetPath(path).ToLowerInvariant() + "/-",
                ExpressionHelpers.GetPath(from).ToLowerInvariant() + "/" + positionFrom));

            return this;
        }

        /// <summary>
        /// Move to the end of a list
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="from">source location</param>
        /// <param name="path">target location</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Move<TProp>(
           Expression<Func<TModel, TProp>> from,
           Expression<Func<TModel, IList<TProp>>> path)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "move",
                ExpressionHelpers.GetPath(path).ToLowerInvariant() + "/-",
                ExpressionHelpers.GetPath(from).ToLowerInvariant()));

            return this;
        }

        /// <summary>
        /// Copy the value at specified location to the target location.  Willr esult in, for example:
        /// { "op": "copy", "from": "/a/b/c", "path": "/a/b/e" }
        /// </summary>
        /// <param name="from">source location</param>
        /// <param name="path">target location</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Copy<TProp>(
           Expression<Func<TModel, TProp>> from,
           Expression<Func<TModel, TProp>> path)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "copy",
                ExpressionHelpers.GetPath(path).ToLowerInvariant()
              , ExpressionHelpers.GetPath(from).ToLowerInvariant()));

            return this;
        }

        /// <summary>
        /// Copy from a position in a list to a new location
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="from">source location</param>
        /// <param name="positionFrom">position</param>
        /// <param name="path">target location</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Copy<TProp>(
           Expression<Func<TModel, IList<TProp>>> from,
            int positionFrom,
           Expression<Func<TModel, TProp>> path)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "copy",
                ExpressionHelpers.GetPath(path).ToLowerInvariant(),
                ExpressionHelpers.GetPath(from).ToLowerInvariant() + "/" + positionFrom));

            return this;
        }

        /// <summary>
        /// Copy from a property to a location in a list
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="from">source location</param>      
        /// <param name="path">target location</param>
        /// <param name="positionTo">position</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Copy<TProp>(
            Expression<Func<TModel, TProp>> from,
            Expression<Func<TModel, IList<TProp>>> path,
            int positionTo)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "copy",
                ExpressionHelpers.GetPath(path).ToLowerInvariant() + "/" + positionTo,
                ExpressionHelpers.GetPath(from).ToLowerInvariant()));

            return this;
        }

        /// <summary>
        /// Copy from a position in a list to a new location in a list
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="from">source location</param>
        /// <param name="positionFrom">position (source)</param>
        /// <param name="path">target location</param>
        /// <param name="positionTo">position (target)</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Copy<TProp>(
            Expression<Func<TModel, IList<TProp>>> from,
            int positionFrom,
            Expression<Func<TModel, IList<TProp>>> path,
            int positionTo)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "copy",
                ExpressionHelpers.GetPath(path).ToLowerInvariant() + "/" + positionTo,
                ExpressionHelpers.GetPath(from).ToLowerInvariant() + "/" + positionFrom));

            return this;
        }

        /// <summary>
        /// Copy from a position in a list to the end of another list
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="from">source location</param>
        /// <param name="positionFrom">position</param>
        /// <param name="path">target location</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Copy<TProp>(
            Expression<Func<TModel, IList<TProp>>> from,
            int positionFrom,
            Expression<Func<TModel, IList<TProp>>> path)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "copy",
                ExpressionHelpers.GetPath(path).ToLowerInvariant() + "/-",
                ExpressionHelpers.GetPath(from).ToLowerInvariant() + "/" + positionFrom));

            return this;
        }

        /// <summary>
        /// Copy to the end of a list
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="from">source location</param>
        /// <param name="path">target location</param>
        /// <returns></returns>
        public JsonPatchDocument<TModel> Copy<TProp>(
            Expression<Func<TModel, TProp>> from,
            Expression<Func<TModel, IList<TProp>>> path)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Operations.Add(new Operation<TModel>(
                "copy",
                ExpressionHelpers.GetPath(path).ToLowerInvariant() + "/-",
                ExpressionHelpers.GetPath(from).ToLowerInvariant()));

            return this;
        }

        /// <summary>
        /// Apply this JsonPatchDocument 
        /// </summary>
        /// <param name="objectToApplyTo">Object to apply the JsonPatchDocument to</param>
        public void ApplyTo(TModel objectToApplyTo)
        {
            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            ApplyTo(objectToApplyTo, new ObjectAdapter(ContractResolver, logErrorAction: null));
        }

        /// <summary>
        /// Apply this JsonPatchDocument 
        /// </summary>
        /// <param name="objectToApplyTo">Object to apply the JsonPatchDocument to</param>
        /// <param name="logErrorAction">Action to log errors</param>
        public void ApplyTo(TModel objectToApplyTo, Action<JsonPatchError> logErrorAction)
        {
            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            ApplyTo(objectToApplyTo, new ObjectAdapter(ContractResolver, logErrorAction));
        }

        /// <summary>
        /// Apply this JsonPatchDocument  
        /// </summary>
        /// <param name="objectToApplyTo">Object to apply the JsonPatchDocument to</param>
        /// <param name="adapter">IObjectAdapter instance to use when applying</param>
        public void ApplyTo(TModel objectToApplyTo, IObjectAdapter adapter)
        {
            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            if (adapter == null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            // apply each operation in order
            foreach (var op in Operations)
            {
                op.Apply(objectToApplyTo, adapter);
            }
        }

        IList<Operation> IJsonPatchDocument.GetOperations()
        {
            var allOps = new List<Operation>();

            if (Operations != null)
            {
                foreach (var op in Operations)
                {
                    var untypedOp = new Operation();

                    untypedOp.op = op.op;
                    untypedOp.value = op.value;
                    untypedOp.path = op.path;
                    untypedOp.from = op.from;

                    allOps.Add(untypedOp);
                }
            }

            return allOps;
        }
    }
}