using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Unity.TubeRenderer
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class TubeRenderer : MonoBehaviour
    {
        [Min(1)]
        public int subdivisions = 3;
        [Min(0)]
        public int segments = 8;
        public Vector3[] positions = new Vector3[0];
        [Min(0)]
        public float startWidth = 1f;
        [Min(0)]
        public float endWidth = 1f;
        public bool showNodesInEditor = false;
        public Vector2 uvScale = Vector2.one;
        public bool inside = false;
        public Material Material;
        public bool UseWorldSpace = true;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh mesh = null;
        private float theta = 0f;
        private int lastUpdate = 0;

        public Vector3 GetPosition(float f)
        {
            int a = Math.Max(0, Math.Min(positions.Length, Mathf.FloorToInt(f)));
            int b = Math.Max(0, Math.Min(positions.Length, Mathf.CeilToInt(f)));
            float t = f - a;
            return Vector3.Lerp(positions[a], positions[b], t);
        }

        public Vector3 GetPosition(int index)
        {
            return positions[index];
        }

        public void SetPositions(Vector3[] positions)
        {
            this.positions = positions;
        }

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            Material = (Material == null) ? new Material(Shader.Find("Universal Render Pipeline/Lit")) : Material;
            if (mesh == null) mesh = new Mesh();
            meshFilter.mesh = CreateMesh();
            lastUpdate = PropHashCode();
        }

        private Mesh CreateMesh()
        {
            if (positions != null && positions.Length == 0) return null;

            Vector3[] interpolatedPositions = Enumerable.Range(0, (positions.Length - 1) * subdivisions)
                .Select(i => ((float)i) / ((float)subdivisions))
                .Select(f => GetPosition(f))
                .Append(positions.Last())
                .ToArray();

            if (UseWorldSpace)
                interpolatedPositions = interpolatedPositions
                                            .Select((p) => transform.InverseTransformPoint(p))
                                            .ToArray();

            // To have correct UVs, an additional vertex to close the loop is added
            int nRadialVerts = segments + 1;
            theta = (Mathf.PI * 2) / segments;

            Vector3[] verts = new Vector3[interpolatedPositions.Length * nRadialVerts];
            Vector2[] uvs = new Vector2[verts.Length];
            Vector3[] normals = new Vector3[verts.Length];
            int[] tris = new int[2 * 3 * verts.Length];
            float cumDist = 0;

            for (int i = 0; i < interpolatedPositions.Length; i++)
            {
                float dia = Mathf.Lerp(startWidth, endWidth, (float)i / interpolatedPositions.Length);

                cumDist += (i == 0) ? 0 : Vector3.Distance(interpolatedPositions[i], interpolatedPositions[i - 1]);

                Vector3 localForward = GetVertexFwd(interpolatedPositions, i);
                Vector3 localUp = Vector3.Cross(localForward, Vector3.up);
                Vector3 localRight = Vector3.Cross(localForward, localUp);

                for (int j = 0; j < nRadialVerts; ++j)
                {
                    float t = theta * j;
                    Vector3 vert = interpolatedPositions[i] + (Mathf.Sin(t) * localUp * dia) + (Mathf.Cos(t) * localRight * dia);
                    int x = i * nRadialVerts + j;
                    verts[x] = vert;
                    // Map V in world space using the current distance along the tube 
                    uvs[x] = uvScale * new Vector2(t / (Mathf.PI * 2), cumDist);
                    normals[x] = (vert - interpolatedPositions[i]).normalized;
                    if (i >= interpolatedPositions.Length - 1) continue;
                    // Do not create degenerate triangles
                    if (i == nRadialVerts - 1) continue;

                    if (inside) normals[x] = -normals[x];
                    if (inside)
                    {
                        tris[x * 6] = x;
                        tris[x * 6 + 1] = x + nRadialVerts;
                        tris[x * 6 + 2] = x + 1;

                        tris[x * 6 + 3] = x;
                        tris[x * 6 + 4] = x + nRadialVerts - 1;
                        tris[x * 6 + 5] = x + nRadialVerts;
                    }
                    else
                    {
                        tris[x * 6] = x + 1;
                        tris[x * 6 + 1] = x + nRadialVerts;
                        tris[x * 6 + 2] = x;

                        tris[x * 6 + 3] = x + nRadialVerts;
                        tris[x * 6 + 4] = x + nRadialVerts - 1;
                        tris[x * 6 + 5] = x;
                    }
                }
            }
            mesh.Clear();
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.normals = normals;
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            meshRenderer.material = Material;
            return mesh;
        }

        private Vector3 GetVertexFwd(Vector3[] positions, int i)
        {
            Vector3 lastPosition;
            Vector3 thisPosition;
            if (i <= 0)
            {
                lastPosition = positions[i];
            }
            else
            {
                lastPosition = positions[i - 1];
            }
            if (i < positions.Length - 1)
            {
                thisPosition = positions[i + 1];
            }
            else
            {
                thisPosition = positions[i];
            }
            return (lastPosition - thisPosition).normalized;
        }

        private void OnDrawGizmos()
        {
            if (showNodesInEditor)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < positions.Length; ++i)
                {
                    float dia = Mathf.Lerp(startWidth, endWidth, (float)i / positions.Length);
                    Vector3 pos = UseWorldSpace ? positions[i] : transform.TransformPoint(positions[i]);
                    Gizmos.DrawSphere(pos, dia);
                }
            }
        }

        private int PropHashCode()
        {
            return positions.Aggregate(0, (total, it) => total
                    ^ it.GetHashCode())
                    ^ positions.GetHashCode()
                    ^ segments.GetHashCode()
                    ^ subdivisions.GetHashCode()
                    ^ startWidth.GetHashCode()
                    ^ endWidth.GetHashCode()
                    ^ Material.GetHashCode();
        }

        public void EditorUpdate()
        {
            if (lastUpdate != PropHashCode())
            {
                meshFilter.mesh = CreateMesh();
            }
        }
    }
}