Shader "Custom/PixelArtShader"
{
    Properties
    {
        _PixelSize ("Pixel Size", Float) = 8.0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float _PixelSize;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 frag (v2f i) : SV_Target
            {
                float2 uv = floor(i.uv * _PixelSize) / _PixelSize;
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
