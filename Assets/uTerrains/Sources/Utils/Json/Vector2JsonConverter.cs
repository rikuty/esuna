using System;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

namespace UltimateTerrains
{
    public class Vector2JsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector2);
        }

        public override void WriteJson(JsonWriter writer, object val, JsonSerializer serializer)
        {
            var value = (Vector2) val;
            writer.WriteValue(value.x.ToString(CultureInfo.InvariantCulture) + ";" +
                              value.y.ToString(CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var s = (string) reader.Value;
            var parts = s.Split(';');

            return new Vector2(float.Parse(parts[0], CultureInfo.InvariantCulture),
                               float.Parse(parts[1], CultureInfo.InvariantCulture));
        }
    }
}