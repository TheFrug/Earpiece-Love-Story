Shader "Custom/Text/RedGlassesText"
{
    Properties
    {
        _FaceColor ("Face Color", Color) = (1,1,1,1)
        _MainTex ("Font Atlas", 2D) = "white" {}
        
        _GradientScale ("Gradient Scale", Float) = 5.0
        _ScaleRatioA ("Scale Ratio A", Float) = 1.0
        _ScaleRatioB ("Scale Ratio B", Float) = 1.0
        _ScaleRatioC ("Scale Ratio C", Float) = 1.0
        
        _NoiseScale ("Pixel Scale", Float) = 1.0
        _NoiseSeed ("Noise Seed", Vector) = (12.9898, 78.233, 43758.5453, 0)
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }

        Cull Off
        Lighting Off 
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One Zero

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _GradientScale;
            float _ScaleRatioA;
            
            float _NoiseScale;
            float3 _NoiseSeed;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            float hash(float2 p)
            {
                return frac(sin(dot(p, _NoiseSeed.xy)) * _NoiseSeed.z);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Read the Signed Distance Field from the font atlas
                float sd = tex2D(_MainTex, i.texcoord).a;
                
                // Convert SDF to crisp alpha using standard TMP math
                float sdScale = _GradientScale * _ScaleRatioA;
                float textAlpha = clamp((sd - 0.5) * sdScale + 0.5, 0.0, 1.0);

                // Normalize screen coordinates
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                
                // Scale by screen params to get pixel coordinates
                float2 pixelPos = screenUV * _ScreenParams.xy;
                
                // Apply custom scaling
                pixelPos = floor(pixelPos / max(1.0, _NoiseScale));

                // Generate independent noise for Green and Blue channels
                float noiseR = hash(pixelPos);
                float noiseG = hash(pixelPos + float2(100.0, 100.0));
                float noiseB = hash(pixelPos + float2(200.0, 200.0));

                float channelR = noiseR;
                if (textAlpha > 0.9) channelR = 0.85    ;
                else if (textAlpha > 0.1) channelR = 0.6;

                return fixed4(channelR, noiseG, noiseB, 1.0);
            }
            ENDCG
        }
    }
}

