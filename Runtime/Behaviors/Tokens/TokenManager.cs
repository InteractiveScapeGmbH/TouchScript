using System.Collections.Generic;
using TouchScript.Behaviors.Cursors;
using TouchScript.Pointers;
using TouchScript.Utils;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

namespace TouchScript.Behaviors.Tokens
{
    public class TokenManager : MonoBehaviour
    {
        [FormerlySerializedAs("_objectPrefab")] [SerializeField] private TokenTransform _tokenPrefab;

        private const int MaxObjectCount = 24;
        private ObjectPool<TokenTransform> _tokenPool;
        private readonly Dictionary<int, TokenTransform> _tokens = new();
        private CustomSampler _tokenSampler;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _tokenSampler = CustomSampler.Create("[TouchScript] Token Transform");
            _tokenSampler.Begin();
            _tokenPool = new ObjectPool<TokenTransform>(MaxObjectCount, SpawnTokenTransform, null, HideTokenTransform);
            _rectTransform = GetComponent<RectTransform>();
            _tokenSampler.End();
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
                var tokenTransform = _tokenPool.Get();
                tokenTransform.Init(_rectTransform, pointer);
                _tokens.Add(pointer.Id, tokenTransform);
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
                if (!_tokens.TryGetValue(pointer.Id, out var token)) continue;
                token.UpdatePointer(pointer);
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
                if(!_tokens.TryGetValue(pointer.Id, out var token)) continue;
                _tokens.Remove(pointer.Id);
                _tokenPool.Release(token);
            }
            _tokenSampler.End();
        }
        
        private void HideTokenTransform(PointerCursor pointer)
        {
            pointer.Hide();
        }

        private TokenTransform SpawnTokenTransform()
        {
            return Instantiate(_tokenPrefab);
        }
    }
}