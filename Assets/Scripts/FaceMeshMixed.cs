using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MediaPipe.FaceMesh
{

    public class FaceMeshMixed : MonoBehaviour
    {
        [SerializeField] Shader _shader;
        [SerializeField] ResourceSet _resource;
        [SerializeField] Camera _camera = null;

        Mesh _mesh;
       
        Material[] _materials;
        List<int> _triangles;

        void Start()
        {
            //material???
            _materials = new Material[4];

            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i] = new Material(_shader);
            }


            //mesh????
            _mesh = new Mesh();
            _mesh.SetVertices(_resource.faceMeshTemplate.vertices);
            _mesh.SetUVs(0, _resource.faceMeshTemplate.uv);

            _mesh.subMeshCount = _materials.Length;
            _triangles = new();
            int trianglesCount = (_resource.faceMeshTemplate.triangles.Length/3)/_materials.Length;

            //Triangles???
            _triangles.AddRange(_resource.faceMeshTemplate.GetTriangles(0));

            for (int i = 0; i < _materials.Length; i++)
            {
                _mesh.SetTriangles(_triangles.GetRange(i * trianglesCount * 3, trianglesCount * 3), i);

            }
        }

        public void UpdateMesh(ComputeBuffer vertexBuffer)
        {
            try
            {
                foreach(Material mat in _materials)
                {
                  mat.SetBuffer("_Vertices", vertexBuffer);
                }
            }
            catch
            {

            }
        }

        public void Draw(Texture[] textures)
        {
            int index = 0;
            foreach (Texture texture in textures)
            {
                _materials[index].SetTexture("_MainTex", texture);

                Graphics.DrawMesh(_mesh, transform.position, transform.rotation, _materials[index], 0, _camera, index);

                index++;
            }

        }
    }
}