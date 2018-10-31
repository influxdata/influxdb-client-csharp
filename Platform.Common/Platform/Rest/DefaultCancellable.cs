namespace Platform.Common.Platform.Rest
{
    public class DefaultCancellable : ICancellable
    {
        private bool _wasCancelled = false;

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