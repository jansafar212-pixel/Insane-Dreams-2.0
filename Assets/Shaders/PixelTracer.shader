Shader "AKUMU/PixelTracer"
{
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION; half4 color:COLOR; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; half4 color:COLOR; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS=TransformObjectToHClip(input.positionOS.xyz);
                output.color=input.color;output.uv=input.uv;return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                // Two hard bands: pale core, restrained colored fringe. One unlit pass;
                // existing vertex alpha provides the short temporal/directional tail fade.
                half core=step(abs(input.uv.y-.5),.22);
                half3 rgb=lerp(input.color.rgb,lerp(input.color.rgb,half3(1,1,.94),.72),core);
                return half4(rgb,input.color.a*lerp(.2,1,core));
            }
            ENDHLSL
        }
    }
}
