using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InfluxDB.Client.Core.Internal
{
    public class EnumConverter : StringEnumConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            try
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
            catch (JsonSerializationException e)
            {
                Trace.WriteLine($"Error converting enum value. Returning null. {e}");

                return null;
            }
        }
    }
}