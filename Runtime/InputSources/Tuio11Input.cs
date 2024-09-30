/*
 * @author Valentin Simonov / http://va.lent.in/
 */

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
using TouchScript.Pointers;
using TuioNet.Common;
using TuioNet.Tuio11;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Processes TUIO 1.1 input.
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/TUIO 1.1 Input")]
    public sealed class Tuio11Input : TuioInput
    {
        private TuioClient _client;
        private Tuio11Processor _processor;

        protected override void Init()
        {
            if (IsInitialized) return;
            _client = new TuioClient(_connectionType, _ipAddress, _port, false);
            _processor = new Tuio11Processor(_client);
            Connect();
            IsInitialized = true;
        }

        protected override void Connect()
        {
            _client?.Connect();
            _processor.OnCursorAdded += AddCursor;
            _processor.OnCursorUpdated += UpdateCursor;
            _processor.OnCursorRemoved += RemoveCursor;

            _processor.OnObjectAdded += AddObject;
            _processor.OnObjectUpdated += UpdateObject;
            _processor.OnObjectRemoved += RemoveObject;
        }

        protected override void Disconnect()
        {
            _processor.OnCursorAdded -= AddCursor;
            _processor.OnCursorUpdated -= UpdateCursor;
            _processor.OnCursorRemoved -= RemoveCursor;

            _processor.OnObjectAdded -= AddObject;
            _processor.OnObjectUpdated -= UpdateObject;
            _processor.OnObjectRemoved -= RemoveObject;
            _client?.Disconnect();
        }

        public override bool UpdateInput()
        {
            _client.ProcessMessages();
            return true;
        }

        private void AddObject(Tuio11Object tuio11Object)
        {
            lock (this)
            {
                var screenPosition = new Vector2
                {
                    x = tuio11Object.Position.X * ScreenWidth,
                    y = (1f - tuio11Object.Position.Y) * ScreenHeight
                };
                var objectPointer = AddObject(screenPosition);
                UpdateObjectProperties(objectPointer, tuio11Object);
                ObjectToInternalId.Add(tuio11Object.SymbolId, objectPointer);
            }
        }

        private void UpdateObject(Tuio11Object tuio11Object)
        {
            lock (this)
            {
                if (!ObjectToInternalId.TryGetValue(tuio11Object.SymbolId, out var objectPointer)) return;
                var screenPosition = new Vector2
                {
                    x = tuio11Object.Position.X * ScreenWidth,
                    y = (1f - tuio11Object.Position.Y) * ScreenHeight
                };
                objectPointer.Position = RemapCoordinates(screenPosition);
                UpdateObjectProperties(objectPointer, tuio11Object);
                UpdatePointer(objectPointer);
            }
        }

        private void RemoveObject(Tuio11Object tuio11Object)
        {
            lock (this)
            {
                if (!ObjectToInternalId.TryGetValue(tuio11Object.SymbolId, out var objectPointer)) return;
                ObjectToInternalId.Remove(tuio11Object.SymbolId);
                ReleasePointer(objectPointer);
                RemovePointer(objectPointer);
            }
        }

        private void AddCursor(Tuio11Cursor tuio11Cursor)
        {
            lock(this)
            {
                var screenPosition = new Vector2
                {
                    x = tuio11Cursor.Position.X * ScreenWidth,
                    y = (1f - tuio11Cursor.Position.Y) * ScreenHeight
                };
                TouchToInternalId.Add(tuio11Cursor.CursorId, AddTouch(screenPosition));
            }
        }

        private void UpdateCursor(Tuio11Cursor tuio11Cursor)
        {
            lock (this)
            {
                if (!TouchToInternalId.TryGetValue(tuio11Cursor.CursorId, out var touchPointer)) return;
                var screenPosition = new Vector2
                {
                    x = tuio11Cursor.Position.X * ScreenWidth,
                    y = (1f - tuio11Cursor.Position.Y) * ScreenHeight
                };
                touchPointer.Position = RemapCoordinates(screenPosition);
                UpdatePointer(touchPointer);
            }
        }

        private void RemoveCursor(Tuio11Cursor tuio11Cursor)
        {
            lock (this)
            {
                if (!TouchToInternalId.TryGetValue(tuio11Cursor.CursorId, out var touchPointer)) return;
                TouchToInternalId.Remove(tuio11Cursor.CursorId);
                ReleasePointer(touchPointer);
                RemovePointer(touchPointer);
            }
        }

        private void UpdateObjectProperties(ObjectPointer pointer, Tuio11Object tuio11Object)
        {
            pointer.ObjectId = (int)tuio11Object.SymbolId;
            pointer.Angle = tuio11Object.Angle;
        }
    }
}

#endif