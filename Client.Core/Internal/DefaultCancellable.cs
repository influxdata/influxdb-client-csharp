namespace InfluxDB.Client.Core.Internal
{
    public class DefaultCancellable : ICancellable
    {
        private bool _wasCancelled;

        public void Cancel()
        {
            _wasCancelled = true;
        }

        public bool IsCancelled()
        {
            return _wasCancelled;
        }
    }
}