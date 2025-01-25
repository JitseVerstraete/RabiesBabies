Shader "Custom/Fresnel" {
   Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _RimPower ("RimPower", Range(0,1)) = 0.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
    
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:blend

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
            float3 viewDir;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        half _RimPower;

        void surf(Input IN, inout SurfaceOutputStandard o) {

            // Albedo comes from a texture tinted by color
        fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
        o.Albedo = c.rgb;
        // Metallic and smoothness come from slider variables
        o.Metallic = _Metallic;
        o.Smoothness = _Glossiness;



        half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
                float4 Tex2D1 = tex2D(_MainTex, (IN.uv_MainTex.xyxy).xy);
                float4 Multiply0 = Tex2D1 * _Color;
                float4 Multiply2 = Multiply0 * _Color.a;
                if (_RimPower > 0)
                {
                    o.Emission = _Color.a * _Color.rgb * pow(rim, _RimPower);
                }

         o.Alpha = 0.5; // 50% transparent

     
         }
    
        ENDCG
    }
    FallBack "Diffuse"
}