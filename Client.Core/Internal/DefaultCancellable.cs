using System.Threading;

namespace InfluxDB.Client.Core.Internal
{
    public class DefaultCancellable : ICancellable
    {
        private bool _wasCancelled;
        internal readonly CancellationTokenSource CancellationToken;

        public DefaultCancellable(CancellationToken cancellationToken = default)
        {
            CancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        public void Cancel()
        {
            _wasCancelled = true;
            CancellationToken.Cancel();
        }

        public bool IsCancelled()
        {
            return _wasCancelled || CancellationToken.IsCancellationRequested;
        }
    }
}