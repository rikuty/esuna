using System;
using System.Globalization;
using Newtonsoft.Json;

namespace UltimateTerrains
{
    public class Vector3dJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector3d);
        }

        public override void WriteJson(JsonWriter writer, object val, JsonSerializer serializer)
        {
            var value = (Vector3d) val;
            writer.WriteValue(value.x.ToString(CultureInfo.InvariantCulture) + ";" +
                              value.y.ToString(CultureInfo.InvariantCulture) + ";" +
                              value.z.ToString(CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var s = (string) reader.Value;
            var parts = s.Split(';');

            return new Vector3d(double.Parse(parts[0], CultureInfo.InvariantCulture),
                                double.Parse(parts[1], CultureInfo.InvariantCulture),
                                double.Parse(parts[2], CultureInfo.InvariantCulture));
        }
    }
}