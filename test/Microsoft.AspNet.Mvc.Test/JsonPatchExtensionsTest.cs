using System;
using System.Collections.Generic;
using Microsoft.AspNet.JsonPatch;
using Microsoft.AspNet.JsonPatch.Operations;
using Microsoft.AspNet.Mvc.ModelBinding;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class JsonPatchExtensionsTest
    {
        [Fact]
        public void ApplyTo_JsonPatchDocument_ModelState()
        {
            var operation = new Operation<Customer>("add", "Customer/CustomerId", null, "TestName");
            var patchDoc = new JsonPatchDocument<Customer>();
            patchDoc.Operations.Add(operation);

            var modelState = new ModelStateDictionary();

            patchDoc.ApplyTo(new Customer(), modelState);

            Assert.Equal("Property does not exist at path 'Customer/CustomerId'.",
                modelState["JsonPatchDocument`1"].Errors[0].ErrorMessage);
        }

        [Fact]
        public void ApplyTo_JsonPatchDocument_PrefixModelState()
        {
            var operation = new Operation<Customer>("add", "Customer/CustomerId", null, "TestName");
            var patchDoc = new JsonPatchDocument<Customer>();
            patchDoc.Operations.Add(operation);

            var modelState = new ModelStateDictionary();

            patchDoc.ApplyTo(new Customer(), modelState, "jsonpatch");

            Assert.Equal("Property does not exist at path 'Customer/CustomerId'.",
                modelState["jsonpatch.JsonPatchDocument`1"].Errors[0].ErrorMessage);
        }

        public class Customer
        {
            public string CustomerName { get; set; }
        }
    }
}