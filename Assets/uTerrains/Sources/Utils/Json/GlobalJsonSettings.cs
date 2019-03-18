using System.Globalization;
using Newtonsoft.Json;

namespace UltimateTerrains
{
    public static class GlobalJsonSettings
    {
        public static JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            Culture = CultureInfo.InvariantCulture,
            Converters =
            {
                new Vector3JsonConverter(),
                new Vector2JsonConverter(),
                new Vector3dJsonConverter(),
                new Vector3iJsonConverter(),
                new Vector3bJsonConverter(),
                new Vector3sJsonConverter(),
                new Vector2iJsonConverter(),
                new Vector4JsonConverter(),
                new Color32JsonConverter(),
                new ColorJsonConverter()
            }
        };

        public static JsonSerializerSettings SettingsLite = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            Culture = CultureInfo.InvariantCulture,
            Converters =
            {
                new Vector3JsonConverter(),
                new Vector2JsonConverter(),
                new Vector3dJsonConverter(),
                new Vector3iJsonConverter(),
                new Vector3bJsonConverter(),
                new Vector3sJsonConverter(),
                new Vector2iJsonConverter(),
                new Vector4JsonConverter(),
                new Color32JsonConverter(),
                new ColorJsonConverter()
            }
        };
    }
}