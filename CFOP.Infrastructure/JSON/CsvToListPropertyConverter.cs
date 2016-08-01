using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CFOP.Infrastructure.JSON
{
    public class CsvToListPropertyConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var csv = token.ToString();
            return csv.Split(',').Select(v => v.Trim()).ToList();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(string).IsAssignableFrom(objectType);
        }
    }
}
