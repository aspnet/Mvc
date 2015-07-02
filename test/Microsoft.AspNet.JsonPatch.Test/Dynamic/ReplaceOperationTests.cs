﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using Microsoft.AspNet.JsonPatch.Exceptions;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.JsonPatch.Test.Dynamic
{
    public class ReplaceOperationTests
    {



        [Fact]
        public void ReplaceGuidTest()
        {
            dynamic doc = new SimpleDTO()
            {
                GuidValue = Guid.NewGuid()
            };

            var newGuid = Guid.NewGuid();
            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("GuidValue", newGuid);


            // serialize & deserialize 
            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserizalized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            deserizalized.ApplyTo(doc);

            Assert.Equal(newGuid, doc.GuidValue);


        }


        [Fact]
        public void ReplaceGuidTestExpandoObject()
        {
            dynamic doc = new ExpandoObject();
            doc.GuidValue = Guid.NewGuid();


            var newGuid = Guid.NewGuid();
            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("GuidValue", newGuid);


            // serialize & deserialize 
            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserizalized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            deserizalized.ApplyTo(doc);

            Assert.Equal(newGuid, doc.GuidValue);


        }



        [Fact]
        public void ReplaceGuidTestExpandoObjectInAnonymous()
        {
            dynamic internalObject = new ExpandoObject();
            internalObject.GuidValue = Guid.NewGuid();

            dynamic doc = new
            {
                InternalObject = internalObject
            };


            var newGuid = Guid.NewGuid();
            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("internalobject/GuidValue", newGuid);


            // serialize & deserialize 
            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserizalized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            deserizalized.ApplyTo(doc);

            Assert.Equal(newGuid, doc.InternalObject.GuidValue);


        }





        [Fact]
        public void ReplaceNestedObjectTest()
        {
            dynamic doc = new ExpandoObject();
            doc.SimpleDTO = new SimpleDTO()
            {
                IntegerValue = 5,
                IntegerList = new List<int>() { 1, 2, 3 }
            };


            var newDTO = new SimpleDTO()
            {
                DoubleValue = 1
            };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("SimpleDTO", newDTO);


            // serialize & deserialize 
            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(1, doc.SimpleDTO.DoubleValue);
            Assert.Equal(0, doc.SimpleDTO.IntegerValue);
            Assert.Equal(null, doc.SimpleDTO.IntegerList);


        }




        [Fact]
        public void ReplaceInList()
        {
            dynamic doc = new ExpandoObject();
            doc.IntegerList = new List<int>() { 1, 2, 3 };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("IntegerList/0", 5);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 5, 2, 3 }, doc.IntegerList);

        }



        [Fact]
        public void ReplaceFullList()
        {
            dynamic doc = new ExpandoObject();
            doc.IntegerList = new List<int>() { 1, 2, 3 };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("IntegerList", new List<int>() { 4, 5, 6 });

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 4, 5, 6 }, doc.IntegerList);

        }


        [Fact]
        public void ReplaceInListInList()
        {
            dynamic doc = new ExpandoObject();
            doc.SimpleDTOList = new List<SimpleDTO>() {
                new SimpleDTO() {
                    IntegerList = new List<int>(){1,2,3}
                }};


            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("SimpleDTOList/0/IntegerList/0", 4);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(4, doc.SimpleDTOList[0].IntegerList[0]);
        }



        [Fact]
        public void ReplaceInListInListAtEnd()
        {
            dynamic doc = new ExpandoObject();
            doc.SimpleDTOList = new List<SimpleDTO>() {
                new SimpleDTO() {
                    IntegerList = new List<int>(){1,2,3}
                }};


            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("SimpleDTOList/0/IntegerList/-", 4);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(4, doc.SimpleDTOList[0].IntegerList[2]);
        }



        public void ReplaceInListInListInvalidPositionTooLarge()
        {
            dynamic doc = new ExpandoObject();
            doc.SimpleDTOList = new List<SimpleDTO>() {
                new SimpleDTO() {
                    IntegerList = new List<int>(){1,2,3}
                }};

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("SimpleDTOList/10/IntegerList/0", 4);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            Assert.Throws<JsonPatchException>(() => { deserialized.ApplyTo(doc); });


        }

        public void ReplaceInListInListInvalidPositionTooSmall()
        {
            dynamic doc = new ExpandoObject();
            doc.SimpleDTOList = new List<SimpleDTO>() {
                new SimpleDTO() {
                    IntegerList = new List<int>(){1,2,3}
                }};

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("SimpleDTOList/-20/IntegerList/0", 4);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);


            Assert.Throws<JsonPatchException>(() => { deserialized.ApplyTo(doc); });


        }


        [Fact]
        public void ReplaceFullListFromEnumerable()
        {
            dynamic doc = new ExpandoObject();
            doc.IntegerList = new List<int>() { 1, 2, 3 };


            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("IntegerList", new List<int>() { 4, 5, 6 });

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);
            Assert.Equal(new List<int>() { 4, 5, 6 }, doc.IntegerList);

        }

        [Fact]
        public void ReplaceFullListWithCollection()
        {
            dynamic doc = new ExpandoObject();
            doc.IntegerList = new List<int>() { 1, 2, 3 };

            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("IntegerList", new Collection<int>() { 4, 5, 6 });

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 4, 5, 6 }, doc.IntegerList);


        }




        [Fact]
        public void ReplaceAtEndOfList()
        {
            dynamic doc = new ExpandoObject();
            doc.IntegerList = new List<int>() { 1, 2, 3 };


            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("IntegerList/-", 5);
            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);
            deserialized.ApplyTo(doc);

            Assert.Equal(new List<int>() { 1, 2, 5 }, doc.IntegerList);

        }


        [Fact]
        public void ReplaceInListInvalidInvalidPositionTooLarge()
        {
            dynamic doc = new ExpandoObject();
            doc.IntegerList = new List<int>() { 1, 2, 3 };


            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("IntegerList/3", 5);
            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            Assert.Throws<JsonPatchException>(() =>
            {
                deserialized.ApplyTo(doc);
            });


        }



        [Fact]
        public void ReplaceInListInvalidPositionTooSmall()
        {

            dynamic doc = new ExpandoObject();
            doc.IntegerList = new List<int>() { 1, 2, 3 };


            // create patch
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            patchDoc.Replace("IntegerList/-1", 5);

            var serialized = JsonConvert.SerializeObject(patchDoc);
            var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument>(serialized);

            Assert.Throws<JsonPatchException>(() =>
            {
                deserialized.ApplyTo(doc);
            });


        }

    }
}
