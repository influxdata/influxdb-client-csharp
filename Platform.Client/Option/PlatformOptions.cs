using System;
using System.Net.Http;
using Platform.Common.Platform;

namespace Platform.Client.Option
{
    /**
     * PlatformOptions are used to configure the InfluxData Platform connections.
     */
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

        /**
         * The scheme uses to Authentication.
         */
        public enum EAuthScheme
        {
            /**
             * Basic auth.
             */
            Session,

            /**
             * Authentication token.
             */
            Token
        }

        /**
          * A builder for {@code PlatformOptions}.
          */
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

            /**
             * Set the url to connect to Platform.
             *
             * @param url the url to connect to Platform. It must be defined.
             * @return {@code this}
             */
            public Builder Url(string url)
            {
                Arguments.CheckNonEmptyString(url, "url");
                
                UrlString = url;
                
                return this;
            }
            
            /**
             * Set the Timeout to connect to Platform.
             *
             * @param timeout is the timeout to connect to Platform. It must be defined.
             * @return {@code this}
             */
            public Builder Timeout(TimeSpan timeout)
            {
                Arguments.CheckNotNull(timeout, "timeout");
                
                TimeOut = timeout;
                
                return this;
            }

            /**
             * Setup authorization by {@link AuthScheme#SESSION}.
             *
             * @param username the username to use in the basic auth
             * @param password the password to use in the basic auth
             * @return {@link PlatformOptions}
             */
            public PlatformOptions.Builder Authenticate(string username,                      
                            char[] password) 
            {
                Arguments.CheckNonEmptyString(username, "username");
                Arguments.CheckNotNull(password, "password");

                AuthScheme = EAuthScheme.Session;
                Username = username;
                Password = password;

                return this;
            }

            /**
             * Setup authorization by {@link AuthScheme#TOKEN}.
             *
             * @param token the token to use for the authorization
             * @return {@link PlatformOptions}
             */
            public PlatformOptions.Builder AuthenticateToken(char[] token)
            {
                Arguments.CheckNotNull(token, "token");

                AuthScheme = EAuthScheme.Token;
                Token = token;

                return this;
            }

            /**
             * Build an instance of PlatformOptions.
             *
             * @return {@link PlatformOptions}
             */
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