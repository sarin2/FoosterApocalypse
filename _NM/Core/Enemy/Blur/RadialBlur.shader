Shader "Hidden/RadialBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurAmount ("Blur Amount", Range(0, 1)) = 0.1
        _BlurCenter ("Blur Center", Vector) = (0.5, 0.5, 0, 0)
        _BlurSampleCount ("Blur Sample Count", Range(1,99)) = 9
        _BlurSampleIntensity("Blur Sample Intensity", Float) = 0.1
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        ZTest Always 
        ZWrite Off 
        Cull Off

        Pass
        {
            Name "RadialBlur"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float _BlurAmount;
                float _BlurSampleIntensity;
                uint _BlurSampleCount;
                float4 _BlurCenter;
                float4 _MainTex_ST;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 dir = input.uv - _BlurCenter.xy;
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                uint sampleCount = (uint)_BlurSampleCount;
                
                for(uint sample = 0; sample < sampleCount / 2; ++sample)
                {
                    float curIntensity = 1.0;
                    
                    // Negative direction sample
                    float2 uvNeg = input.uv - dir * _BlurAmount * (curIntensity + (sample * _BlurSampleIntensity));
                    col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvNeg);
                    
                    // Positive direction sample
                    float2 uvPos = input.uv + dir * _BlurAmount * (curIntensity - (sample * _BlurSampleIntensity));
                    col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvPos);
                }
                
                col /= float(sampleCount);
                
                return col;
            }
            ENDHLSL
        }
    }
}