namespace InfluxDB.Client.Core
{
    /// <summary>
    /// Asynchronous query that can be cancelled. Cancellation is perform by the <see cref="Cancel"/> method.
    /// </summary>
    public interface ICancellable
    {
        
        /// <summary>
        /// Attempt to cancel execution of this query.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Returns true if query was cancelled.
        /// </summary>
        /// <returns>true if query was cancell</returns>
        bool IsCancelled();
    }
}