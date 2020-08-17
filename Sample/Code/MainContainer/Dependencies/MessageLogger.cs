using UnityEngine;
using VoxCake.IoC;

namespace MyNamespace
{
    public class MessageLogger : IMessageLogger, IRegistrableDependency, IRemovableDependency
    {
        private readonly MessageObserver _messageObserver;
        
        public MessageLogger(MessageObserver messageObserver)
        {
            _messageObserver = messageObserver;
        }

        void IRegistrableDependency.OnRegister()
        {
            _messageObserver.AddListener(LogMessage);
        }

        void IRemovableDependency.OnRemove()
        {
            _messageObserver.RemoveListener(LogMessage);
        }

        public void LogMessage(string message)
        {
            Debug.Log(message);
        }
    }
}