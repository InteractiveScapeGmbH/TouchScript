using System.Text;
using TouchScript.Behaviors.Cursors.UI;
using TouchScript.Pointers;
using TouchScript.Utils;

namespace TouchScript.Behaviors.Cursors
{
    public class DebugMouseCursor : TextPointerCursor<MousePointer>
    {
         #region Public properties

        /// <summary>
        /// Default cursor sub object.
        /// </summary>
        public TextureSwitch DefaultCursor;

        /// <summary>
        /// Pressed cursor sub object.
        /// </summary>
        public TextureSwitch PressedCursor;

        /// <summary>
        /// Should the value of <see cref="Pointer.Buttons"/> be shown on the cursor.
        /// </summary>
        public bool ShowButtons = false;

        #endregion

        #region Protected methods

        /// <inheritdoc />
        protected override void UpdateOnce(IPointer pointer)
        {
            switch (State)
            {
                case CursorState.Released:
                case CursorState.Over:
                    if (DefaultCursor != null) DefaultCursor.Show();
                    if (PressedCursor != null) PressedCursor.Hide();
                    break;
                case CursorState.Pressed:
                case CursorState.OverPressed:
                    if (DefaultCursor != null) DefaultCursor.Hide();
                    if (PressedCursor != null) PressedCursor.Show();
                    break;
            }

            base.UpdateOnce(pointer);
        }

        /// <inheritdoc />
        protected override void GenerateText(MousePointer pointer, StringBuilder str)
        {
            base.GenerateText(pointer, str);

            if (ShowButtons)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("Buttons: ");
                PointerUtils.PressedButtonsToString(pointer.Buttons, str);
            }
        }

        /// <inheritdoc />
        protected override bool TextIsVisible()
        {
            return base.TextIsVisible() || ShowButtons;
        }

        /// <inheritdoc />
        protected override uint GetHash(MousePointer pointer)
        {
            var hash = base.GetHash(pointer);

            if (ShowButtons) hash += (uint) (pointer.Buttons & Pointer.PointerButtonState.AnyButtonPressed);

            return hash;
        }

        #endregion
    }
}