using System;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Behaviors.Tokens
{
    [CreateAssetMenu(fileName = "Token Event Channel", menuName = "TouchScript/Event Channel/New Token Event Channel", order = 0)]
    public class TokenEventChannel : ScriptableObject
    {
        public event Action<ObjectPointer> OnTokenAdded;
        public event Action<ObjectPointer> OnTokenUpdated;
        public event Action<ObjectPointer> OnTokenRemoved;

        public void RaiseAdded(ObjectPointer pointer)
        {
            OnTokenAdded?.Invoke(pointer);
        }

        public void RaiseUpdated(ObjectPointer pointer)
        {
            OnTokenUpdated?.Invoke(pointer);
        }

        public void RaiseRemoved(ObjectPointer pointer)
        {
            OnTokenRemoved?.Invoke(pointer);
        }
    }
}