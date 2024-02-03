using System;

namespace InfluxDB.Client.Test
{
    internal static class DateTimeEx
    {
#if NETCOREAPP2_1_OR_GREATER
        public static readonly DateTime UnixEpoch = DateTime.UnixEpoch;
#else
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#endif
    }
}
