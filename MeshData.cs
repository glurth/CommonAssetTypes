using UnityEngine;
using EyE.UnityAssetTypes;
namespace EyE.Geometry
{
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
    }
}