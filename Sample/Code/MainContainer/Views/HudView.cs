using System;
using UnityEngine;
using UnityEngine.UI;

namespace MyNamespace
{
    public class HudView : MonoBehaviour
    {
        [SerializeField] private Text _text;
        [SerializeField] private Button _button;

        public event Action ButtonClicked;

        public void SubscribeToButtonEvent()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }
        
        public void UnsubscribeFromButtonEvent()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            ButtonClicked?.Invoke();
        }
        
        public void SetText(string text)
        {
            _text.text = text;
        }
    }
}