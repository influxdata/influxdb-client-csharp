using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public void BeforeIntercept(HttpRequestMessage req)
        {
            if (!_enabled)
            {
                //
                // Disabled
                //
                SetHeader(req.Headers, "Accept-Encoding", "identity");
            }
            else if (ContentRegex.Match(req.RequestUri.AbsolutePath).Success)
            {
                //
                // GZIP request
                //
                SetHeader(req.Content.Headers, "Content-Encoding", "gzip");
                SetHeader(req.Headers,"Accept-Encoding", "identity");

                req.Content = new CompressedContent(req.Content);
            }
            else if (AcceptRegex.Match(req.RequestUri.AbsolutePath).Success)
            {
                //
                // GZIP response
                //
                SetHeader(req.Headers, "Accept-Encoding", "gzip");
            }
            else
            {
                //
                // Disabled
                //
                SetHeader(req.Headers, "Accept-Encoding", "identity");
            }
        }

        private static void SetHeader(HttpHeaders headers, string name, string value)
        {
            headers.Add(name, value);
        }

        public void AfterIntercept(int statusCode, Func<HttpResponseHeaders> headers, object body)
        {
        }
    }
    
    class CompressedContent : HttpContent
    {
        private readonly HttpContent _originalContent;

        public CompressedContent(HttpContent originalContent)
        {
            _originalContent = originalContent;

            foreach (var header in originalContent.Headers)
            {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;

            return false;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var gzipStream = new GZipStream(stream, CompressionMode.Compress, true);

            return _originalContent.CopyToAsync(gzipStream).ContinueWith(t=> gzipStream.Dispose());
        }
    }
}