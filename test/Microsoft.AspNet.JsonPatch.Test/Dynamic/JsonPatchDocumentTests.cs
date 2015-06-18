using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.JsonPatch.Exceptions;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.JsonPatch.Test.Dynamic
{
    public class JsonPatchDocumentTests
    {

        //[Fact]
        //public void InvalidPathAtBeginningShouldThrowException()
        //{

        //    JsonPatchDocument patchDoc = new JsonPatchDocument();
        //    Assert.Throws<JsonPatchException>(() => { patchDoc.Add("//NewInt", 1); });

        //}


        //[Fact]
        //public void InvalidPathAtEndShouldThrowException()
        //{

        //    JsonPatchDocument patchDoc = new JsonPatchDocument();
        //    Assert.Throws<JsonPatchException>(() => { patchDoc.Add("NewInt//", 1); });

        //}


        //[Fact]
        //public void InvalidPathWithDotShouldThrowException()
        //{

        //    JsonPatchDocument patchDoc = new JsonPatchDocument();
        //    Assert.Throws<JsonPatchException>(() => { patchDoc.Add("NewInt.Test", 1); });

        //}



        //[Fact]
        //public void NonGenericPatchDocToGenericMustSerialize()
        //{
        //    var doc = new SimpleDTO()
        //    {
        //        StringProperty = "A",
        //        AnotherStringProperty = "B"
        //    };

        //    JsonPatchDocument patchDoc = new JsonPatchDocument();
        //    patchDoc.Copy("StringProperty", "AnotherStringProperty");

        //    var serialized = JsonConvert.SerializeObject(patchDoc);
        //    var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTO>>(serialized);

        //    deserialized.ApplyTo(doc);

        //    Assert.Equal("A", doc.AnotherStringProperty);

        //}


        //[Fact]
        //public void GenericPatchDocToNonGenericMustSerialize()
        //{
        //    var doc = new SimpleDTO()
        //    {
        //        StringProperty = "A",
        //        AnotherStringProperty = "B"
        //    };

        //    JsonPatchDocument<SimpleDTO> patchDocTyped = new JsonPatchDocument<SimpleDTO>();
        //    patchDocTyped.Copy<string>(o => o.StringProperty, o => o.AnotherStringProperty);


        //    JsonPatchDocument patchDocUntyped = new JsonPatchDocument();
        //   // patchDocUntyped.Copy("StringProperty", "AnotherStringProperty");


        //    var serializedTyped = JsonConvert.SerializeObject(patchDocTyped);
        //    var serializedUntyped = JsonConvert.SerializeObject(patchDocUntyped);
        //    var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serializedTyped);

        //    deserialized.ApplyTo(doc);

        //    Assert.Equal("A", doc.AnotherStringProperty);

        //}


    }
}
