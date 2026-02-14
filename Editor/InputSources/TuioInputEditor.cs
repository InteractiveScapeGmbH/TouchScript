/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using TouchScript.InputSources;
using UnityEditor;

namespace TouchScript.Editor.InputSources
{
    [CustomEditor(typeof(TuioInput), true)]
    [CanEditMultipleObjects]
    internal sealed class TuioInputEditor : InputSourceEditor
    {
        private TuioInput _instance;
        private SerializedProperty _connectionType;
        private SerializedProperty _port;
        private SerializedProperty _ipAddress;
        private SerializedProperty _pointerOffset;

        protected override void OnEnable()
        {
            base.OnEnable();

            _instance = target as TuioInput;
            _connectionType = serializedObject.FindProperty("_connectionType");
            _port = serializedObject.FindProperty("_port");
            _ipAddress = serializedObject.FindProperty("_ipAddress");
            if(target is Tuio20Input)
            {
                _pointerOffset = serializedObject.FindProperty("_pointerOffset");
            }
            else
            {
                _pointerOffset = null;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_connectionType);
            EditorGUILayout.PropertyField(_port);
            EditorGUILayout.PropertyField(_ipAddress);
            if (_pointerOffset != null)
            {
                EditorGUILayout.PropertyField(_pointerOffset);
            }
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}