using System.Text;
using TouchScript.Pointers;

namespace TouchScript.Behaviors.Cursors
{
    public class DebugTouchCursor : TextPointerCursor<TouchPointer>
    {
        #region Public properties

        /// <summary>
        /// Should the value of <see cref="TouchPointer.Pressure"/> be shown on the cursor.
        /// </summary>
        public bool ShowPressure = false;

        /// <summary>
        /// Should the value of <see cref="TouchPointer.Rotation"/> be shown on the cursor.
        /// </summary>
        public bool ShowRotation = false;

        #endregion

        #region Protected methods

        /// <inheritdoc />
        protected override void GenerateText(TouchPointer pointer, StringBuilder str)
        {
            base.GenerateText(pointer, str);

            if (ShowPressure)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("Pressure: ");
                str.AppendFormat("{0:0.000}", pointer.Pressure);
            }
            if (ShowRotation)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("Rotation: ");
                str.Append(pointer.Rotation);
            }
        }

        /// <inheritdoc />
        protected override bool TextIsVisible()
        {
            return base.TextIsVisible() || ShowPressure || ShowRotation;
        }

        /// <inheritdoc />
        protected override uint GetHash(TouchPointer pointer)
        {
            var hash = base.GetHash(pointer);

            if (ShowPressure) hash += (uint) (pointer.Pressure * 1024) << 8;
            if (ShowRotation) hash += (uint) (pointer.Rotation * 1024) << 16;

            return hash;
        }

        #endregion
    }
}