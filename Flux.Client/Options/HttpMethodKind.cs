using System;

namespace Flux.Flux.Options
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
        /** <summary>
         * All-caps name of the method.
         * </summary>
         */
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