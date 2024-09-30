using TouchScript.Pointers;
using TuioNet.Common;
using TuioNet.Tuio20;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Processes TUIO 2.0 input
    /// </summary>
    
    [AddComponentMenu("TouchScript/Input Sources/TUIO 2.0 Input")]
    public sealed class Tuio20Input : TuioInput
    {
        private TuioClient _client;
        private Tuio20Processor _processor;
        protected override void Init()
        {
            if (IsInitialized) return;
            _client = new TuioClient(_connectionType, _ipAddress, _port, false);
            _processor = new Tuio20Processor(_client);
            Connect();
            IsInitialized = true;
        }
        
        protected override void Connect()
        {
            _client?.Connect();

            _processor.OnObjectAdded += TuioAdd;
            _processor.OnObjectUpdated += TuioUpdate;
            _processor.OnObjectRemoved += TuioRemove;
        }

        protected override void Disconnect()
        {
            _processor.OnObjectAdded -= TuioAdd;
            _processor.OnObjectUpdated -= TuioUpdate;
            _processor.OnObjectRemoved -= TuioRemove;
            _client?.Disconnect();
        }
        
        public override bool UpdateInput()
        {
            _client.ProcessMessages();
            return true;
        }

        private void TuioAdd(Tuio20Object tuio20Object)
        {
            lock (this)
            {
                if (tuio20Object.ContainsNewTuioPointer())
                {
                    var tuioPointer = tuio20Object.Pointer;
                    var screenPosition = new Vector2
                    {
                        x = tuioPointer.Position.X * ScreenWidth,
                        y = (1f - tuioPointer.Position.Y) * ScreenHeight
                    };
                   TouchToInternalId.Add(tuioPointer.SessionId, AddTouch(screenPosition));
                }

                if (tuio20Object.ContainsNewTuioToken())
                {
                    var token = tuio20Object.Token;
                    var screenPosition = new Vector2
                    {
                        x = token.Position.X * ScreenWidth,
                        y = (1f - token.Position.Y) * ScreenHeight
                    };
                    var objectPointer = AddObject(screenPosition);
                    UpdateObjectProperties(objectPointer,token);
                    ObjectToInternalId.Add(token.SessionId, objectPointer);
                }
                
            }
        }

        private void TuioUpdate(Tuio20Object tuio20Object)
        {
            lock (this)
            {
                if (tuio20Object.ContainsTuioPointer())
                {
                    var tuioPointer = tuio20Object.Pointer;
                    if(!TouchToInternalId.TryGetValue(tuioPointer.SessionId, out var touchPointer)) return;
                    var screenPosition = new Vector2
                    {
                        x = tuioPointer.Position.X * ScreenWidth,
                        y = (1f - tuioPointer.Position.Y) * ScreenHeight
                    };
                    touchPointer.Position = RemapCoordinates(screenPosition);
                    UpdatePointer(touchPointer);
                }

                if (tuio20Object.ContainsTuioToken())
                {
                    var token = tuio20Object.Token;
                    if (!ObjectToInternalId.TryGetValue(token.SessionId, out var objectPointer)) return;
                    var screenPosition = new Vector2
                    {
                        x = token.Position.X * ScreenWidth,
                        y = (1f - token.Position.Y) * ScreenHeight
                    };
                    objectPointer.Position = RemapCoordinates(screenPosition);
                    UpdateObjectProperties(objectPointer, token);
                    UpdatePointer(objectPointer);
                }
            }
        }

        private void TuioRemove(Tuio20Object tuio20Object)
        {
            lock (this)
            {
                if (tuio20Object.ContainsTuioPointer())
                {
                    var tuioPointer = tuio20Object.Pointer;
                    if (!TouchToInternalId.TryGetValue(tuioPointer.SessionId, out var touchPointer)) return;
                    TouchToInternalId.Remove(tuioPointer.SessionId);
                    ReleasePointer(touchPointer);
                    RemovePointer(touchPointer);
                }

                if (tuio20Object.ContainsTuioToken())
                {
                    var token = tuio20Object.Token;
                    if (!ObjectToInternalId.TryGetValue(token.SessionId, out var objectPointer)) return;
                    ObjectToInternalId.Remove(token.SessionId);
                    ReleasePointer(objectPointer);
                    RemovePointer(objectPointer);
                }
            }
        }

        public void TuioRefresh(TuioTime tuioTime) { }
        
        private void UpdateObjectProperties(ObjectPointer pointer, Tuio20Token token)
        {
            pointer.ObjectId = (int)token.ComponentId;
            pointer.Angle = token.Angle;
        }
    }
}