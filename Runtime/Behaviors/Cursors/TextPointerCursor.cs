using System.Text;
using TMPro;
using TouchScript.Pointers;
using TouchScript.Utils;

namespace TouchScript.Behaviors.Cursors
{
    /// <summary>
    /// Abstract class for pointer cursors with text.
    /// </summary>
    /// <typeparam name="T">Pointer type.</typeparam>
    /// <seealso cref="TouchScript.Behaviors.Cursors.PointerCursor" />
    public abstract class TextPointerCursor<T> : PointerCursor<T> where T : IPointer
    {
        #region Public properties

        /// <summary>
        /// Should the value of <see cref="Pointer.Id"/> be shown on screen on the cursor.
        /// </summary>
        public bool ShowPointerId = true;

        /// <summary>
        /// Should the value of <see cref="Pointer.Flags"/> be shown on screen on the cursor.
        /// </summary>
        public bool ShowFlags = false;

        /// <summary>
        /// The link to UI.Text component.
        /// </summary>
        public TMP_Text Text;

        #endregion

        #region Private variables

        private static StringBuilder s_stringBuilder = new StringBuilder(64);

        #endregion

        #region Protected methods

        /// <inheritdoc />
        protected override void UpdateOnce(IPointer pointer)
        {
            base.UpdateOnce(pointer);

            if (Text == null) return;
            if (!TextIsVisible())
            {
                Text.enabled = false;
                return;
            }

            Text.enabled = true;
            s_stringBuilder.Length = 0;
            GenerateText((T) pointer, s_stringBuilder);

            Text.text = s_stringBuilder.ToString();
        }

        /// <summary>
        /// Generates text for pointer.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="str">The string builder to use.</param>
        protected virtual void GenerateText(T pointer, StringBuilder str)
        {
            if (ShowPointerId)
            {
                str.Append("Id: ");
                str.Append(pointer.Id);
            }
            if (ShowFlags)
            {
                if (str.Length > 0) str.Append("\n");
                str.Append("Flags: ");
                BinaryUtils.ToBinaryString(pointer.Flags, str, 8);
            }
        }

        /// <summary>
        /// Indicates if text should be visible.
        /// </summary>
        /// <returns><c>True</c> if pointer text should be displayed; <c>false</c> otherwise.</returns>
        protected virtual bool TextIsVisible()
        {
            return ShowPointerId || ShowFlags;
        }

        /// <summary>
        /// Typed version of <see cref="GetPointerHash"/>. Returns a hash of a cursor state.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <returns>Integer hash.</returns>
        protected virtual uint GetHash(T pointer)
        {
            var hash = (uint) State;
            if (ShowFlags) hash += pointer.Flags << 3;
            return hash;
        }

        /// <inheritdoc />
        protected sealed override uint GetPointerHash(IPointer pointer)
        {
            return GetHash((T) pointer);
        }

        #endregion
    }
}