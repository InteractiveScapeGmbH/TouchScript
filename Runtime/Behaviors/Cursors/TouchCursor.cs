/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;
using TouchScript.Pointers;
using UnityEngine;

namespace TouchScript.Behaviors.Cursors
{
    /// <summary>
    /// Cursor for touch pointers.
    /// </summary>
    [HelpURL("http://touchscript.github.io/docs/html/T_TouchScript_Behaviors_Cursors_TouchCursor.htm")]
    public class TouchCursor : PointerCursor
    {
        private TouchPointer _pointer;
        protected override void UpdateOnce(IPointer pointer)
        {
            _pointer = (TouchPointer)pointer;
            gameObject.name = $"Touch {_pointer.Id}";
        }
    }
}