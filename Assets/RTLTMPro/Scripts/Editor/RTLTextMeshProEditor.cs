using UnityEditor;
using UnityEngine;

namespace RTLTMPro
{
    [CustomEditor(typeof(RTLTextMeshPro)), CanEditMultipleObjects]
    public class RTLTextMeshProEditor : Editor
    {
        private SerializedProperty originalTextProp;
        private SerializedProperty preserveNumbersProp;
        private SerializedProperty farsiProp;
        private SerializedProperty fixTagsProp;
        private SerializedProperty forceFixProp;

        private RTLTextMeshPro tmpro;

        private void OnEnable()
        {
            tmpro = (RTLTextMeshPro)target;

            preserveNumbersProp = serializedObject.FindProperty("preserveNumbers");
            farsiProp = serializedObject.FindProperty("farsi");
            fixTagsProp = serializedObject.FindProperty("fixTags");
            forceFixProp = serializedObject.FindProperty("forceFix");
            originalTextProp = serializedObject.FindProperty("originalText");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("RTL Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(originalTextProp, new GUIContent("RTL Text Input Box"));

            farsiProp.boolValue = EditorGUILayout.Toggle("Farsi", farsiProp.boolValue);
            forceFixProp.boolValue = EditorGUILayout.Toggle("Force Fix", forceFixProp.boolValue);
            preserveNumbersProp.boolValue = EditorGUILayout.Toggle("Preserve Numbers", preserveNumbersProp.boolValue);

            if (tmpro.richText)
                fixTagsProp.boolValue = EditorGUILayout.Toggle("Fix Tags", fixTagsProp.boolValue);

            if (GUILayout.Button("Re-Fix"))
            {
                tmpro.UpdateText();
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
