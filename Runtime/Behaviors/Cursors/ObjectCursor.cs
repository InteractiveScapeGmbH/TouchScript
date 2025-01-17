/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Behaviors.Cursors
{
    /// <summary>
    /// Cursor for object pointers.
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_Cursors_ObjectCursor.htm")]
    public class ObjectCursor : PointerCursor
    {
        private ObjectPointer _pointer;
        #region Protected methods

        protected override void UpdateOnce(IPointer pointer)
        {
            _pointer = (ObjectPointer)pointer;
            gameObject.name = $"Object {_pointer.ObjectId}";
        }

        /// <inheritdoc />
        protected override void UpdatePointerInternal(IPointer pointer)
        {
            base.UpdatePointerInternal(pointer);
            if (pointer is not ObjectPointer objectPointer) return;
            var angle = objectPointer.Angle * Mathf.Rad2Deg;
            var rotation = Quaternion.AngleAxis(angle, Vector3.back);
            Rect.rotation = rotation;
        }
        #endregion
    }
}