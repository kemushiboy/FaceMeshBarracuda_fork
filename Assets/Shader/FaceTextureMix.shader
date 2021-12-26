Shader "Hidden/MediaPipe/FaceMesh/FaceTextureMix"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                uint vid : SV_VertexID;
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
              //float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            StructuredBuffer<float4> _Vertices;

            v2f vert (appdata v)
            {
                v2f o;

                //原点を合わせる
                //(-0.5,-0.5)平行移動する行列
                float4x4 t ={
                1,0,0,-0.5,
                0,1,0,-0.5,
                0,0,1,0,
                0,0,0,1
                };

                o.vertex = float4(v.uv,0,1);

                float2 uv;
                if(v.vid < 180)
                    uv = float2( o.vertex.x/2, o.vertex.y);
                else
                    uv = float2(0.5+ o.vertex.x/2, o.vertex.y);

                o.uv = TRANSFORM_TEX(uv, _MainTex);

                 o.vertex = UnityObjectToClipPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                return col;
            }
            ENDCG
        }
    }
}
