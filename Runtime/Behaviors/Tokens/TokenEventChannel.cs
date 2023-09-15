using System;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Behaviors.Tokens
{
    public class TokenEventChannel : ScriptableObject
    {
        public event Action<Pointer> OnTokenAdded;
        public event Action<Pointer> OnTokenUpdated;
        public event Action<Pointer> OnTokenRemoved;

        public void RaiseAdded(Pointer pointer)
        {
            OnTokenAdded?.Invoke(pointer);
        }

        public void RaiseUpdated(Pointer pointer)
        {
            OnTokenUpdated?.Invoke(pointer);
        }

        public void RaiseRemoved(Pointer pointer)
        {
            OnTokenRemoved?.Invoke(pointer);
        }
    }
}