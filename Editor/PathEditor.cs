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
            {
                component.GetFromChildren();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Reverse"))
            {
                component.Reverse();
                SceneView.RepaintAll();
            }

            GUILayout.Label("Length: " + component.GetLength());

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (Event.current.commandName == "FrameSelected")
            {
                Event.current.commandName = "";

                if (component.PointCount > 0)
                {
                    Bounds b = new Bounds(component.points[0].position, Vector3.zero);

                    for (int i = 0; i < component.PointCount; i++)
                    {
                        b.Encapsulate(component.points[i].position);
                    }

                    float largestDim = Mathf.Max(b.extents.x, Mathf.Max(b.extents.y, b.extents.z));

                    SceneView.currentDrawingSceneView.LookAt(
                        b.center, SceneView.currentDrawingSceneView.rotation, largestDim);
                }
            }
        }
    }
}