using TuioNet.Tuio11;
using UnityEngine;

namespace TouchScript.InputSources
{
    public class Tuio11Input : ITuioInput
    {
        private readonly Tuio11Dispatcher _dispatcher;
        private readonly TuioInput _input;
        public Tuio11Input(TuioInput input)
        {
            _input = input;
            _dispatcher = (Tuio11Dispatcher)input.TuioDispatcher;
        }
        
        public void AddCallbacks()
        {
            _dispatcher.OnCursorAdd += AddCursor;
            _dispatcher.OnCursorUpdate += UpdateCursor;
            _dispatcher.OnCursorRemove += RemoveCursor;
            
            _dispatcher.OnObjectAdd += AddObject;
            _dispatcher.OnObjectUpdate += UpdateObject;
            _dispatcher.OnObjectRemove += RemoveObject;
        }

        public void RemoveCallbacks()
        {
            _dispatcher.OnCursorAdd -= AddCursor;
            _dispatcher.OnCursorUpdate -= UpdateCursor;
            _dispatcher.OnCursorRemove -= RemoveCursor;
            
            _dispatcher.OnObjectAdd -= AddObject;
            _dispatcher.OnObjectUpdate -= UpdateObject;
            _dispatcher.OnObjectRemove -= RemoveObject;
        }
        
        private void AddObject(object sender, Tuio11Object tuio11Object)
        {
            lock (this)
            {
                var normalizedPosition = new Vector2(tuio11Object.Position.X, tuio11Object.Position.Y);
                _input.AddObject(tuio11Object.SessionId, (int)tuio11Object.SymbolId, normalizedPosition, tuio11Object.Angle);
            }
        }
        
        private void UpdateObject(object sender, Tuio11Object tuio11Object)
        {
            lock (this)
            {
                var normalizedPosition = new Vector2(tuio11Object.Position.X, tuio11Object.Position.Y);
                _input.UpdateObject(tuio11Object.SessionId, normalizedPosition, tuio11Object.Angle);
            }
        }
        
        private void RemoveObject(object sender, Tuio11Object tuio11Object)
        {
            lock (this)
            {
                _input.RemoveObject(tuio11Object.SessionId);
            }
        }
        
        private void AddCursor(object sender, Tuio11Cursor tuio11Cursor)
        {
            lock(this)
            {
                var normalizedPosition = new Vector2(tuio11Cursor.Position.X, tuio11Cursor.Position.Y);
                _input.AddTouch(tuio11Cursor.SessionId, normalizedPosition);
            }
        }
        
        private void UpdateCursor(object sender, Tuio11Cursor tuio11Cursor)
        {
            lock (this)
            {
                var normalizedPosition = new Vector2(tuio11Cursor.Position.X, tuio11Cursor.Position.Y);
                _input.UpdateTouch(tuio11Cursor.SessionId, normalizedPosition);
            }
        }
        
        private void RemoveCursor(object sender, Tuio11Cursor tuio11Cursor)
        {
            lock (this)
            {
                _input.RemoveTouch(tuio11Cursor.SessionId);
            }
        }
    }
}