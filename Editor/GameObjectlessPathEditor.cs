using UnityEngine;
using UnityEditor;
using Nothke.Paths;

[CustomEditor(typeof(GameObjectlessPath))]
public class GameObjectlessPathEditor : Editor
{
    GameObjectlessPath path;

    private void OnEnable()
    {
        path = (GameObjectlessPath)target;
    }

    private void OnSceneGUI()
    {
        if (Event.current.commandName == "FrameSelected")
        {
            Event.current.commandName = "";

            if (path.PointCount > 0)
            {
                Bounds b = new Bounds(path[0], Vector3.zero);

                for (int i = 0; i < path.PointCount; i++)
                {
                    b.Encapsulate(path[i]);
                }

                float largestDim = Mathf.Max(b.extents.x, Mathf.Max(b.extents.y, b.extents.z));

                SceneView.currentDrawingSceneView.LookAt(
                    b.center, SceneView.currentDrawingSceneView.rotation, largestDim);
            }
        }
    }
}
