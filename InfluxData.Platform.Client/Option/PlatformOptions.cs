using System;
using Platform.Common.Platform;

namespace InfluxData.Platform.Client.Option
{
    /// <summary>
    /// PlatformOptions are used to configure the InfluxData Platform connections.
    /// </summary>
    public class PlatformOptions
    {
        public string Url { get; private set; }

        public EAuthScheme AuthScheme { get; private set; }
        public char[] Token { get; private set; }
        public string Username { get; private set; }
        public char[] Password { get; private set; }

        public TimeSpan Timeout { get; private set; }

        private PlatformOptions(Builder builder)
        {
            Arguments.CheckNotNull(builder, "PlatformOptions.Builder");

            Url = builder.UrlString;
            AuthScheme = builder.AuthScheme;
            Token = builder.Token;
            Username = builder.Username;
            Password = builder.Password;
            Timeout = builder.TimeOut;
        }

        /// <summary>
        /// The scheme uses to Authentication.
        /// </summary>
        public enum EAuthScheme
        {
            /// <summary>
            /// Basic auth.
            /// </summary>
            Session,

            /// <summary>
            /// Authentication token.
            /// </summary>
            Token
        }

        /// <summary>
        /// A builder for <see cref="PlatformOptions"/>.
        /// </summary>
        public sealed class Builder
        {
            public string UrlString { get; private set; }

            public EAuthScheme AuthScheme { get; private set; }
            public char[] Token { get; private set; }
            public string Username { get; private set; }
            public char[] Password { get; private set; }

            public TimeSpan TimeOut { get; private set; }

            public static Builder CreateNew()
            {
                return new Builder();
            }

            /// <summary>
            /// Set the url to connect to Platform.
            /// </summary>
            /// <param name="url">the url to connect to Platform. It must be defined.</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder Url(string url)
            {
                Arguments.CheckNonEmptyString(url, "url");

                UrlString = url;

                return this;
            }

            /// <summary>
            /// Set the Timeout to connect to Platform.
            /// </summary>
            /// <param name="timeout">the timeout to connect to Platform. It must be defined.</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder Timeout(TimeSpan timeout)
            {
                Arguments.CheckNotNull(timeout, "timeout");

                TimeOut = timeout;

                return this;
            }

            /// <summary>
            /// Setup authorization by <see cref="EAuthScheme.Session"/>.
            /// </summary>
            /// <param name="username">the username to use in the basic auth</param>
            /// <param name="password">the password to use in the basic auth</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder Authenticate(string username,
                char[] password)
            {
                Arguments.CheckNonEmptyString(username, "username");
                Arguments.CheckNotNull(password, "password");

                AuthScheme = EAuthScheme.Session;
                Username = username;
                Password = password;

                return this;
            }

            /// <summary>
            /// Setup authorization by <see cref="EAuthScheme.Token"/>.
            /// </summary>
            /// <param name="token">the token to use for the authorization</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder AuthenticateToken(char[] token)
            {
                Arguments.CheckNotNull(token, "token");

                AuthScheme = EAuthScheme.Token;
                Token = token;

                return this;
            }

            /// <summary>
            /// Build an instance of PlatformOptions.
            /// </summary>
            /// <returns><see cref="PlatformOptions"/></returns>
            /// <exception cref="InvalidOperationException">If url is not defined.</exception>
            public PlatformOptions Build()
            {
                if (string.IsNullOrEmpty(UrlString))
                {
                    throw new InvalidOperationException("The url to connect to Platform has to be defined.");
                }

                if (TimeOut == TimeSpan.Zero || TimeOut == TimeSpan.FromMilliseconds(0))
                {
                    TimeOut = TimeSpan.FromSeconds(60);
                }

                return new PlatformOptions(this);
            }
        }
    }
}