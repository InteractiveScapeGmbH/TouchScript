using System;
using TouchScript.Behaviors.Cursors;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Behaviors.Tokens
{
    public class TokenTransform : PointerCursor
    {
        [SerializeField] private TokenEventChannel _tokenEventChannel;
        public int Id { get; private set; }

        public void Init(int id)
        {
            Id = id;
            gameObject.name = $"Token {Id}";
        }
        
        private void OnEnable()
        {
            _tokenEventChannel.OnTokenUpdated += UpdateToken;
        }

        private void OnDisable()
        {
            _tokenEventChannel.OnTokenUpdated -= UpdateToken;
        }

        private void UpdateToken(ObjectPointer pointer)
        {
            if (pointer.ObjectId != Id) return;
            UpdatePointer(pointer);
        }
        
        protected override void UpdatePointerInternal(IPointer pointer)
        {
            base.UpdatePointerInternal(pointer);
            if (pointer is not ObjectPointer objectPointer) return;
            var angle = objectPointer.Angle * Mathf.Rad2Deg;
            var rotation = Quaternion.AngleAxis(angle, Vector3.back);
            Rect.rotation = rotation;
        }
    }
}