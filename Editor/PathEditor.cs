using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nothke.Paths
{
    [CustomEditor(typeof(Path))]
    public class PathEditor : Editor
    {
        Path component;

        //SerializedProperty prop;

        private void OnEnable()
        {
            component = (Path)target;

            //prop = serializedObject.FindProperty("SomeProperty");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //EditorGUILayout.PropertyField(prop);

            if (GUILayout.Button("Update from children"))
                component.GetFromChildren();

            if (GUILayout.Button("Reverse"))
                component.Reverse();

            GUILayout.Label("Length: " + component.GetLength());

            serializedObject.ApplyModifiedProperties();
        }
    }
}