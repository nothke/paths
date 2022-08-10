#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

using UnityEditor;

namespace Nothke.Paths
{
    public class PathVisibilityOptions : ScriptableObject
    {
        static PathVisibilityOptions e;

        public static PathVisibilityOptions E
        {
            get
            {
                if (e) return e;
                else
                {
                    const string FILENAME = "PathVisibilityOptions.asset";//
                    const string FOLDER = "Assets/Data";
                    const string PATH = FOLDER + "/" + FILENAME;

                    e = AssetDatabase.LoadAssetAtPath<PathVisibilityOptions>(PATH);

                    if (!e)
                    {

                        e = CreateInstance<PathVisibilityOptions>();

                        if (!Directory.Exists(FOLDER))
                            Directory.CreateDirectory(FOLDER);

                        AssetDatabase.CreateAsset(e, PATH);
                        Debug.LogWarning("No PathVisibilityOptions was found so created one in " + PATH);
                    }
                    else
                    {
                        //Debug.Log("Found PathVisibilityOptions in resources");
                    }

                    return e;
                }
            }
        }

        // public variables

        public bool liveEdit = false;
        public bool drawDirectionArrows = true;
        [Min(0)] public float directionArrowsScale = 1;
        public bool highlightSelectedPaths = true;
        public Color highlightSelectedColor = new Color(1, 1, 1, 0.75f);
        [Min(0)] public float endNodesSize = 0.2f;
        public bool drawPointCrosses = false;
        [Min(0)] public float pointCrossesSize = 0.1f;
        public enum ColorMode { PathColor, Random, SpawnAllowed, ByType, TypeMask }
        public ColorMode colorMode;
        public bool showKnots = false;
    }
}

#endif