using System;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

namespace UltimateTerrains
{
    public class Vector4JsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector4);
        }

        public override void WriteJson(JsonWriter writer, object val, JsonSerializer serializer)
        {
            var value = (Vector4) val;
            writer.WriteValue(value.x.ToString(CultureInfo.InvariantCulture) + ";" +
                              value.y.ToString(CultureInfo.InvariantCulture) + ";" +
                              value.z.ToString(CultureInfo.InvariantCulture) + ";" +
                              value.w.ToString(CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var s = (string) reader.Value;
            var parts = s.Split(';');

            return new Vector4(float.Parse(parts[0], CultureInfo.InvariantCulture),
                               float.Parse(parts[1], CultureInfo.InvariantCulture),
                               float.Parse(parts[2], CultureInfo.InvariantCulture),
                               float.Parse(parts[3], CultureInfo.InvariantCulture));
        }
    }
}