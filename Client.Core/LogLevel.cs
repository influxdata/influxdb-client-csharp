namespace InfluxDB.Client.Core
{
    /// <summary>
    /// This enum represents REST client verbosity levels.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Disable logging.
        /// </summary>
        None,
        
        /// <summary>
        /// Logs request and response lines.
        /// </summary>
        Basic,
        
        /// <summary>
        /// Logs request and response lines including headers.
        /// </summary>
        Headers,
        
        /// <summary>
        /// Logs request and response lines including headers and body (if present).
        /// <para>Note that applying the `Body` LogLevel will disable chunking while streaming
        /// and will load the whole response into memory.</para>
        /// </summary>
        Body
    }
}