using System;
using System.Collections.Generic;
using System.Net;
using TouchScript.Pointers;
using TouchScript.Utils;
using TuioNet.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace TouchScript.InputSources
{
    [HelpURL("https://github.com/InteractiveScapeGmbH/TouchScript/wiki/TUIOInput")]
    [AddComponentMenu("TouchScript/Input Sources/TUIO Input")]
    public class TuioInput : InputSource
    {
        [SerializeField] private TuioVersion _tuioVersion = TuioVersion.Tuio11;
        [SerializeField] private TuioConnectionType _connectionType = TuioConnectionType.UDP;
        [SerializeField] private string _ipAddress = "127.0.0.1";
        [SerializeField] private int _udpPort = 3333;

        private TuioSession _session;
        private bool _isInitialized = false;

        private readonly Dictionary<uint, TouchPointer> _touchToInternalId = new();
        private readonly Dictionary<uint, ObjectPointer> _objectToInternalId = new();

        private ObjectPool<TouchPointer> _touchPool;
        private ObjectPool<ObjectPointer> _objectPool;

        private ITuioInput _tuioInput;

        public Vector2Int Resolution { get; private set; }

        public int UdpPort
        {
            get => _udpPort;
            set
            {
                if(value < 0) return;
                UdpPort = value;
            }
        }

        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                if (IPAddress.TryParse(value, out _))
                {
                    _ipAddress = value;
                }
            }
        }
        
        public ITuioDispatcher TuioDispatcher
        {
            get
            {
                if (_session == null)
                {
                    Init();
                }

                return _session.TuioDispatcher;
            }
        }

        protected void Awake()
        {
            _touchPool = new ObjectPool<TouchPointer>(50, () => new TouchPointer(this), null, ResetPointer);
            _objectPool = new ObjectPool<ObjectPointer>(50, () => new ObjectPointer(this), null, ResetPointer);
            _tuioInput = _tuioVersion switch
            {
                TuioVersion.Tuio11 => new Tuio11Input(this),
                TuioVersion.Tuio20 => new Tuio20Input(this),
                _ => throw new ArgumentOutOfRangeException($"{typeof(TuioVersion)} has no value of {_tuioVersion}.")
            };
        }

        public void Reinit()
        {
            _session.Dispose();
            _isInitialized = false;
            Init();
        }

        protected override void Init()
        {
            if(_isInitialized) return;
            var port = UdpPort;
            if (_connectionType == TuioConnectionType.Websocket)
            {
                port = _tuioVersion switch
                {
                    TuioVersion.Tuio11 => 3333,
                    TuioVersion.Tuio20 => 3343,
                    _ => throw new ArgumentOutOfRangeException($"{typeof(TuioVersion)} has no value of {_tuioVersion}.")
                };
            }

            _session = new TuioSession(_tuioVersion, _connectionType, IpAddress, port, false);
            _isInitialized = true;
        }
        
        public void AddMessageListener(MessageListener listener)
        {
            _session.AddMessageListener(listener);
        }

        public void RemoveMessageListener(string messageProfile)
        {
            _session.RemoveMessageListener(messageProfile);
        }
        
        public void RemoveMessageListener(MessageListener listener)
        {
            RemoveMessageListener(listener.MessageProfile);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ScreenWidth = Screen.width;
            ScreenHeight = Screen.height;
            Resolution = new Vector2Int(ScreenWidth, ScreenHeight);
            _tuioInput.AddCallbacks();
        }

        protected override void OnDisable()
        {
            foreach (var touchPointer in _touchToInternalId.Values)
            {
                CancelPointer(touchPointer);
            }

            foreach (var objectPointer in _objectToInternalId.Values)
            {
                CancelPointer(objectPointer);
            }
            
            _touchToInternalId.Clear();
            _objectToInternalId.Clear();
            _tuioInput.RemoveCallbacks();
            base.OnDisable();
        }

        public override bool UpdateInput()
        {
            _session.ProcessMessages();
            return true;
        }

        public override bool CancelPointer(Pointer pointer, bool shouldReturn)
        {
            lock (this)
            {
                if (pointer.Type == Pointer.PointerType.Touch)
                {
                    uint? touchId = null;
                    foreach (var kvp in _touchToInternalId)
                    {
                        if(kvp.Value.Id != pointer.Id) continue;
                        touchId = kvp.Key;
                        break;
                    }

                    if (touchId == null) return false;
                    CancelPointer(pointer);
                    if (shouldReturn)
                    {
                        _touchToInternalId[touchId.Value] = ReturnTouch(pointer as TouchPointer);
                    }
                    else
                    {
                        _touchToInternalId.Remove(touchId.Value);
                    }

                    return true;
                }

                uint? objectId = null;
                foreach (var kvp in _objectToInternalId)
                {
                    if (kvp.Value.Id != pointer.Id) continue;
                    objectId = kvp.Key;
                    break;
                }

                if (objectId == null) return false;
                CancelPointer(pointer);
                if (shouldReturn)
                {
                    _objectToInternalId[objectId.Value] = ReturnObject(pointer as ObjectPointer);
                }
                else
                {
                    _objectToInternalId.Remove(objectId.Value);
                }

                return true;
            }
        }

        public override void INTERNAL_DiscardPointer(Pointer pointer)
        {
            switch (pointer.Type)
            {
                case Pointer.PointerType.Touch:
                    _touchPool.Release(pointer as TouchPointer);
                    break;
                case Pointer.PointerType.Object:
                    _objectPool.Release(pointer as ObjectPointer);
                    break;
            }
        }
        
        public void AddObject(uint sessionId, int symbolId, Vector2 normalizedPosition, float angle)
        {
            var pointer = _objectPool.Get();
            var screenPosition = NormalizedToScreenPosition(normalizedPosition);
            pointer.Position = RemapCoordinates(screenPosition);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown |
                               Pointer.PointerButtonState.FirstButtonPressed;
            pointer.ObjectId = symbolId;
            pointer.Angle = angle;
            AddPointer(pointer);
            PressPointer(pointer);
            _objectToInternalId.Add(sessionId, pointer);
        }

        public void UpdateObject(uint sessionId, Vector2 normalizedPosition, float angle)
        {
            if (!_objectToInternalId.TryGetValue(sessionId, out var objectPointer)) return;
            var screenPosition = NormalizedToScreenPosition(normalizedPosition);
            objectPointer.Position = RemapCoordinates(screenPosition);
            objectPointer.Angle = angle;
            UpdatePointer(objectPointer);
        }

        public void RemoveObject(uint sessionId)
        {
            if (!_objectToInternalId.Remove(sessionId, out var objectPointer)) return;
            ReleasePointer(objectPointer);
            RemovePointer(objectPointer);
        }

        private Vector2 NormalizedToScreenPosition(Vector2 normalizedPosition)
        {
            var screenPosition = new Vector2
            {
                x = normalizedPosition.x * ScreenWidth,
                y = (1f - normalizedPosition.y) * ScreenHeight,
            };
            return screenPosition;
        }

        private ObjectPointer ReturnObject(ObjectPointer pointer)
        {
            var newPointer = _objectPool.Get();
            newPointer.CopyFrom(pointer);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown |
                               Pointer.PointerButtonState.FirstButtonPressed;
            newPointer.Flags |= Pointer.FLAG_RETURNED;
            AddPointer(newPointer);
            PressPointer(newPointer);
            return newPointer;
        }
        
        public void AddTouch(uint id, Vector2 normalizedPosition)
        {
            var pointer = _touchPool.Get();
            var screenPosition = NormalizedToScreenPosition(normalizedPosition);
            pointer.Position = RemapCoordinates(screenPosition);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown |
                               Pointer.PointerButtonState.FirstButtonPressed;
            AddPointer(pointer);
            PressPointer(pointer);
            _touchToInternalId.Add(id, pointer);
        }

        public void UpdateTouch(uint id, Vector2 normalizedPosition)
        {
            if (!_touchToInternalId.TryGetValue(id, out var touchPointer)) return;
            var screenPosition = NormalizedToScreenPosition(normalizedPosition);
            touchPointer.Position = RemapCoordinates(screenPosition);
            UpdatePointer(touchPointer);
        }

        public void RemoveTouch(uint id)
        {
            if (!_touchToInternalId.Remove(id, out var touchPointer)) return;
            ReleasePointer(touchPointer);
            RemovePointer(touchPointer);
        }

        private TouchPointer ReturnTouch(TouchPointer pointer)
        {
            var newPointer = _touchPool.Get();
            newPointer.CopyFrom(pointer);
            pointer.Buttons |= Pointer.PointerButtonState.FirstButtonDown |
                               Pointer.PointerButtonState.FirstButtonPressed;
            newPointer.Flags |= Pointer.FLAG_RETURNED;
            AddPointer(newPointer);
            PressPointer(newPointer);
            return newPointer;
        }

        protected override void UpdateCoordinatesRemapper(ICoordinatesRemapper remapper) { }
        
        private void ResetPointer(Pointer pointer)
        {
            pointer.INTERNAL_Reset();
        }
    }
}