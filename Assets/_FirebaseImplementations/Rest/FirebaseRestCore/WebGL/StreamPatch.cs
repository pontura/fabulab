using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yaguar.FirebaseRest
{
    // Applies the data of a "patch" event of a StreamListener to an object: each property of the JSON sets the public
    // field or property of the same name. Dates are kept as they come: the app stores ISO timestamps as strings, and
    // parsing them as DateTime would turn them into culture formatted text (ex: "11/24/2026 23:19:28")
    public static class StreamPatch
    {
        static readonly JsonSerializerSettings Settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };

        public static void ApplyTo(object target, string json)
        {
            JObject patch = JsonConvert.DeserializeObject<JObject>(json, Settings);
            if (patch == null)
                return;
            System.Type type = target.GetType();
            foreach (JProperty prop in patch.Properties()) {
                FieldInfo field = type.GetField(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                    field.SetValue(target, prop.Value.ToObject(field.FieldType));

                PropertyInfo property = type.GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanWrite)
                    property.SetValue(target, prop.Value.ToObject(property.PropertyType));
            }
        }
    }
}
