using System;
using System.Globalization;
using Newtonsoft.Json;

namespace UltimateTerrains
{
    public class Vector3sJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector3s);
        }

        public override void WriteJson(JsonWriter writer, object val, JsonSerializer serializer)
        {
            var value = (Vector3s) val;
            writer.WriteValue(value.x.ToString(CultureInfo.InvariantCulture) + ";" +
                              value.y.ToString(CultureInfo.InvariantCulture) + ";" +
                              value.z.ToString(CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var s = (string) reader.Value;
            var parts = s.Split(';');

            return new Vector3s(short.Parse(parts[0], CultureInfo.InvariantCulture),
                                short.Parse(parts[1], CultureInfo.InvariantCulture),
                                short.Parse(parts[2], CultureInfo.InvariantCulture));
        }
    }
}