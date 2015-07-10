// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.JsonPatch.Adapters;
using Microsoft.AspNet.JsonPatch.Converters;
using Microsoft.AspNet.JsonPatch.Exceptions;
using Microsoft.AspNet.JsonPatch.Helpers;
using Microsoft.AspNet.JsonPatch.Operations;
using Microsoft.Framework.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNet.JsonPatch
{
    [JsonConverter(typeof(JsonPatchDocumentConverter))]
    public class JsonPatchDocument : IJsonPatchDocument
    {

        public List<Operation> Operations { get; private set; }

        [JsonIgnore]
        public IContractResolver ContractResolver { get; set; }

        /// <summary>
        /// Action for logging <see cref="JsonPatchError"/>.
        /// </summary>
        public Action<JsonPatchError> LogErrorAction { get; }


        public JsonPatchDocument()
        {
            Operations = new List<Operation>();
            ContractResolver = new DefaultContractResolver();             
        }


        public JsonPatchDocument(Action<JsonPatchError> logErrorAction)
            : this()
        {
            LogErrorAction = LogErrorAction;
        }

        // Create from list of operations - logErrorAction isn't required in this case, as
        // logging errors through that specific logErrorAction happens when the PatchDocument
        // is created, not after deserialization.
        public JsonPatchDocument([NotNull] List<Operation> operations, [NotNull]  IContractResolver contractResolver)
        {
            Operations = operations;
            ContractResolver = contractResolver;
        }


        public JsonPatchDocument Add([NotNull] string path, object value)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                LogError(new JsonPatchError(
                   null,
                   null,
                   Resources.FormatInvalidValueForPath(path)));           
            }

            Operations.Add(new Operation("add", checkPathResult.AdjustedPath, null, value));
            return this;
        }

        public JsonPatchDocument Remove([NotNull] string path)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                LogError(new JsonPatchError(
                  null,
                  null,
                  Resources.FormatInvalidValueForPath(path)));
            }

            Operations.Add(new Operation("remove", checkPathResult.AdjustedPath, null, null));
            return this;
        }

        public JsonPatchDocument Replace([NotNull] string path, object value)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                LogError(new JsonPatchError(
                  null,
                  null,
                  Resources.FormatInvalidValueForPath(path)));
            }

            Operations.Add(new Operation("replace", checkPathResult.AdjustedPath, null, value));
            return this;
        }

        public JsonPatchDocument Move([NotNull] string from, [NotNull] string path)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            var checkFromResult = PathHelpers.CheckPath(from);

            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                LogError(new JsonPatchError(
                   null,
                   null,
                   Resources.FormatInvalidValueForPath(path)));
            }

            if (!checkFromResult.IsCorrectlyFormedPath)
            {
                LogError(new JsonPatchError(
                  null,
                  null,
                  Resources.FormatInvalidValueForPath(from)));
            }


            Operations.Add(new Operation("move", checkPathResult.AdjustedPath, checkFromResult.AdjustedPath));
            return this;
        }

        public JsonPatchDocument Copy([NotNull] string from, [NotNull] string path)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            var checkFromResult = PathHelpers.CheckPath(from);

            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                LogError(new JsonPatchError(
                  null,
                  null,
                  Resources.FormatInvalidValueForPath(path)));
            }

            if (!checkFromResult.IsCorrectlyFormedPath)
            {
                LogError(new JsonPatchError(
                  null,
                  null,
                  Resources.FormatInvalidValueForPath(from)));
            }

            Operations.Add(new Operation("copy", checkPathResult.AdjustedPath, checkFromResult.AdjustedPath));
            return this;
        }


        public void ApplyTo([NotNull] object objectToApplyTo)           
        {
            ApplyTo(objectToApplyTo, new ObjectAdapter(ContractResolver, logErrorAction: null));
        }

        public void ApplyTo([NotNull] object objectToApplyTo, Action<JsonPatchError> logErrorAction)
          
        {
            ApplyTo(objectToApplyTo, new ObjectAdapter(ContractResolver, logErrorAction));
        }

        public void ApplyTo([NotNull] object objectToApplyTo, [NotNull] IObjectAdapter adapter)            
        {
            // apply each operation in order
            foreach (var op in Operations)
            {
                op.Apply(objectToApplyTo, adapter);
            }
        }

        // LogError method is required on untyped JsonPatchDocument, as errors may 
        // be thrown in case of invalid paths.

        private void LogError([NotNull] JsonPatchError jsonPatchError)
        {
            if (LogErrorAction != null)
            {
                LogErrorAction(jsonPatchError);
            }
 
            // should throw error, even when logging, according to spec.
            throw new JsonPatchException(jsonPatchError);
        }



        public List<Operation> GetOperations()
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
