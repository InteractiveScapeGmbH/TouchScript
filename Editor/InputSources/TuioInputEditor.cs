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
        private SerializedProperty _tuioVersion;
        private SerializedProperty _connectionType;
        private SerializedProperty _udpPort;
        private SerializedProperty _ipAddress;

        protected override void OnEnable()
        {
            base.OnEnable();
            _instance = target as TuioInput;
            _tuioVersion = serializedObject.FindProperty("_tuioVersion");
            _connectionType = serializedObject.FindProperty("_connectionType");
            _udpPort = serializedObject.FindProperty("_udpPort");
            _ipAddress = serializedObject.FindProperty("_ipAddress");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_tuioVersion);
            EditorGUILayout.PropertyField(_connectionType);
            EditorGUILayout.PropertyField(_udpPort);
            EditorGUILayout.PropertyField(_ipAddress);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}