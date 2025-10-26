using UnityEngine;
using EyE.UnityAssetTypes;
namespace EyE.Geometry
{

    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>
    /// Thread-safe container for mesh data. Can be created and modified off the main Unity thread.
    /// Only ToMesh() touches UnityEngine.Mesh and must be called on the main thread.
    /// </summary>
    public class MeshData
    {
        /// <summary>Index format to use for mesh creation.</summary>
        public IndexFormat indexFormat = IndexFormat.UInt16;

        /// <summary>Vertex positions.</summary>
        public Vector3[] vertices;

        /// <summary>Triangle indices.</summary>
        public int[] triangles;

        /// <summary>Vertex normals. Optional.</summary>
        public Vector3[] meshNormals;

        /// <summary>UV channel 0. Optional.</summary>
        public Vector2[] meshUV0s;

        /// <summary>UV channel 1. Optional.</summary>
        public Vector2[] meshUV1s;

        /// <summary>UV channel 2. Optional.</summary>
        public Vector2[] meshUV2s;

        /// <summary>Vertex colors. Optional.</summary>
        public Color[] meshColors;

        /// <summary>Per-vertex tangents. Optional.</summary>
        public Vector4[] meshTangents;

        /// <summary>Axis-aligned bounding box for the mesh.</summary>
        public Bounds bounds;

        /// <summary>Optional link for existing systems needing to track the resulting mesh.</summary>
        public FacesAndNeighbors facesAndNeighborsRef;

        /// <summary>Name for the mesh.</summary>
        public string name;

        /// <summary>Number of vertices stored.</summary>
        public int vertexCount
        {
            get
            {
                if (vertices == null)
                    return 0;
                return vertices.Length;
            }
        }

        /// <summary>
        /// Builds a Unity Mesh using the currently stored data.
        /// Must be called on the main Unity thread.
        /// </summary>
        /// <returns>Created Unity Mesh.</returns>
        public Mesh ToMesh()
        {
            Mesh newMesh = new Mesh();

            if (vertices != null && vertices.Length >= 0xFFFF)
                newMesh.indexFormat = IndexFormat.UInt32;
            else
                newMesh.indexFormat = indexFormat;

            if (vertices != null)
                newMesh.SetVertices(vertices);

            if (triangles != null)
                newMesh.SetTriangles(triangles, 0);

            if (meshNormals != null && meshNormals.Length == (vertices != null ? vertices.Length : 0))
                newMesh.SetNormals(meshNormals);

            if (meshUV0s != null)
                newMesh.SetUVs(0, meshUV0s);

            if (meshUV1s != null)
                newMesh.SetUVs(1, meshUV1s);

            if (meshUV2s != null)
                newMesh.SetUVs(2, meshUV2s);

            if (meshColors != null)
                newMesh.SetColors(meshColors);

            if (meshTangents != null)
                newMesh.SetTangents(meshTangents);

            newMesh.bounds = bounds;
            newMesh.name = name;

            if (facesAndNeighborsRef != null)
                facesAndNeighborsRef.meshRef = newMesh;

            return newMesh;
        }

        /// <summary>
        /// Recalculate bounding box from vertices only. Safe off-thread.
        /// </summary>
        public void RecalculateBounds()
        {
            if (vertices == null || vertices.Length == 0)
                return;

            Vector3 min = vertices[0];
            Vector3 max = vertices[0];

            for (int i = 1; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                if (v.x < min.x) min.x = v.x;
                if (v.y < min.y) min.y = v.y;
                if (v.z < min.z) min.z = v.z;
                if (v.x > max.x) max.x = v.x;
                if (v.y > max.y) max.y = v.y;
                if (v.z > max.z) max.z = v.z;
            }

            bounds = new Bounds((min + max) * 0.5f, max - min);
        }

        /// <summary>
        /// Recalculates vertex normals using the triangle list and vertex positions.
        /// Safe to run off the Unity main thread.
        /// </summary>
        public void RecalculateNormals()
        {
            if (vertices == null || triangles == null)
                return;

            if (meshNormals == null || meshNormals.Length != vertices.Length)
                meshNormals = new Vector3[vertices.Length];

            // Zero normals
            for (int i = 0; i < meshNormals.Length; i++)
            {
                meshNormals[i] = Vector3.zero;
            }

            // Accumulate face normals
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int i0 = triangles[i];
                int i1 = triangles[i + 1];
                int i2 = triangles[i + 2];

                Vector3 v0 = vertices[i0];
                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];

                Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);

                meshNormals[i0] += normal;
                meshNormals[i1] += normal;
                meshNormals[i2] += normal;
            }

            // Normalize accumulated normals
            for (int i = 0; i < meshNormals.Length; i++)
            {
                meshNormals[i] = meshNormals[i].normalized;
            }
        }


        /// <summary>
        /// Constructor that copies data from a Unity Mesh.
        /// Only legal to call on the main thread.
        /// </summary>
        /// <param name="mesh">Source mesh.</param>
        public MeshData(Mesh mesh)
        {
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            indexFormat = mesh.indexFormat;
            vertices = mesh.vertices;
            triangles = mesh.triangles;
            meshNormals = mesh.normals;
            meshUV0s = mesh.uv;
            meshUV1s = mesh.uv2;
            meshUV2s = mesh.uv3;
            meshColors = mesh.colors;
            meshTangents = mesh.tangents;
            bounds = mesh.bounds;
            name = mesh.name;
        }

        /// <summary>Constructs an empty instance.</summary>
        public MeshData()
        {
        }
    }


    /*
    /// <summary>
    /// Stores mesh information in a class that can be instantiated and used outside of the main unity thread.
    /// </summary>
    public class MeshData
    {
        public UnityEngine.Rendering.IndexFormat indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
        public Vector3[] vertices;
        public int[] triangles;
        public Vector3[] meshNormals;

        public Vector2[] meshUV0s;
        public Vector2[] meshUV1s;
        public Vector2[] meshUV2s;
        public FacesAndNeighbors facesAndNeighborsRef;
        public string name;
        public Mesh ToMesh()
        {
            Mesh newMesh = new Mesh();
            if(vertices.Length>=0xFFFF)
                newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            else
                newMesh.indexFormat = indexFormat;
            newMesh.SetVertices(vertices);
            newMesh.SetTriangles(triangles, 0);
            newMesh.SetNormals(meshNormals);
            newMesh.SetUVs(0, meshUV0s);
            newMesh.SetUVs(1, meshUV1s);
            newMesh.SetUVs(2, meshUV2s);

            newMesh.name = name;
            if (facesAndNeighborsRef != null)
                facesAndNeighborsRef.meshRef = newMesh;

            return newMesh;
        }

        public int vertexCount => vertices.Length;

        public MeshData(Mesh mesh)
        {
            if (mesh == null) throw new System.ArgumentNullException(nameof(mesh));

            indexFormat = mesh.indexFormat;
            vertices = mesh.vertices;
            triangles = mesh.triangles;
            meshNormals = mesh.normals;

            meshUV0s = mesh.uv;
            meshUV1s = mesh.uv2;
            meshUV2s = mesh.uv3;

            name = mesh.name;
        }
        public MeshData()
        { }
    }*/
}