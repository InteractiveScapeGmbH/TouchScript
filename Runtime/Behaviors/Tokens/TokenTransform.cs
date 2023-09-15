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

        private void OnEnable()
        {
            _tokenEventChannel.OnTokenUpdated += UpdatePointer;
        }

        private void OnDisable()
        {
            _tokenEventChannel.OnTokenUpdated -= UpdatePointer;
        }

        protected override void UpdateOnce(IPointer pointer)
        {
            base.UpdateOnce(pointer);
            if (pointer is not ObjectPointer objectPointer) return;
            Id = objectPointer.ObjectId;
            gameObject.name = $"Token {Id}";
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