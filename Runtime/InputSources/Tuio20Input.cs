using TuioNet.Tuio20;
using UnityEngine;

namespace TouchScript.InputSources
{
    public class Tuio20Input : ITuioInput
    {
        private readonly Tuio20Dispatcher _dispatcher;
        private readonly TuioInput _input;
        
        public Tuio20Input(TuioInput input)
        {
            _input = input;
            _dispatcher = (Tuio20Dispatcher)input.TuioDispatcher;
        }
        public void AddCallbacks()
        {
            _dispatcher.OnObjectAdd += TuioAdd;
            _dispatcher.OnObjectUpdate += TuioUpdate;
            _dispatcher.OnObjectRemove += TuioRemove;
        }

        public void RemoveCallbacks()
        {
            _dispatcher.OnObjectAdd -= TuioAdd;
            _dispatcher.OnObjectUpdate -= TuioUpdate;
            _dispatcher.OnObjectRemove -= TuioRemove;
        }
        private void TuioAdd(object sender, Tuio20Object tuio20Object)
        {
            lock (this)
            {
                Vector2 normalizedPosition;
                if (tuio20Object.ContainsNewTuioPointer())
                {
                    var tuioPointer = tuio20Object.Pointer;
                    normalizedPosition = new Vector2(tuioPointer.Position.X, tuioPointer.Position.Y);
                    _input.AddTouch(tuioPointer.SessionId, normalizedPosition);
                }
        
                if (tuio20Object.ContainsNewTuioToken())
                {
                    var token = tuio20Object.Token;
                    normalizedPosition = new Vector2(token.Position.X, token.Position.Y);
                    _input.AddObject(token.SessionId, (int)token.ComponentId, normalizedPosition, token.Angle);
                }
            }
        }
        
        private void TuioUpdate(object sender, Tuio20Object tuio20Object)
        {
            lock (this)
            {
                Vector2 normalizedPosition;
                if (tuio20Object.ContainsTuioPointer())
                {
                    var tuioPointer = tuio20Object.Pointer;
                    normalizedPosition = new Vector2(tuioPointer.Position.X, tuioPointer.Position.Y);
                    _input.UpdateTouch(tuioPointer.SessionId, normalizedPosition);
                }
        
                if (tuio20Object.ContainsTuioToken())
                {
                    var token = tuio20Object.Token;
                    normalizedPosition = new Vector2(token.Position.X, token.Position.Y);
                    _input.UpdateObject(token.SessionId, normalizedPosition, token.Angle);
                }
            }
        }
        
        private void TuioRemove(object sender, Tuio20Object tuio20Object)
        {
            lock (this)
            {
                if (tuio20Object.ContainsTuioPointer())
                {
                    var tuioPointer = tuio20Object.Pointer;
                    _input.RemoveTouch(tuioPointer.SessionId);
                }
        
                if (tuio20Object.ContainsTuioToken())
                {
                    var token = tuio20Object.Token;
                    _input.RemoveObject(token.SessionId);
                }
            }
        }
    }
}