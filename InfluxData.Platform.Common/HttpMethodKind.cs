using System;

namespace Platform.Common
{
    public enum HttpMethodKind
    {
        Get,
        Post,
        Put,
        Patch,
        Delete
    }

    public static class HttpMethodKindExtensions
    {
        /// <summary>
        /// Method kind to HTTP method name
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <returns>method name</returns>
        /// <exception cref="ArgumentException">for not supported method kind</exception>
        public static string Name(this HttpMethodKind method)
        {
            switch (method)
            {
                case HttpMethodKind.Get:
                    return "GET";
                case HttpMethodKind.Post:
                    return "POST";
                case HttpMethodKind.Put:
                    return "PUT";
                case HttpMethodKind.Patch:
                    return "PATCH";
                case HttpMethodKind.Delete:
                    return "DELETE";
            }

            throw new ArgumentException($"Bad value: {method}");
        }
    }
}