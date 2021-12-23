using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace MediaPipe.FaceMesh
{
    public class FaceSwap : MonoBehaviour
    {

        [SerializeField] PipeLineManager _pipeline = null;
        [SerializeField] FaceMesh _faceMesh = null;
        [SerializeField] FaceMeshTransformed _faceMeshTransformed = null;

        [SerializeField] RenderTexture _faceUVMappedRT = null;
        [SerializeField] RenderTexture _faceSwappedRT = null;
        //[SerializeField] Texture _swapFaceTexture = null;
        //[SerializeField] Material _material = null;
        //[SerializeField] Material _material2 = null;
        [SerializeField] Texture _texture0_1 = null;
        [SerializeField] Texture _texture1_0 = null;
        [SerializeField] Texture _texture1_1 = null;

        [Space]
        [SerializeField] Texture[] splitFaces;


        CompositeTexture _composite;

        // Start is called before the first frame update
        void Start()
        {
            _composite = new CompositeTexture();
        }

        // Update is called once per frame
        void Update()
        {
            //mesh情報をアップデート
            _faceMesh.UpdateMesh(_pipeline.RefinedFaceVertexBuffer);//Refinedを使うのでCropMatrix不要
            _faceMeshTransformed.UpdateMesh(_pipeline.RawFaceVertexBuffer);//用意されたmeshに貼り付けるのでrefinedだとだめ

            //RenderTextureにFaceTextureを書き込み
            _faceMeshTransformed.Draw(_pipeline.CroppedFaceTexture);

            //renderTextureと取り込んだテクスチャを合成;
            Graphics.CopyTexture(_faceUVMappedRT, _faceSwappedRT);

            int index = 0;

            foreach(Texture splitFace in splitFaces)
            {
                _composite.Composite(_faceSwappedRT, splitFace, 5,5,index);
                index++;
            }

            //_composite.Composite(_faceSwappedRT, _texture0_1, 0, 0.5f, 0.5f, 1);
            //_composite.Composite(_faceSwappedRT, _texture1_0, 0.5f, 1f, 0, 0.5f);
           // _composite.Composite(_faceSwappedRT, _texture1_1, 0.5f, 1, 0.5f, 1);

            //合成結果をメッシュ上に描画
            _faceMesh.Draw(_faceSwappedRT);
        }


        public void SaveTexture()
        {
            //System.DateTime UnixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            //  long now = (long)(System.DateTime.Now - UnixEpoch).TotalSeconds;

            //  string filePath = "Assets/" + now + ".png";
            //  Debug.Log(filePath);
            //  TextureController.SaveImage(_faceUVMappedRT, filePath);

            Texture2D[] splitTexture = TextureController.Split(_faceUVMappedRT, 5, 5);
            TextureController.SaveImages(splitTexture, "Assets/SplitFaces");
        }
    }
}