using Microsoft.AspNet.JsonPatch.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace Microsoft.AspNet.JsonPatch.Test
{
	public class SimpleObjectAdapterTests
	{

		[Fact]
		public void AddResultsInReplace()
		{
			var doc = new SimpleDTO()
			{
				StringProperty = "A"
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Add<string>(o => o.StringProperty, "B");

			patchDoc.ApplyTo(doc);

			Assert.Equal("B", doc.StringProperty);

		}


		[Fact]
		public void AddToList()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Add<int>(o => o.IntegerList, 4, 0);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 4, 1, 2, 3 }, doc.IntegerList);
		}


		[Fact]
		public void AddToListInvalidPositionTooLarge()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Add<int>(o => o.IntegerList, 4, 3);

			Assert.Throws<JsonPatchException<SimpleDTO>>(() => { patchDoc.ApplyTo(doc); });
		}


		[Fact]
		public void AddToListInvalidPositionTooSmall()
		{

			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Add<int>(o => o.IntegerList, 4, -1);

			Assert.Throws<JsonPatchException<SimpleDTO>>(() => { patchDoc.ApplyTo(doc); });

		}

		[Fact]
		public void AddToListAppend()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Add<int>(o => o.IntegerList, 4);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 2, 3, 4 }, doc.IntegerList);

		}


		[Fact]
		public void Remove()
		{
			var doc = new SimpleDTO()
			{
				StringProperty = "A"
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Remove<string>(o => o.StringProperty);

			patchDoc.ApplyTo(doc);

			Assert.Equal(null, doc.StringProperty);

		}



		[Fact]
		public void RemoveFromList()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Remove<int>(o => o.IntegerList, 2);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 2 }, doc.IntegerList);
		}


		[Fact]
		public void RemoveFromListInvalidPositionTooLarge()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Remove<int>(o => o.IntegerList, 3);


			Assert.Throws<JsonPatchException<SimpleDTO>>(() => { patchDoc.ApplyTo(doc); });

		}


		[Fact]
		public void RemoveFromListInvalidPositionTooSmall()
		{

			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Remove<int>(o => o.IntegerList, -1);

			Assert.Throws<JsonPatchException<SimpleDTO>>(() => { patchDoc.ApplyTo(doc); });

		}


		[Fact]
		public void RemoveFromEndOfList()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Remove<int>(o => o.IntegerList);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 2 }, doc.IntegerList);

		}


		[Fact]
		public void Replace()
		{
			var doc = new SimpleDTO()
			{
				StringProperty = "A",
				DecimalValue = 10
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Replace<string>(o => o.StringProperty, "B");

			patchDoc.Replace(o => o.DecimalValue, 12);

			patchDoc.ApplyTo(doc);

			Assert.Equal("B", doc.StringProperty);
			Assert.Equal(12, doc.DecimalValue);



		}

		[Fact]
		public void SerializationMustNotIncudeEnvelope()
		{
			var doc = new SimpleDTO()
			{
				StringProperty = "A",
				DecimalValue = 10,
				DoubleValue = 10,
				FloatValue = 10,
				IntegerValue = 10
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Replace(o => o.StringProperty, "B");
			patchDoc.Replace(o => o.DecimalValue, 12);
			patchDoc.Replace(o => o.DoubleValue, 12);
			patchDoc.Replace(o => o.FloatValue, 12);
			patchDoc.Replace(o => o.IntegerValue, 12);

			var serialized = JsonConvert.SerializeObject(patchDoc);

			Assert.Equal(false, serialized.Contains("operations"));
			Assert.Equal(false, serialized.Contains("Operations"));


		}



		[Fact]
		public void DeserializationMustWorkWithoutEnvelope()
		{
			var doc = new SimpleDTO()
			{
				StringProperty = "A",
				DecimalValue = 10,
				DoubleValue = 10,
				FloatValue = 10,
				IntegerValue = 10
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Replace(o => o.StringProperty, "B");
			patchDoc.Replace(o => o.DecimalValue, 12);
			patchDoc.Replace(o => o.DoubleValue, 12);
			patchDoc.Replace(o => o.FloatValue, 12);
			patchDoc.Replace(o => o.IntegerValue, 12);

			// default: no envelope
			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserialized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTO>>(serialized);

			Assert.IsType<JsonPatchDocument<SimpleDTO>>(deserialized);


		}


		[Fact]
		public void DeserializationMustFailWithEnvelope()
		{
			string serialized = "{\"Operations\": [{ \"op\": \"replace\", \"path\": \"/title\", \"value\": \"New Title\"}]}";

			Assert.Throws<JsonPatchException>(() =>
			{
				var deserialized
					= JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTO>>(serialized);
			});

		}



		[Fact]
		public void SerializationTests()
		{
			var doc = new SimpleDTO()
			{
				StringProperty = "A",
				DecimalValue = 10,
				DoubleValue = 10,
				FloatValue = 10,
				IntegerValue = 10
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Replace(o => o.StringProperty, "B");
			patchDoc.Replace(o => o.DecimalValue, 12);
			patchDoc.Replace(o => o.DoubleValue, 12);
			patchDoc.Replace(o => o.FloatValue, 12);
			patchDoc.Replace(o => o.IntegerValue, 12);

			// serialize & deserialize 
			var serialized = JsonConvert.SerializeObject(patchDoc);
			var deserizalized = JsonConvert.DeserializeObject<JsonPatchDocument<SimpleDTO>>(serialized);


			deserizalized.ApplyTo(doc);

			Assert.Equal("B", doc.StringProperty);
			Assert.Equal(12, doc.DecimalValue);
			Assert.Equal(12, doc.DoubleValue);
			Assert.Equal(12, doc.FloatValue);
			Assert.Equal(12, doc.IntegerValue);

		}

		[Fact]
		public void ReplaceInList()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Replace<int>(o => o.IntegerList, 5, 0);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 5, 2, 3 }, doc.IntegerList);

		}



		[Fact]
		public void ReplaceFullList()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Replace<List<int>>(o => o.IntegerList, new List<int>() { 4, 5, 6 });

			patchDoc.ApplyTo(doc);


			Assert.Equal(new List<int>() { 4, 5, 6 }, doc.IntegerList);

		}



		[Fact]
		public void ReplaceFullListFromEnumerable()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Replace<IEnumerable<int>>(o => o.IntegerList, new List<int>() { 4, 5, 6 });

			patchDoc.ApplyTo(doc);


			Assert.Equal(new List<int>() { 4, 5, 6 }, doc.IntegerList);

		}



		[Fact]
		public void ReplaceFullListWithCollection()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Replace<IEnumerable<int>>(o => o.IntegerList, new Collection<int>() { 4, 5, 6 });

			// should throw an exception due to list/collection cast.

			Assert.Throws<JsonPatchException<SimpleDTO>>(() =>
			{
				patchDoc.ApplyTo(doc);
			});

		}



		[Fact]
		public void ReplaceAtEndOfList()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Replace<int>(o => o.IntegerList, 5);

			patchDoc.ApplyTo(doc);


			Assert.Equal(new List<int>() { 1, 2, 5 }, doc.IntegerList);

		}

		[Fact]
		public void ReplaceInListInvalidInvalidPositionTooLarge()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Replace<int>(o => o.IntegerList, 5, 3);

			Assert.Throws<JsonPatchException<SimpleDTO>>(() =>
			{
				patchDoc.ApplyTo(doc);
			});


		}


		[Fact]
		public void ReplaceInListInvalidPositionTooSmall()
		{

			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Replace<int>(o => o.IntegerList, 5, -1);

			Assert.Throws<JsonPatchException<SimpleDTO>>(() =>
			{
				patchDoc.ApplyTo(doc);
			});


		}





		[Fact]
		public void Copy()
		{
			var doc = new SimpleDTO()
			{
				StringProperty = "A",
				AnotherStringProperty = "B"
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Copy<string>(o => o.StringProperty, o => o.AnotherStringProperty);

			patchDoc.ApplyTo(doc);

			Assert.Equal("A", doc.AnotherStringProperty);

		}



		[Fact]
		public void CopyInList()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Copy<int>(o => o.IntegerList, 0, o => o.IntegerList, 1);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 1, 2, 3 }, doc.IntegerList);
		}


		[Fact]
		public void CopyFromListToEndOfList()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Copy<int>(o => o.IntegerList, 0, o => o.IntegerList);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 1, 2, 3, 1 }, doc.IntegerList);
		}




		[Fact]
		public void CopyFromListToNonList()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Copy<int>(o => o.IntegerList, 0, o => o.IntegerValue);

			patchDoc.ApplyTo(doc);

			Assert.Equal(1, doc.IntegerValue);
		}


		[Fact]
		public void CopyFromNonListToList()
		{
			var doc = new SimpleDTO()
			{
				IntegerValue = 5,
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Copy<int>(o => o.IntegerValue, o => o.IntegerList, 0);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 5, 1, 2, 3 }, doc.IntegerList);
		}




		[Fact]
		public void CopyToEndOfList()
		{
			var doc = new SimpleDTO()
			{
				IntegerValue = 5,
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Copy<int>(o => o.IntegerValue, o => o.IntegerList);

			patchDoc.ApplyTo(doc);


			Assert.Equal(new List<int>() { 1, 2, 3, 5 }, doc.IntegerList);

		}


		[Fact]
		public void Move()
		{
			var doc = new SimpleDTO()
			{
				StringProperty = "A",
				AnotherStringProperty = "B"
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Move<string>(o => o.StringProperty, o => o.AnotherStringProperty);

			patchDoc.ApplyTo(doc);

			Assert.Equal("A", doc.AnotherStringProperty);
			Assert.Equal(null, doc.StringProperty);
		}





		[Fact]
		public void MoveInList()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Move<int>(o => o.IntegerList, 0, o => o.IntegerList, 1);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 2, 1, 3 }, doc.IntegerList);
		}

		[Fact]
		public void MoveFromListToEndOfList()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Move<int>(o => o.IntegerList, 0, o => o.IntegerList);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 2, 3, 1 }, doc.IntegerList);
		}


		[Fact]
		public void MoveFomListToNonList()
		{
			var doc = new SimpleDTO()
			{
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Move<int>(o => o.IntegerList, 0, o => o.IntegerValue);

			patchDoc.ApplyTo(doc);

			Assert.Equal(new List<int>() { 2, 3 }, doc.IntegerList);
			Assert.Equal(1, doc.IntegerValue);
		}


		[Fact]
		public void MoveFromNonListToList()
		{
			var doc = new SimpleDTO()
			{
				IntegerValue = 5,
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Move<int>(o => o.IntegerValue, o => o.IntegerList, 0);

			patchDoc.ApplyTo(doc);

			Assert.Equal(0, doc.IntegerValue);
			Assert.Equal(new List<int>() { 5, 1, 2, 3 }, doc.IntegerList);
		}
		 



		[Fact]
		public void MoveToEndOfList()
		{
			var doc = new SimpleDTO()
			{
				IntegerValue = 5,
				IntegerList = new List<int>() { 1, 2, 3 }
			};

			// create patch
			JsonPatchDocument<SimpleDTO> patchDoc = new JsonPatchDocument<SimpleDTO>();
			patchDoc.Move<int>(o => o.IntegerValue, o => o.IntegerList);

			patchDoc.ApplyTo(doc);

			Assert.Equal(0, doc.IntegerValue);
			Assert.Equal(new List<int>() { 1, 2, 3, 5 }, doc.IntegerList);

		}

	}
}