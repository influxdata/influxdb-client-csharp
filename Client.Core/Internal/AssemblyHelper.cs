using System;
using System.Reflection;

namespace InfluxDB.Client.Core.Internal
{
    public static class AssemblyHelper
    {
        public static string GetVersion(Type type)
        {
            try
            {
                return type.GetTypeInfo()
                    .Assembly
                    .GetCustomAttribute<AssemblyFileVersionAttribute>()
                    .Version;
            }
            catch (Exception)
            {
                return "unknown";
            }
        }
    }
}