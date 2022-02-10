using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using InfluxDB.Client.Core.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace InfluxDB.Client.Core.Exceptions
{
    public class InfluxException : Exception
    {
        public InfluxException(string message, Exception exception = null) : this(message, 0, exception)
        {
        }

        public InfluxException(Exception exception) : base(exception.Message, exception)
        {
            Code = 0;
        }

        public InfluxException(string message, int code, Exception exception = null) : base(message, exception)
        {
            Code = code;
            Status = 0;
        }

        /// <summary>
        ///     Gets the reference code unique to the error type. If the reference code is not present than return "0".
        /// </summary>
        public int Code { get; }

        /// <summary>
        ///     Gets the HTTP status code of the unsuccessful response. If the response is not present than return "0".
        /// </summary>
        public int Status { get; set; }
    }

    public class HttpException : InfluxException
    {
        public HttpException(string message, int status, Exception exception = null) : base(message, 0, exception)
        {
            Status = status;
        }

        /// <summary>
        ///     The JSON unsuccessful response body.
        /// </summary>
        public JObject ErrorBody { get; set; }

        /// <summary>
        ///     The retry interval is used when the InfluxDB server does not specify "Retry-After" header.
        /// </summary>
        public int? RetryAfter { get; set; }

        public static HttpException Create(RestResponse requestResult, object body)
        {
            Arguments.CheckNotNull(requestResult, nameof(requestResult));

            // var httpHeaders = LoggingHandler.ToHeaders(requestResult.Headers);

            return Create(body, requestResult.Headers, requestResult.ErrorMessage, requestResult.StatusCode,
                requestResult.ErrorException);
        }

        public static HttpException Create(HttpResponseMessage requestResult, object body)
        {
            Arguments.CheckNotNull(requestResult, nameof(requestResult));

            return Create(body, requestResult.Headers.ToHeaderParameters(), "", requestResult.StatusCode);
        }

        public static HttpException Create(object content, IEnumerable<HeaderParameter> headers, string ErrorMessage,
            HttpStatusCode statusCode, Exception exception = null)
        {
            string stringBody = null;
            var errorBody = new JObject();
            string errorMessage = null;

            int? retryAfter = null;
            var headerParameters = headers?.ToList();
            {
                var retryHeader = headerParameters?.FirstOrDefault(header => header.Name.Equals("Retry-After"));
                if (retryHeader != null)
                {
                    retryAfter = Convert.ToInt32(retryHeader.Value);
                }
            }

            if (content != null)
            {
                if (content is Stream)
                {
                    var stream = content as Stream;
                    var sr = new StreamReader(stream);
                    stringBody = sr.ReadToEnd();
                }
                else
                {
                    stringBody = content.ToString();
                }
            }

            if (!string.IsNullOrEmpty(stringBody))
            {
                try
                {
                    errorBody = JObject.Parse(stringBody);
                    if (errorBody.ContainsKey("message"))
                    {
                        errorMessage = errorBody.GetValue("message").ToString();
                    }
                }
                catch (JsonException)
                {
                    errorBody = new JObject();
                }
            }

            var keys = new[] { "X-Platform-Error-Code", "X-Influx-Error", "X-InfluxDb-Error" };

            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = headerParameters?
                    .Where(header => keys.Contains(header.Name, StringComparer.OrdinalIgnoreCase))
                    .Select(header => header.Value.ToString()).FirstOrDefault();
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = ErrorMessage;
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = stringBody;
            }

            var err = (int)statusCode switch
            {
                400 => new BadRequestException(errorMessage, exception),
                401 => new UnauthorizedException(errorMessage, exception),
                402 => new PaymentRequiredException(errorMessage, exception),
                403 => new ForbiddenException(errorMessage, exception),
                404 => new NotFoundException(errorMessage, exception),
                405 => new MethodNotAllowedException(errorMessage, exception),
                406 => new NotAcceptableException(errorMessage, exception),
                407 => new ProxyAuthenticationRequiredException(errorMessage, exception),
                408 => new RequestTimeoutException(errorMessage, exception),
                413 => new RequestEntityTooLargeException(errorMessage, exception),
                422 => new UnprocessableEntityException(errorMessage, exception),
                429 => new TooManyRequestsException(errorMessage, exception),
                500 => new InternalServerErrorException(errorMessage, exception),
                501 => new HttpNotImplementedException(errorMessage, exception),
                502 => new BadGatewayException(errorMessage, exception),
                503 => new ServiceUnavailableException(errorMessage, exception),
                _ => new HttpException(errorMessage, (int)statusCode, exception)
            };

            err.ErrorBody = errorBody;
            err.RetryAfter = retryAfter;

            return err;
        }
    }

    /// <summary>
    /// The exception for response: HTTP 400 - Bad Request.
    /// </summary>
    public class BadRequestException : HttpException
    {
        public BadRequestException(string message, Exception exception = null) : base(message, 400, exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 401 - Unauthorized.
    /// </summary>
    public class UnauthorizedException : HttpException
    {
        public UnauthorizedException(string message, Exception exception = null) : base(message, 401, exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 402 - Payment Required.
    /// </summary>
    public class PaymentRequiredException : HttpException
    {
        public PaymentRequiredException(string message, Exception exception = null) : base(message, 402, exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 403 - Forbidden.
    /// </summary>
    public class ForbiddenException : HttpException
    {
        public ForbiddenException(string message, Exception exception = null) : base(message, 403, exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 404 - Not Found.
    /// </summary>
    public class NotFoundException : HttpException
    {
        public NotFoundException(string message, Exception exception = null) : base(message, 404, exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 405 - Method Not Allowed.
    /// </summary>
    public class MethodNotAllowedException : HttpException
    {
        public MethodNotAllowedException(string message, Exception exception = null) : base(message, 405, exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 406 - Not Acceptable.
    /// </summary>
    public class NotAcceptableException : HttpException
    {
        public NotAcceptableException(string message, Exception exception = null) : base(message, 406, exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 407 - Proxy Authentication Required.
    /// </summary>
    public class ProxyAuthenticationRequiredException : HttpException
    {
        public ProxyAuthenticationRequiredException(string message, Exception exception = null) : base(message, 407,
            exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 408 - Request Timeout.
    /// </summary>
    public class RequestTimeoutException : HttpException
    {
        public RequestTimeoutException(string message, Exception exception = null) : base(message, 408, exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 413 - Request Entity Too Large.
    /// </summary>
    public class RequestEntityTooLargeException : HttpException
    {
        public RequestEntityTooLargeException(string message, Exception exception = null) : base(message, 413,
            exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 422 - Unprocessable Entity.
    /// </summary>
    public class UnprocessableEntityException : HttpException
    {
        public UnprocessableEntityException(string message, Exception exception = null) : base(message, 422, exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 429 - Too Many Requests.
    /// </summary>
    public class TooManyRequestsException : HttpException
    {
        public TooManyRequestsException(string message, Exception exception = null) : base(message, 429, exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 500 - Internal Server Error.
    /// </summary>
    public class InternalServerErrorException : HttpException
    {
        public InternalServerErrorException(string message, Exception exception = null) : base(message, 500, exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 501 - Not Implemented.
    /// </summary>
    public class HttpNotImplementedException : HttpException
    {
        public HttpNotImplementedException(string message, Exception exception = null) : base(message, 501, exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 502 - Bad Gateway.
    /// </summary>
    public class BadGatewayException : HttpException
    {
        public BadGatewayException(string message, Exception exception = null) : base(message, 502, exception)
        {
        }
    }

    /// <summary>
    /// The exception for response: HTTP 503 - Service Unavailable.
    /// </summary>
    public class ServiceUnavailableException : HttpException
    {
        public ServiceUnavailableException(string message, Exception exception = null) : base(message, 503, exception)
        {
        }
    }
}