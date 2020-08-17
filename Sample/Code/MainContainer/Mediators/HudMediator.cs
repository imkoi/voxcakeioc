using UnityEngine;
using VoxCake.IoC;

namespace MyNamespace
{
    public class HudMediator : IRegistrableDependency, IRemovableDependency
    {
        private readonly HudView _view;
        private readonly MessageObserver _messageObserver;
        
        public HudMediator(HudView view, MessageObserver messageObserver)
        {
            _view = view;
            _messageObserver = messageObserver;
        }
        
        void IRegistrableDependency.OnRegister()
        {
            _messageObserver.AddListener(LogMessageFromObserver);
            _view.SetText("MyHud registered to context!");
            
            _view.ButtonClicked += OnButtonClicked;
            _view.SubscribeToButtonEvent();
        }

        void IRemovableDependency.OnRemove()
        {
            _messageObserver.RemoveListener(LogMessageFromObserver);

            _view.ButtonClicked -= OnButtonClicked;
            _view.UnsubscribeFromButtonEvent();
            Object.Destroy(_view);
        }

        private void OnButtonClicked()
        {
            _messageObserver.Dispatch("Button clicked!");
        }

        private void LogMessageFromObserver(string message)
        {
            _view.SetText(message);
        }
    }
}