using System;
using System.IO;

namespace InfluxDB.Client.Test.utils
{
    internal static class TestUtils
    {
        public static string ResolveCertificatePath(string fileName)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(baseDir, "certificates", fileName);
            if (File.Exists(path))
            {
                return path;
            }

            var dir = new DirectoryInfo(baseDir);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "Client.Test", "certificates", fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                var directCandidate = Path.Combine(dir.FullName, "certificates", fileName);
                if (File.Exists(directCandidate))
                {
                    return directCandidate;
                }

                dir = dir.Parent;
            }

            return Path.Combine(baseDir, "certificates", fileName);
        }
    }
}