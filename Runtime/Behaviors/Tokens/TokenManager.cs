using System;
using TouchScript.Pointers;
using UnityEngine;
using UnityEngine.Profiling;

namespace TouchScript.Behaviors.Tokens
{
    public class TokenManager : MonoBehaviour
    {
        [SerializeField] private TokenEventChannel _tokenEventChannel;
        
        private CustomSampler _tokenSampler;

        public event Action<Pointer> OnTokenAdded;
        public event Action<Pointer> OnTokenUpdated;
        public event Action<Pointer> OnTokenRemoved; 

        private void Awake()
        {
            _tokenSampler = CustomSampler.Create("[TouchScript] Token Manager");
        }

        private void OnEnable()
        {
            var touchManager = TouchManager.Instance;
            if (touchManager == null) return;
            touchManager.PointersAdded += OnPointerAdded;
            touchManager.PointersUpdated += OnPointerUpdated;
            touchManager.PointersRemoved += OnPointerRemoved;
        }

        private void OnDisable()
        {
            var touchManager = TouchManager.Instance;
            if (touchManager == null) return;
            touchManager.PointersAdded -= OnPointerAdded;
            touchManager.PointersUpdated -= OnPointerUpdated;
            touchManager.PointersRemoved -= OnPointerRemoved;
        }
        private void OnPointerAdded(object sender, PointerEventArgs e)
        {
            _tokenSampler.Begin();
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                if((pointer.Flags & Pointer.FLAG_INTERNAL) > 0) continue;
                if(pointer.Type != Pointer.PointerType.Object) continue;
                _tokenEventChannel.RaiseAdded(pointer);
            }
            _tokenSampler.End();
        }

        private void OnPointerUpdated(object sender, PointerEventArgs e)
        {
            _tokenSampler.Begin();
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                _tokenEventChannel.RaiseUpdated(pointer);
            }
            _tokenSampler.End();
        }

        private void OnPointerRemoved(object sender, PointerEventArgs e)
        {
            _tokenSampler.Begin();
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                _tokenEventChannel.RaiseRemoved(pointer);
            }
            _tokenSampler.End();
        }
    }
}