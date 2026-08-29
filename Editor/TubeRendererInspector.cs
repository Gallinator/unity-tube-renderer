using UnityEditor;
using UnityEngine;

namespace Unity.TubeRenderer
{
    [CustomEditor(typeof(TubeRenderer))]
    public class TubeRendererInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TubeRenderer script = (TubeRenderer)target;
            script.EditorUpdate();
        }
    }
}
