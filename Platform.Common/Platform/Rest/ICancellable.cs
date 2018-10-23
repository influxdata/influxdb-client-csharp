namespace Platform.Common.Platform.Rest
{
    public interface ICancellable
    {
        /**
         * Asynchronous query that can be cancelled. Cancellation is perform by the {@code cancel} method.
         *
         */

        /**
         * Attempt to cancel execution of this query.
         */
        void Cancel();

        /**
         * Returns {@link Boolean#TRUE} if query was cancelled.
         *
         * @return {@link Boolean#TRUE} if query was cancelled
         */
        bool IsCancelled();
    }
}