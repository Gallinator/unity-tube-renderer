using UnityEditor;
using UnityEngine;

namespace Unity.TubeRenderer
{
    [CustomEditor(typeof(TubeRenderer))]
    public class TubeRendererInspector : Editor
    {
        void OnEnable()
        {
            TubeRenderer script = (TubeRenderer)target;
            EditorApplication.update += script.EditorUpdate;
        }
        void OnDisable()
        {
            TubeRenderer script = (TubeRenderer)target;
            EditorApplication.update -= script.EditorUpdate;
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
