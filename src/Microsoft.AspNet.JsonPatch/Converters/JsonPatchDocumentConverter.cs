using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.JsonPatch.Exceptions;
using Microsoft.AspNet.JsonPatch.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNet.JsonPatch.Converters
{
    public class JsonPatchDocumentConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, 
            JsonSerializer serializer)
        {

            try
            {

                if (reader.TokenType == JsonToken.Null)
                    return null;

                // load jObject
                var jObject = JArray.Load(reader);

                // Create target object for Json => list of operations
                var concreteList = typeof(List<Operation>);
                var targetOperations = Activator.CreateInstance(concreteList);

                // Create a new reader for this jObject, and set all properties 
                // to match the original reader.
                var jObjectReader = jObject.CreateReader();
                jObjectReader.Culture = reader.Culture;
                jObjectReader.DateParseHandling = reader.DateParseHandling;
                jObjectReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
                jObjectReader.FloatParseHandling = reader.FloatParseHandling;

                // Populate the object properties
                serializer.Populate(jObjectReader, targetOperations);

                // container target: the JsonPatchDocument. 
                var container = Activator.CreateInstance(objectType, targetOperations, new DefaultContractResolver());

                return container;

            }
            catch (Exception ex)
            {
                throw new JsonPatchException(Resources.FormatInvalidJsonPatchDocument(objectType.Name), ex);
            }

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is IJsonPatchDocument)
            {
                var jsonPatchDoc = (IJsonPatchDocument)value;
                var lst = jsonPatchDoc.GetOperations();

                // write out the operations, no envelope
                serializer.Serialize(writer, lst);

            }
        }
    }
}
