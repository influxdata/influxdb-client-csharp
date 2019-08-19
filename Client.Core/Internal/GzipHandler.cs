using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using RestSharp;

namespace InfluxDB.Client.Core.Internal
{
    public class GzipHandler
    {
        private static readonly Regex ContentRegex = new Regex(@".*/write");
        private static readonly Regex AcceptRegex = new Regex(@".*/query");

        private bool _enabled;

        public void EnableGzip()
        {
            _enabled = true;
        }

        public void DisableGzip()
        {
            _enabled = false;
        }

        public bool IsEnabledGzip()
        {
            return _enabled;
        }

        public void BeforeIntercept(IRestRequest request)
        {
            if (!_enabled)
            {
                //
                // Disabled
                //
                request.AddOrUpdateParameter("Accept-Encoding", "identity", ParameterType.HttpHeader);
                request.AddDecompressionMethod(DecompressionMethods.None);
            }
            else if (ContentRegex.Match(request.Resource).Success)
            {
                //
                // GZIP request
                //
                request.AddOrUpdateParameter("Content-Encoding", "gzip", ParameterType.HttpHeader);
                request.AddOrUpdateParameter("Accept-Encoding", "identity", ParameterType.HttpHeader);
                request.AddDecompressionMethod(DecompressionMethods.None);
                
                var body = request.Parameters.FirstOrDefault(parameter =>
                    parameter.Type.Equals(ParameterType.RequestBody));

                if (body != null)
                {
                    byte[] bytes;
                    
                    if (body.Value is byte[])
                    {
                        bytes = (byte[]) body.Value;
                    }
                    else
                    {
                        bytes = Encoding.UTF8.GetBytes(body.Value.ToString());
                    }
                    
                    using (var msi = new MemoryStream(bytes))
                    using (var mso = new MemoryStream()) {
                        using (var gs = new GZipStream(mso, CompressionMode.Compress)) {
                            msi.CopyTo(gs);
                        }

                        body.Value = mso.ToArray();
                        body.Name = "application/x-gzip";
                    }
                }
            }
            else if (AcceptRegex.Match(request.Resource).Success)
            {
                //
                // GZIP response
                //
                request.AddDecompressionMethod(DecompressionMethods.GZip);
            }
            else
            {
                //
                // Disabled
                //
                request.AddOrUpdateParameter("Accept-Encoding", "identity", ParameterType.HttpHeader);
                request.AddDecompressionMethod(DecompressionMethods.None);
            }
        }

        public object AfterIntercept(int statusCode, Func<IList<HttpHeader>> headers, object body)
        {
            return body;
        }
    }
}