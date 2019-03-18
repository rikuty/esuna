﻿using System;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

namespace UltimateTerrains
{
    public class Color32JsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color32);
        }

        public override void WriteJson(JsonWriter writer, object val, JsonSerializer serializer)
        {
            var value = (Color32) val;
            writer.WriteValue(value.r.ToString(CultureInfo.InvariantCulture) + ";" +
                              value.g.ToString(CultureInfo.InvariantCulture) + ";" +
                              value.b.ToString(CultureInfo.InvariantCulture) + ";" +
                              value.a.ToString(CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var s = (string) reader.Value;
            var parts = s.Split(';');

            return new Color32(byte.Parse(parts[0], CultureInfo.InvariantCulture),
                               byte.Parse(parts[1], CultureInfo.InvariantCulture),
                               byte.Parse(parts[2], CultureInfo.InvariantCulture),
                               byte.Parse(parts[3], CultureInfo.InvariantCulture));
        }
    }
}