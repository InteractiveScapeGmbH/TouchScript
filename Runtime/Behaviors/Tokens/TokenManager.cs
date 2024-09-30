using System.Collections.Generic;
using TouchScript.Pointers;
using UnityEngine;
using UnityEngine.Profiling;

namespace TouchScript.Behaviors.Tokens
{
    public class TokenManager : MonoBehaviour
    {
        [SerializeField] private TokenTransform _prefab;
        [SerializeField] private TokenEventChannel _tokenEventChannel;
        [field:SerializeField] public int[] AllowedIds { get; set; }

        private readonly Dictionary<int, TokenTransform> _tokens = new ();
        private CustomSampler _tokenSampler;

        private void Awake()
        {
            _tokenSampler = CustomSampler.Create("[TouchScript] Token Manager");
        }

        private void Start()
        {
            SpawnTokens();
        }

        private void SpawnTokens()
        {
            foreach (var id in AllowedIds)
            {
                var token = Instantiate(_prefab, transform);
                token.Init(id);
                _tokens[id] = token;
                token.gameObject.SetActive(false);
            }
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
                if(pointer is not ObjectPointer tokenPointer) continue;
                _tokenEventChannel.RaiseAdded(tokenPointer);
                ShowToken(tokenPointer);
                
            }
            _tokenSampler.End();
        }

        private void ShowToken(ObjectPointer pointer)
        {
            if (_tokens.TryGetValue(pointer.ObjectId, out var token))
            {
                token.gameObject.SetActive(true);
                token.UpdatePointer(pointer);
            }
        }

        private void OnPointerUpdated(object sender, PointerEventArgs e)
        {
            _tokenSampler.Begin();
            var count = e.Pointers.Count;
            for (var i = 0; i < count; i++)
            {
                var pointer = e.Pointers[i];
                if(pointer is not ObjectPointer tokenPointer) continue;
                _tokenEventChannel.RaiseUpdated(tokenPointer);
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
                if(pointer is not ObjectPointer tokenPointer) continue;
                _tokenEventChannel.RaiseRemoved(tokenPointer);
                HideToken(tokenPointer);
            }
            _tokenSampler.End();
        }
        
        private void HideToken(ObjectPointer pointer)
        {
            if (_tokens.TryGetValue(pointer.ObjectId, out var token))
            {
                token.gameObject.SetActive(false);
            }
        }
    }
}