using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.JsonPatch.Adapters;
using Microsoft.AspNet.JsonPatch.Converters;
using Microsoft.AspNet.JsonPatch.Exceptions;
using Microsoft.AspNet.JsonPatch.Helpers;
using Microsoft.AspNet.JsonPatch.Operations;
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

        public JsonPatchDocument()
        {
            Operations = new List<Operation>();
            ContractResolver = new DefaultContractResolver();
             
        }

        // Create from list of operations  
        public JsonPatchDocument(List<Operation> operations, IContractResolver contractResolver)
        {
            Operations = operations;
            ContractResolver = contractResolver;
        }


        public JsonPatchDocument Add(string path, object value)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                throw new JsonPatchException(
                   string.Format("Provided string is not a valid path: {0}",
                          path), null);
            }

            Operations.Add(new Operation("add", checkPathResult.AdjustedPath, null, value));
            return this;
        }

        public JsonPatchDocument Remove(string path)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                throw new JsonPatchException(
                   string.Format("Provided string is not a valid path: {0}",
                          path), null);
            }

            Operations.Add(new Operation("remove", checkPathResult.AdjustedPath, null, null));
            return this;
        }

        public JsonPatchDocument Replace(string path, object value)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                throw new JsonPatchException(
                   string.Format("Provided string is not a valid path: {0}",
                          path), null);
            }

            Operations.Add(new Operation("replace", checkPathResult.AdjustedPath, null, value));
            return this;
        }

        public JsonPatchDocument Move(string from, string path)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            var checkFromResult = PathHelpers.CheckPath(from);

            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                throw new JsonPatchException(
                   string.Format("Provided string is not a valid path: {0}",
                          path), null);
            }

            if (!checkFromResult.IsCorrectlyFormedPath)
            {
                throw new JsonPatchException(
                   string.Format("Provided string is not a valid path: {0}",
                          from), null);
            }


            Operations.Add(new Operation("move", checkPathResult.AdjustedPath, checkFromResult.AdjustedPath));
            return this;
        }

        public JsonPatchDocument Copy(string from, string path)
        {
            var checkPathResult = PathHelpers.CheckPath(path);
            var checkFromResult = PathHelpers.CheckPath(from);

            if (!checkPathResult.IsCorrectlyFormedPath)
            {
                throw new JsonPatchException(
                   string.Format("Provided string is not a valid path: {0}",
                          path), null);
            }

            if (!checkFromResult.IsCorrectlyFormedPath)
            {
                throw new JsonPatchException(
                   string.Format("Provided string is not a valid path: {0}",
                          from), null);
            }

            Operations.Add(new Operation("copy", checkPathResult.AdjustedPath, checkFromResult.AdjustedPath));
            return this;
        }


        public void ApplyTo<TModel>(TModel objectToApplyTo) 
            where TModel : class
        {
            ApplyTo(objectToApplyTo, new ObjectAdapter<TModel>(ContractResolver, logErrorAction: null));
        }

        public void ApplyTo<TModel>(TModel objectToApplyTo, Action<JsonPatchError> logErrorAction)
            where TModel : class
        {
            ApplyTo(objectToApplyTo, new ObjectAdapter<TModel>(ContractResolver, logErrorAction));
        }

        public void ApplyTo<TModel>(TModel objectToApplyTo, IObjectAdapter adapter)
              where TModel : class
        {
            // apply each operation in order
            foreach (var op in Operations)
            {
              //  op.Apply(objectToApplyTo, adapter);
            }
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
