using System.Collections.Generic;
using UnityEngine;

namespace DeskQuest.Common
{
    /// <summary>
    /// Small procedural meshes, so the prototype ships no model files.
    /// </summary>
    public static class MeshBuilder
    {
        /// <summary>
        /// A flat annulus lying in the XZ plane, centred on the origin. Used for the
        /// placement reticle.
        /// </summary>
        public static Mesh CreateRing(float innerRadius, float outerRadius, int segments)
        {
            if (segments < 3)
                segments = 3;

            List<Vector3> vertices = new List<Vector3>(segments * 2);
            List<int> triangles = new List<int>(segments * 12);
            List<Vector2> uvs = new List<Vector2>(segments * 2);

            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                float sin = Mathf.Sin(angle);
                float cos = Mathf.Cos(angle);

                vertices.Add(new Vector3(cos * innerRadius, 0f, sin * innerRadius));
                vertices.Add(new Vector3(cos * outerRadius, 0f, sin * outerRadius));
                uvs.Add(new Vector2(i / (float)segments, 0f));
                uvs.Add(new Vector2(i / (float)segments, 1f));
            }

            // Wound both ways so the ring is visible from above regardless of which
            // fallback shader is in use. The preferred one disables backface culling,
            // but the pipeline fallbacks do not, and a one-sided reticle that happens to
            // face downwards is invisible with no error to explain it. A few hundred
            // extra triangles is not a cost worth optimising here.

            for (int i = 0; i < segments; i++)
            {
                int inner = i * 2;
                int outer = inner + 1;
                int nextInner = ((i + 1) % segments) * 2;
                int nextOuter = nextInner + 1;

                AddQuad(triangles, inner, outer, nextOuter, nextInner);
            }

            Mesh mesh = new Mesh();
            mesh.name = "DeskQuest Ring";
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Adds a quad as two triangles, in both winding orders.</summary>
        static void AddQuad(List<int> triangles, int a, int b, int c, int d)
        {
            triangles.Add(a); triangles.Add(b); triangles.Add(c);
            triangles.Add(a); triangles.Add(c); triangles.Add(d);

            triangles.Add(c); triangles.Add(b); triangles.Add(a);
            triangles.Add(d); triangles.Add(c); triangles.Add(a);
        }
    }
}
