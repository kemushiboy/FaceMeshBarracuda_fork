using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System.IO;


namespace MediaPipe.FaceMesh
{
    public class MixedFaceGenerator : MonoBehaviour
    {

        [SerializeField] PipeLineManager _pipeline = null;
        [SerializeField] FaceMesh _faceMesh = null;
        [SerializeField] FaceMeshTransformed _faceMeshTransformed = null;

        [SerializeField] RenderTexture _cameraInputTexture = null;
        [SerializeField] RenderTexture _faceUVMappedRT = null;
        [SerializeField] RenderTexture _faceSwappedRT = null;
        [SerializeField] Camera _faceTextureCamera = null;

        [Space]
        [SerializeField] Vector2Int grid;

        CompositeTexture _compositeTexture;

        float swappedSize;

        float textureSize;

        string[] _imagesPath;

        // Start is called before the first frame update
        void Start()
        {

            _compositeTexture = new();

            swappedSize = 0;

            textureSize = _faceSwappedRT.width * _faceSwappedRT.height;

            _imagesPath = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "img_celeba"));

        }

        private void OnDestroy()
        {
            _faceUVMappedRT.Release();
            _faceSwappedRT.Release();
        }

        // Update is called once per frame
        void Update()
        {

            Generate();

        }

        IEnumerator GenerateCoroutine()
        {
            while (true)
            {
                Generate();
            }
        }

        public void Generate()
        {
            string name = "";
           for (int i = 0; i < grid.y; i++)
            {

                for (int j = 0; j < grid.x; j++)
                {

                    //画像をランダム読み込み
                    int index = UnityEngine.Random.Range(0, _imagesPath.Length);
                    string path = _imagesPath[index];
                    byte[] bytes = File.ReadAllBytes(path);
                    Texture2D texture = new Texture2D(2,2);
                    texture.LoadImage(bytes);

                    name += Path.GetFileNameWithoutExtension(path) + "_";//ファイル名良い感じに処理したい 

                    //顔画像を設定
                    Graphics.Blit(texture, _cameraInputTexture);

                    Object.Destroy(texture);

                    //mesh情報をアップデート
                    _pipeline.ProcessImage();
                    //_faceMesh.UpdateMesh(_pipeline.RefinedFaceVertexBuffer);//Refinedを使うのでCropMatrix不要
                    _faceMeshTransformed.UpdateMesh(_pipeline.RawFaceVertexBuffer);//用意されたmeshに貼り付けるのでrefinedだとだめ
                    
                    //RenderTextureにFaceTextureを書き込み
                    _faceMeshTransformed.Draw(_pipeline.CroppedFaceTexture);

                    //カメラ更新
                    _faceTextureCamera.Render();

                    //顔の一部を合成
                    _compositeTexture.Composite(_faceSwappedRT, _faceUVMappedRT, grid.x, grid.y , i*grid.x + j);

                    Debug.Log(i * grid.x + j);

                    
                  
                }
            }

            //合成結果をメッシュ上に描画
            _faceMesh.Draw(_faceSwappedRT);

            //画像保存
            Texture2D tex = new Texture2D(_faceSwappedRT.width, _faceSwappedRT.height, TextureFormat.RGBA32, false);
            RenderTexture.active = _faceSwappedRT;
            tex.ReadPixels(new Rect(0, 0, _faceSwappedRT.width, _faceSwappedRT.height), 0, 0);
            tex.Apply();

            byte[] outBytes = tex.EncodeToPNG();
            Object.Destroy(tex);
            File.WriteAllBytes(Path.Combine(Application.streamingAssetsPath, "Generated", TimeUtil.GetUnixTime(System.DateTime.Now).ToString()+".png"), outBytes);


          
        }

    }
}