using LBoL.Base;
using System;
using System.Linq;
using Newtonsoft.Json;

namespace DebugMode.SerializationConverters
{
    public class JsonToStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return new Type[] { typeof(ManaGroup), typeof(ManaGroup?) }.Contains(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("deeznuts");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }
    }
}
