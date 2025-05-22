namespace Marqdouj.JSLogger
{
    internal class LoggerScope(Action onDispose) : IDisposable
    {
        private readonly Action _onDispose = onDispose;

        public void Dispose()
        {
            _onDispose();
        }
    }
}
