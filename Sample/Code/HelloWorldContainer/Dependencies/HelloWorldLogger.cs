using VoxCake.IoC;

namespace MyNamespace
{
    public class HelloWorldLogger : IRegistrableDependency, IRemovableDependency
    {
        private readonly IMessageLogger _messageLogger;
        private readonly HelloWorldObserver _helloWorldObserver;

        public HelloWorldLogger(
            IMessageLogger messageLogger,
            HelloWorldObserver helloWorldObserver)
        {
            _messageLogger = messageLogger;
            _helloWorldObserver = helloWorldObserver;
        }

        void IRegistrableDependency.OnRegister()
        {
            _helloWorldObserver.AddListener(LogHelloWorld);
        }

        void IRemovableDependency.OnRemove()
        {
            _helloWorldObserver.RemoveListener(LogHelloWorld);
        }
        
        private void LogHelloWorld()
        {
            _messageLogger.LogMessage("Hello World!");
        }
    }
}