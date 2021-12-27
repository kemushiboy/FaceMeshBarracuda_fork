using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MediaPipe.FaceMesh
{

    public class FaceMeshMixed : MonoBehaviour
    {
        [SerializeField] Shader _transformShader;
        [SerializeField] ResourceSet _resource;
        [SerializeField] Camera _camera = null;

        Mesh _mesh;
       
        Material _material;
        Material _material2;
        List<int> _triangles;

        void Start()
        {

            _material = new Material(_transformShader);

            _material2 = new Material(_transformShader);

            //mesh????
            _mesh = new Mesh();
            _mesh.SetVertices(_resource.faceMeshTemplate.vertices);
            _mesh.SetUVs(0, _resource.faceMeshTemplate.uv);

            _mesh.subMeshCount = 2;
            _triangles = new();
            int trianglesCount = 440;

            //Triangles???
            _triangles.AddRange(_resource.faceMeshTemplate.GetTriangles(0));
            Debug.Log(_triangles.Count);
            _mesh.SetTriangles(_triangles.GetRange(0, _triangles.Count - trianglesCount*3), 0);
            _mesh.SetTriangles(_triangles.GetRange(_triangles.Count - trianglesCount*3, trianglesCount*3), 1);
        }

        public void UpdateMesh(ComputeBuffer vertexBuffer)
        {
            try
            {
                _material.SetBuffer("_Vertices", vertexBuffer);
            }
            catch
            {

            }
        }

        public void Draw(Texture texture, Texture texture2)
        {
            _material.SetTexture("_MainTex", texture);

            _material2.SetTexture("_MainTex", texture2);

            Graphics.DrawMesh(_mesh, transform.position, transform.rotation, _material, 0, _camera,0);

            Graphics.DrawMesh(_mesh, transform.position, transform.rotation, _material2, 0, _camera, 1) ;

        }
    }
}