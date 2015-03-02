using Microsoft.AspNet.JsonPatch.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace Microsoft.AspNet.JsonPatch.Test
{
	public class NestedObjectTests
	{

		[Fact]
		public void ReplacePropertyInNestedObject()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				IntegerValue = 1

			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<string>(o => o.NestedDTO.StringProperty, "B");

			patchDoc.ApplyTo(doc);

			Assert.Equal("B", doc.NestedDTO.StringProperty);

		}


		[Fact]
		public void ReplacePropertyInNestedObjectWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				IntegerValue = 1

			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<string>(o => o.NestedDTO.StringProperty, "B");


			// serialize & deserialize 
			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);


			deserialized.ApplyTo(doc);

			Assert.Equal("B", doc.NestedDTO.StringProperty);

		}


		[Fact]
		public void ReplaceNestedObject()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				IntegerValue = 1

			};

			var newNested = new NestedDTO() { StringProperty = "B" };

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<NestedDTO>(o => o.NestedDTO, newNested);

			patchDoc.ApplyTo(doc);

			Assert.Equal("B", doc.NestedDTO.StringProperty);

		}


		[Fact]
		public void ReplaceNestedObjectWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				IntegerValue = 1

			};

			var newNested = new NestedDTO() { StringProperty = "B" };

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<NestedDTO>(o => o.NestedDTO, newNested);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);



			deserialized.ApplyTo(doc);

			Assert.Equal("B", doc.NestedDTO.StringProperty);

		}


		[Fact]
		public void AddResultsInReplace()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					StringProperty = "A"
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Add<string>(o => o.SimpleDTO.StringProperty, "B");

			patchDoc.ApplyTo(doc);

			Assert.Equal("B", doc.SimpleDTO.StringProperty);

		}

		[Fact]
		public void AddResultsInReplaceWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					StringProperty = "A"
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Add<string>(o => o.SimpleDTO.StringProperty, "B");

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);


			deserialized.ApplyTo(doc);

			Assert.Equal("B", doc.SimpleDTO.StringProperty);

		}


		[Fact]
		public void AddToList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};


			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Add<int>(o => o.SimpleDTO.IntegerList, 4, 0);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 4, 1, 2, 3 }, doc.SimpleDTO.IntegerList);
		}


		[Fact]
		public void AddToListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};


			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Add<int>(o => o.SimpleDTO.IntegerList, 4, 0);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);


			deserialized.ApplyTo(doc);

			Assert.Equal(new List<int>() { 4, 1, 2, 3 }, doc.SimpleDTO.IntegerList);
		}



		[Fact]
		public void AddToListInvalidPositionTooLarge()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			}
			;

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Add<int>(o => o.SimpleDTO.IntegerList, 4, 3);

			Assert.Throws<JsonPatchException<SimpleDTOWithNestedDTO>>(() => { patchDoc.ApplyTo(doc); });

		}


		[Fact]
		public void AddToListInvalidPositionTooLargeWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			}
			;

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Add<int>(o => o.SimpleDTO.IntegerList, 4, 3);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);



			Assert.Throws<JsonPatchException<SimpleDTOWithNestedDTO>>(() => { deserialized.ApplyTo(doc); });

		}


		[Fact]
		public void AddToListInvalidPositionTooSmall()
		{

			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};


			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Add<int>(o => o.SimpleDTO.IntegerList, 4, -1);

			Assert.Throws<JsonPatchException<SimpleDTOWithNestedDTO>>(() => { patchDoc.ApplyTo(doc); });

		}

		[Fact]
		public void AddToListInvalidPositionTooSmallWithSerialization()
		{

			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};


			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Add<int>(o => o.SimpleDTO.IntegerList, 4, -1);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);



			Assert.Throws<JsonPatchException<SimpleDTOWithNestedDTO>>(() => { deserialized.ApplyTo(doc); });

		}

		[Fact]
		public void AddToListAppend()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};


			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Add<int>(o => o.SimpleDTO.IntegerList, 4);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 2, 3, 4 }, doc.SimpleDTO.IntegerList);

		}

		[Fact]
		public void AddToListAppendWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};


			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Add<int>(o => o.SimpleDTO.IntegerList, 4);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);



			deserialized.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 2, 3, 4 }, doc.SimpleDTO.IntegerList);

		}


		[Fact]
		public void Remove()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					StringProperty = "A"
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Remove<string>(o => o.SimpleDTO.StringProperty);

			patchDoc.ApplyTo(doc);

			Assert.Equal(null, doc.SimpleDTO.StringProperty);

		}



		[Fact]
		public void RemoveWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					StringProperty = "A"
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Remove<string>(o => o.SimpleDTO.StringProperty);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);


			deserialized.ApplyTo(doc);

			Assert.Equal(null, doc.SimpleDTO.StringProperty);

		}



		[Fact]
		public void RemoveFromList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};


			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Remove<int>(o => o.SimpleDTO.IntegerList, 2);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 2 }, doc.SimpleDTO.IntegerList);
		}

		[Fact]
		public void RemoveFromListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};


			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Remove<int>(o => o.SimpleDTO.IntegerList, 2);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);

			deserialized.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 2 }, doc.SimpleDTO.IntegerList);
		}


		[Fact]
		public void RemoveFromListInvalidPositionTooLarge()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Remove<int>(o => o.SimpleDTO.IntegerList, 3);

			Assert.Throws<JsonPatchException<SimpleDTOWithNestedDTO>>(() => { patchDoc.ApplyTo(doc); });

		}

		[Fact]
		public void RemoveFromListInvalidPositionTooLargeWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Remove<int>(o => o.SimpleDTO.IntegerList, 3);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);

			Assert.Throws<JsonPatchException<SimpleDTOWithNestedDTO>>(() => { deserialized.ApplyTo(doc); });

		}



		[Fact]
		public void RemoveFromListInvalidPositionTooSmall()
		{

			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			}
			  ;

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Remove<int>(o => o.SimpleDTO.IntegerList, -1);


			Assert.Throws<JsonPatchException<SimpleDTOWithNestedDTO>>(() => { patchDoc.ApplyTo(doc); });

		}

		[Fact]
		public void RemoveFromListInvalidPositionTooSmallWithSerialization()
		{

			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			}
			  ;

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Remove<int>(o => o.SimpleDTO.IntegerList, -1);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);


			Assert.Throws<JsonPatchException<SimpleDTOWithNestedDTO>>(() => { deserialized.ApplyTo(doc); });

		}


		[Fact]
		public void RemoveFromEndOfList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Remove<int>(o => o.SimpleDTO.IntegerList);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 2 }, doc.SimpleDTO.IntegerList);

		}

		[Fact]
		public void RemoveFromEndOfListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Remove<int>(o => o.SimpleDTO.IntegerList);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);

			deserialized.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 2 }, doc.SimpleDTO.IntegerList);

		}


		[Fact]
		public void Replace()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					StringProperty = "A",
					DecimalValue = 10
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<string>(o => o.SimpleDTO.StringProperty, "B");
			patchDoc.Replace(o => o.SimpleDTO.DecimalValue, 12);

			patchDoc.ApplyTo(doc);

			Assert.Equal("B", doc.SimpleDTO.StringProperty);
			Assert.Equal(12, doc.SimpleDTO.DecimalValue);



		}


		[Fact]
		public void ReplaceWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					StringProperty = "A",
					DecimalValue = 10
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<string>(o => o.SimpleDTO.StringProperty, "B");
			patchDoc.Replace(o => o.SimpleDTO.DecimalValue, 12);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);

			deserialized.ApplyTo(doc);

			Assert.Equal("B", doc.SimpleDTO.StringProperty);
			Assert.Equal(12, doc.SimpleDTO.DecimalValue);



		}





		[Fact]
		public void SerializationTests()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					StringProperty = "A",
					DecimalValue = 10,
					DoubleValue = 10,
					FloatValue = 10,
					IntegerValue = 10
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace(o => o.SimpleDTO.StringProperty, "B");
			patchDoc.Replace(o => o.SimpleDTO.DecimalValue, 12);
			patchDoc.Replace(o => o.SimpleDTO.DoubleValue, 12);
			patchDoc.Replace(o => o.SimpleDTO.FloatValue, 12);
			patchDoc.Replace(o => o.SimpleDTO.IntegerValue, 12);



			// serialize & deserialize 
			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserizalized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);


			deserizalized.ApplyTo(doc);

			Assert.Equal("B", doc.SimpleDTO.StringProperty);
			Assert.Equal(12, doc.SimpleDTO.DecimalValue);
			Assert.Equal(12, doc.SimpleDTO.DoubleValue);
			Assert.Equal(12, doc.SimpleDTO.FloatValue);
			Assert.Equal(12, doc.SimpleDTO.IntegerValue);

		}

		[Fact]
		public void ReplaceInList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<int>(o => o.SimpleDTO.IntegerList, 5, 0);

			patchDoc.ApplyTo(doc);


			Assert.Equal(new List<int>() { 5, 2, 3 }, doc.SimpleDTO.IntegerList);

		}

		[Fact]
		public void ReplaceInListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<int>(o => o.SimpleDTO.IntegerList, 5, 0);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);
			deserialized.ApplyTo(doc);


			Assert.Equal(new List<int>() { 5, 2, 3 }, doc.SimpleDTO.IntegerList);

		}



		[Fact]
		public void ReplaceFullList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<List<int>>(o => o.SimpleDTO.IntegerList, new List<int>() { 4, 5, 6 });

			patchDoc.ApplyTo(doc);


			Assert.Equal(new List<int>() { 4, 5, 6 }, doc.SimpleDTO.IntegerList);

		}


		[Fact]
		public void ReplaceFullListWithSerialiation()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<List<int>>(o => o.SimpleDTO.IntegerList, new List<int>() { 4, 5, 6 });

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);

			deserialized.ApplyTo(doc);


			Assert.Equal(new List<int>() { 4, 5, 6 }, doc.SimpleDTO.IntegerList);

		}



		[Fact]
		public void ReplaceFullListFromEnumerable()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<IEnumerable<int>>(o => o.SimpleDTO.IntegerList, new List<int>() { 4, 5, 6 });

			patchDoc.ApplyTo(doc);


			Assert.Equal(new List<int>() { 4, 5, 6 }, doc.SimpleDTO.IntegerList);

		}

		[Fact]
		public void ReplaceFullListFromEnumerableWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<IEnumerable<int>>(o => o.SimpleDTO.IntegerList, new List<int>() { 4, 5, 6 });

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);

			deserialized.ApplyTo(doc);


			Assert.Equal(new List<int>() { 4, 5, 6 }, doc.SimpleDTO.IntegerList);

		}



		[Fact]
		public void ReplaceFullListWithCollection()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<IEnumerable<int>>(o => o.SimpleDTO.IntegerList, new Collection<int>() { 4, 5, 6 });

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 4, 5, 6 }, doc.SimpleDTO.IntegerList);


		}

		[Fact]
		public void ReplaceFullListWithCollectionWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<IEnumerable<int>>(o => o.SimpleDTO.IntegerList, new Collection<int>() { 4, 5, 6 });

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);
			deserialized.ApplyTo(doc);

			Assert.Equal(new List<int>() { 4, 5, 6 }, doc.SimpleDTO.IntegerList);



		}




		[Fact]
		public void ReplaceAtEndOfList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<int>(o => o.SimpleDTO.IntegerList, 5);

			patchDoc.ApplyTo(doc);


			Assert.Equal(new List<int>() { 1, 2, 5 }, doc.SimpleDTO.IntegerList);

		}


		[Fact]
		public void ReplaceAtEndOfListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<int>(o => o.SimpleDTO.IntegerList, 5);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);
			deserialized.ApplyTo(doc);


			Assert.Equal(new List<int>() { 1, 2, 5 }, doc.SimpleDTO.IntegerList);

		}


		[Fact]
		public void ReplaceInListInvalidInvalidPositionTooLarge()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<int>(o => o.SimpleDTO.IntegerList, 5, 3);

			Assert.Throws<JsonPatchException<SimpleDTOWithNestedDTO>>(() => { patchDoc.ApplyTo(doc); });
		}

		[Fact]
		public void ReplaceInListInvalidInvalidPositionTooLargeWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<int>(o => o.SimpleDTO.IntegerList, 5, 3);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);

			Assert.Throws<JsonPatchException<SimpleDTOWithNestedDTO>>(() => { deserialized.ApplyTo(doc); });
		}


		[Fact]
		public void ReplaceInListInvalidPositionTooSmall()
		{


			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<int>(o => o.SimpleDTO.IntegerList, 5, -1);

			Assert.Throws<JsonPatchException<SimpleDTOWithNestedDTO>>(() => { patchDoc.ApplyTo(doc); });

		}

		[Fact]
		public void ReplaceInListInvalidPositionTooSmallWithSerialization()
		{


			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Replace<int>(o => o.SimpleDTO.IntegerList, 5, -1);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);

			Assert.Throws<JsonPatchException<SimpleDTOWithNestedDTO>>(() => { deserialized.ApplyTo(doc); });

		}





		[Fact]
		public void Copy()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					StringProperty = "A",
					AnotherStringProperty = "B"
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Copy<string>(o => o.SimpleDTO.StringProperty, o => o.SimpleDTO.AnotherStringProperty);

			patchDoc.ApplyTo(doc);

			Assert.Equal("A", doc.SimpleDTO.AnotherStringProperty);

		}

		[Fact]
		public void CopyWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					StringProperty = "A",
					AnotherStringProperty = "B"
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Copy<string>(o => o.SimpleDTO.StringProperty, o => o.SimpleDTO.AnotherStringProperty);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);

			deserialized.ApplyTo(doc);

			Assert.Equal("A", doc.SimpleDTO.AnotherStringProperty);

		}



		[Fact]
		public void CopyInList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Copy<int>(o => o.SimpleDTO.IntegerList, 0, o => o.SimpleDTO.IntegerList, 1);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 1, 2, 3 }, doc.SimpleDTO.IntegerList);
		}

		[Fact]
		public void CopyInListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Copy<int>(o => o.SimpleDTO.IntegerList, 0, o => o.SimpleDTO.IntegerList, 1);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);
			deserialized.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 1, 2, 3 }, doc.SimpleDTO.IntegerList);
		}


		[Fact]
		public void CopyFromListToEndOfList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Copy<int>(o => o.SimpleDTO.IntegerList, 0, o => o.SimpleDTO.IntegerList);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 2, 3, 1 }, doc.SimpleDTO.IntegerList);
		}


		[Fact]
		public void CopyFromListToEndOfListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Copy<int>(o => o.SimpleDTO.IntegerList, 0, o => o.SimpleDTO.IntegerList);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);

			deserialized.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 2, 3, 1 }, doc.SimpleDTO.IntegerList);
		}




		[Fact]
		public void CopyFromListToNonList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Copy<int>(o => o.SimpleDTO.IntegerList, 0, o => o.SimpleDTO.IntegerValue);

			patchDoc.ApplyTo(doc);

			Assert.Equal(1, doc.SimpleDTO.IntegerValue);
		}

		[Fact]
		public void CopyFromListToNonListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Copy<int>(o => o.SimpleDTO.IntegerList, 0, o => o.SimpleDTO.IntegerValue);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);
			deserialized.ApplyTo(doc);

			Assert.Equal(1, doc.SimpleDTO.IntegerValue);
		}


		[Fact]
		public void CopyFromNonListToList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerValue = 5,
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Copy<int>(o => o.SimpleDTO.IntegerValue, o => o.SimpleDTO.IntegerList, 0);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 5, 1, 2, 3 }, doc.SimpleDTO.IntegerList);
		}


		[Fact]
		public void CopyFromNonListToListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerValue = 5,
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Copy<int>(o => o.SimpleDTO.IntegerValue, o => o.SimpleDTO.IntegerList, 0);
			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);
			deserialized.ApplyTo(doc);

			Assert.Equal(new List<int>() { 5, 1, 2, 3 }, doc.SimpleDTO.IntegerList);
		}




		[Fact]
		public void CopyToEndOfList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerValue = 5,
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Copy<int>(o => o.SimpleDTO.IntegerValue, o => o.SimpleDTO.IntegerList);

			patchDoc.ApplyTo(doc);


			Assert.Equal(new List<int>() { 1, 2, 3, 5 }, doc.SimpleDTO.IntegerList);

		}



		[Fact]
		public void CopyToEndOfListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerValue = 5,
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Copy<int>(o => o.SimpleDTO.IntegerValue, o => o.SimpleDTO.IntegerList);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);
			deserialized.ApplyTo(doc);


			Assert.Equal(new List<int>() { 1, 2, 3, 5 }, doc.SimpleDTO.IntegerList);

		}


		[Fact]
		public void Move()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					StringProperty = "A",
					AnotherStringProperty = "B"
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Move<string>(o => o.SimpleDTO.StringProperty, o => o.SimpleDTO.AnotherStringProperty);

			patchDoc.ApplyTo(doc);

			Assert.Equal("A", doc.SimpleDTO.AnotherStringProperty);
			Assert.Equal(null, doc.SimpleDTO.StringProperty);
		}


		[Fact]
		public void MoveWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					StringProperty = "A",
					AnotherStringProperty = "B"
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Move<string>(o => o.SimpleDTO.StringProperty, o => o.SimpleDTO.AnotherStringProperty);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);
			deserialized.ApplyTo(doc);

			Assert.Equal("A", doc.SimpleDTO.AnotherStringProperty);
			Assert.Equal(null, doc.SimpleDTO.StringProperty);
		}





		[Fact]
		public void MoveInList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Move<int>(o => o.SimpleDTO.IntegerList, 0, o => o.SimpleDTO.IntegerList, 1);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 2, 1, 3 }, doc.SimpleDTO.IntegerList);
		}


		[Fact]
		public void MoveInListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Move<int>(o => o.SimpleDTO.IntegerList, 0, o => o.SimpleDTO.IntegerList, 1);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);
			deserialized.ApplyTo(doc);

			Assert.Equal(new List<int>() { 2, 1, 3 }, doc.SimpleDTO.IntegerList);
		}


		[Fact]
		public void MoveFromListToEndOfList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Move<int>(o => o.SimpleDTO.IntegerList, 0, o => o.SimpleDTO.IntegerList);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 2, 3, 1 }, doc.SimpleDTO.IntegerList);
		}


		[Fact]
		public void MoveFromListToEndOfListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Move<int>(o => o.SimpleDTO.IntegerList, 0, o => o.SimpleDTO.IntegerList);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);
			deserialized.ApplyTo(doc);

			Assert.Equal(new List<int>() { 2, 3, 1 }, doc.SimpleDTO.IntegerList);
		}



		[Fact]
		public void MoveFomListToNonList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Move<int>(o => o.SimpleDTO.IntegerList, 0, o => o.SimpleDTO.IntegerValue);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 2, 3 }, doc.SimpleDTO.IntegerList);
			Assert.Equal(1, doc.SimpleDTO.IntegerValue);
		}



		[Fact]
		public void MoveFomListToNonListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Move<int>(o => o.SimpleDTO.IntegerList, 0, o => o.SimpleDTO.IntegerValue);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);
			deserialized.ApplyTo(doc);

			Assert.Equal(new List<int>() { 2, 3 }, doc.SimpleDTO.IntegerList);
			Assert.Equal(1, doc.SimpleDTO.IntegerValue);
		}


		[Fact]
		public void MoveFomListToNonListBetweenHierarchy()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Move<int>(o => o.SimpleDTO.IntegerList, 0, o => o.IntegerValue);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 2, 3 }, doc.SimpleDTO.IntegerList);
			Assert.Equal(1, doc.IntegerValue);
		}

		[Fact]
		public void MoveFomListToNonListBetweenHierarchyWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Move<int>(o => o.SimpleDTO.IntegerList, 0, o => o.IntegerValue);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);
			deserialized.ApplyTo(doc);

			Assert.Equal(new List<int>() { 2, 3 }, doc.SimpleDTO.IntegerList);
			Assert.Equal(1, doc.IntegerValue);
		}


		[Fact]
		public void MoveFromNonListToList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerValue = 5,
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Move<int>(o => o.SimpleDTO.IntegerValue, o => o.SimpleDTO.IntegerList, 0);

			patchDoc.ApplyTo(doc);

			Assert.Equal(0, doc.IntegerValue);
			Assert.Equal(new List<int>() { 5, 1, 2, 3 }, doc.SimpleDTO.IntegerList);
		}


		[Fact]
		public void MoveFromNonListToListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerValue = 5,
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Move<int>(o => o.SimpleDTO.IntegerValue, o => o.SimpleDTO.IntegerList, 0);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);
			deserialized.ApplyTo(doc);

			Assert.Equal(0, doc.IntegerValue);
			Assert.Equal(new List<int>() { 5, 1, 2, 3 }, doc.SimpleDTO.IntegerList);
		}





		[Fact]
		public void MoveToEndOfList()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerValue = 5,
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Move<int>(o => o.SimpleDTO.IntegerValue, o => o.SimpleDTO.IntegerList);

			patchDoc.ApplyTo(doc);

			Assert.Equal(0, doc.IntegerValue);

			Assert.Equal(new List<int>() { 1, 2, 3, 5 }, doc.SimpleDTO.IntegerList);

		}



		[Fact]
		public void MoveToEndOfListWithSerialization()
		{
			var doc = new SimpleDTOWithNestedDTO()
			{
				SimpleDTO = new SimpleDTO()
				{
					IntegerValue = 5,
					IntegerList = new List<int>() { 1, 2, 3 }
				}
			};

			// create patch
			JsonPatchDocument<SimpleDTOWithNestedDTO> patchDoc = new JsonPatchDocument<SimpleDTOWithNestedDTO>();
			patchDoc.Move<int>(o => o.SimpleDTO.IntegerValue, o => o.SimpleDTO.IntegerList);

			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTOWithNestedDTO>>(serialized);
			deserialized.ApplyTo(doc);

			Assert.Equal(0, doc.IntegerValue);

			Assert.Equal(new List<int>() { 1, 2, 3, 5 }, doc.SimpleDTO.IntegerList);

		}


	}
}