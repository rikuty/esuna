using System;
using System.Globalization;
using Newtonsoft.Json;

namespace UltimateTerrains
{
    public class Vector2iJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector2i);
        }

        public override void WriteJson(JsonWriter writer, object val, JsonSerializer serializer)
        {
            var value = (Vector2i) val;
            writer.WriteValue(value.x.ToString(CultureInfo.InvariantCulture) + ";" +
                              value.y.ToString(CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var s = (string) reader.Value;
            var parts = s.Split(';');

            return new Vector2i(int.Parse(parts[0], CultureInfo.InvariantCulture),
                                int.Parse(parts[1], CultureInfo.InvariantCulture));
        }
    }
}