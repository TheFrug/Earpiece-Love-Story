Shader "Hidden/NewImageEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseScale ("Pixel Scale", Float) = 1.0
        _NoiseSeed ("Noise Seed", Vector) = (12.9898, 78.233, 43758.5453, 0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            float _NoiseScale;
            float3 _NoiseSeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float hash(float2 p)
            {
                return frac(sin(dot(p, _NoiseSeed.xy)) * _NoiseSeed.z);
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float2 pixelPos = i.uv * _ScreenParams.xy;
                
                col.r = hash(pixelPos);
                col.g = hash(pixelPos + float2(100.0, 100.0));
                col.b = hash(pixelPos + float2(200.0, 200.0));

                return col;
            }
            ENDCG
        }
    }
}
