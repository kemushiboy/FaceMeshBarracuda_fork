using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System.Threading.Tasks;
using UnityEditor;


namespace MediaPipe.FaceMesh
{
    public class MixedFaceGenerator : MonoBehaviour
    {

        [SerializeField] PipeLineManager _pipeline = null;
        [SerializeField] FaceMesh _faceMesh = null;
        [SerializeField] FaceMeshTransformed _faceMeshTransformed = null;

        [SerializeField] Texture2D[] _faceImages;

        [SerializeField] RenderTexture _cameraInputTexture = null;
        [SerializeField] RenderTexture _faceUVMappedRT = null;
        [SerializeField] RenderTexture _faceSwappedRT = null;
        [SerializeField] Camera _faceTextureCamera = null;

        [Space]
        [SerializeField] Vector2Int grid;

        List<ImageData> _splitFacesData;

        List<CompositeTexture> _composites;

        CompositeTexture _compositeTexture;

        float swappedSize;

        float textureSize;

        public bool isSwapping { get; private set; }

        public bool isSwaped { get; private set; }

        int i, j;

        // Start is called before the first frame update
        void Start()
        {
            _composites = new List<CompositeTexture>();

            _composites.Add(new CompositeTexture());
            _composites.Add(new CompositeTexture());
            _composites.Add(new CompositeTexture());
            _composites.Add(new CompositeTexture());

            _compositeTexture = new();

            _splitFacesData = new();

            swappedSize = 0;

            textureSize = _faceSwappedRT.width * _faceSwappedRT.height;

            isSwapping = false;
            isSwaped = false;

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

        public void Generate()
        {

           for (int i = 0; i < grid.y; i++)
            {

                for (int j = 0; j < grid.x; j++)
                {

                    //顔画像を設定
                    Graphics.Blit(_faceImages[i * grid.x + j], _cameraInputTexture);
                    //ここが更新されない

                    _faceTextureCamera.Render();

                    //mesh情報をアップデート
                    _pipeline.ProcessImage();
                    //_faceMesh.UpdateMesh(_pipeline.RefinedFaceVertexBuffer);//Refinedを使うのでCropMatrix不要
                    _faceMeshTransformed.UpdateMesh(_pipeline.RawFaceVertexBuffer);//用意されたmeshに貼り付けるのでrefinedだとだめ
                    
                    //RenderTextureにFaceTextureを書き込み
                    _faceMeshTransformed.Draw(_pipeline.CroppedFaceTexture);

                    //顔の一部を合成
                    _compositeTexture.Composite(_faceSwappedRT, _faceUVMappedRT, grid.x, grid.y , i*grid.x + j);
                  
                }
            }



            //renderTextureと取り込んだテクスチャを合成;
            //Graphics.CopyTexture(_faceUVMappedRT, _faceSwappedRT);

            //int index = 0;

            //for (int i = 0; i < _splitFacesData.Count; i++)
            {
           //     _composites[i].Composite(_faceSwappedRT, _splitFacesData[i].texture, _splitFacesData[i].capturedData.rect);
            //    index++;
            }

            //合成結果をメッシュ上に描画
            _faceMesh.Draw(_faceSwappedRT);
        }

        public void Reset()
        {
            //テクスチャ選択をリセット、メモリ開放
            foreach (ImageData data in _splitFacesData)
            {
                data.Dispose();
            }
            _splitFacesData.Clear();

            foreach (CompositeTexture composite in _composites)
            {
                composite.Dispose();
            }
            _composites.Clear();

            swappedSize = 0;
            isSwaped = false;
            isSwapping = false;
        }

        //並列で繰り返しSelectTextureを実行する
        public async void SwapTextureParallel(int num)
        {
            Reset();
            isSwapping = true;

            List<Task> tasks = new List<Task>();

            //並列実行
            for(int i=0; i<num; i++)
            {
                tasks.Add(SwapTexture());
            }

            //すべて終わるまで待つ
            await Task.WhenAll(tasks.ToArray());

            isSwapping = false;
            isSwaped = true;

            Debug.Log("Finish Swapping.");
        }

        public async Task SwapTexture()//asyncで困ることがあるかも？
        {
            


            /*
            //入れ替えが一定の面積を占めるまで、テクスチャを入れ替える

            while (swappedSize < textureSize * _textureController._texturePercentage)
            {
                //保存されているデータを読み込み
                ImageData imageData = await _textureController._capturedDataManager.GetRandomData();

                //すでに同じデータを読み込んでいないか調べる
                bool isIdentical = true;
                foreach(ImageData data in _splitFacesData)
                {
                    if(imageData.capturedData.rect == data.capturedData.rect)
                    {
                        isIdentical = false;
                        break;
                    }
                }
                //もう読み込んだデータであれば処理をとばす
                if (!isIdentical)
                {
                    continue;
                }

                _splitFacesData.Add(imageData);

                //合成時のシェーダーエフェクトを実行
                CompositeTexture composite = new CompositeTexture();
                
                composite.StartSwaping();
      
                _composites.Add(composite);

                //置き換えられていないピクセル数を求める
                swappedSize = textureSize;

                for(int y=0; y<_faceSwappedRT.height; y+=block)
                {
                    for(int x=0; x<_faceSwappedRT.width; x+=block)
                    {
                        Rect identityRect = new Rect(x, y, block,block);

                        bool isSepareted = true;

                        foreach(ImageData data in _splitFacesData)
                        {
                            if (identityRect.Overlaps(data.capturedData.rect))
                            {
                                isSepareted = false;
                                break;
                            }
                        }

                        if (isSepareted)
                        {
                            swappedSize -= (block*block);
                        }
                    }
                }
            }*/
        }
    }
}