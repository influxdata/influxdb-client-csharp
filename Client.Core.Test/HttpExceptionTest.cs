using System.Net;
using System.Net.Http;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;

namespace InfluxDB.Client.Core.Test
{
    [TestFixture]
    public class HttpExceptionTest
    {
        [Test]
        public void ExceptionTypes()
        {
            Assert.IsInstanceOf(typeof(BadRequestException), HttpException.Create(Response(400), ""));
            Assert.IsInstanceOf(typeof(UnauthorizedException), HttpException.Create(Response(401), ""));
            Assert.IsInstanceOf(typeof(PaymentRequiredException), HttpException.Create(Response(402), ""));
            Assert.IsInstanceOf(typeof(ForbiddenException), HttpException.Create(Response(403), ""));
            Assert.IsInstanceOf(typeof(NotFoundException), HttpException.Create(Response(404), ""));
            Assert.IsInstanceOf(typeof(MethodNotAllowedException), HttpException.Create(Response(405), ""));
            Assert.IsInstanceOf(typeof(NotAcceptableException), HttpException.Create(Response(406), ""));
            Assert.IsInstanceOf(typeof(ProxyAuthenticationRequiredException), HttpException.Create(Response(407), ""));
            Assert.IsInstanceOf(typeof(RequestTimeoutException), HttpException.Create(Response(408), ""));
            Assert.IsInstanceOf(typeof(RequestEntityTooLargeException), HttpException.Create(Response(413), ""));
            Assert.IsInstanceOf(typeof(UnprocessableEntityException), HttpException.Create(Response(422), ""));
            Assert.IsInstanceOf(typeof(TooManyRequestsException), HttpException.Create(Response(429), ""));
            Assert.IsInstanceOf(typeof(InternalServerErrorException), HttpException.Create(Response(500), ""));
            Assert.IsInstanceOf(typeof(HttpNotImplementedException), HttpException.Create(Response(501), ""));
            Assert.IsInstanceOf(typeof(BadGatewayException), HttpException.Create(Response(502), ""));
            Assert.IsInstanceOf(typeof(ServiceUnavailableException), HttpException.Create(Response(503), ""));
            Assert.IsInstanceOf(typeof(HttpException), HttpException.Create(Response(550), ""));
            Assert.IsInstanceOf(typeof(HttpException), HttpException.Create(Response(390), ""));
        }

        private static HttpResponseMessage Response(int statusCode)
        {
            return new HttpResponseMessage((HttpStatusCode)statusCode);
        }
    }
}