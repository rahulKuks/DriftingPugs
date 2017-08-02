Shader "KleVRelk/Shield distorsion" {
Properties {
        _MainTex ("Texture (R,G=X,Y Distortion; B=Mask; A=Unused)", 2D) = "white" {}
        _IntensityAndScrolling ("Intensity (XY); Scrolling (ZW)", Vector) = (0.1,0.1,1,1)
        [Toggle(MASK)] _MASK ("Texture Blue channel is Mask", Float) = 0
        [Toggle(DEBUGUV)] _DEBUGUV ("Debug Texture Coordinates", Float) = 0
        _Color ("Color", Color) = (1, 1, 1, 1)
        _RimEffect ("Rim effect", Range(0, 1)) = 0
}
 
SubShader {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True"}
        Lighting Off
        Fog { Mode Off }
        ZWrite Off
        LOD 200
   
        // See http://docs.unity3d.com/Manual/SL-GrabPass.html
        GrabPass { "_GrabTexture" }
 
    Pass {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend One One
        Cull Off
        ZWrite Off

        CGPROGRAM
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature MASK
            #pragma shader_feature DEBUGUV
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _GrabTexture;
       
            
            float4 _IntensityAndScrolling;
               
            struct appdata_t {
                float4 vertex  : POSITION;
                half2 texcoord : TEXCOORD0;
                float3 normal : NORMAL;
            };
 
            struct v2f {
                float4 vertex  : SV_POSITION;
                half2 texcoord : TEXCOORD0;
                half2 screenuv : TEXCOORD1;
                float3 normal : NORMAL;
                float3 viewDir : TEXCOORD3;
            #if MASK
                half2 maskuv   : TEXCOORD2;
            #endif
            };
       
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
           
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex); 
                o.texcoord += _Time.gg * _IntensityAndScrolling.zw; 
                o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal.xyz));
                o.viewDir = normalize(_WorldSpaceCameraPos - mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));
                
            #if MASK
               
                o.maskuv = v.texcoord;
            #endif
                           
                half4 screenpos = ComputeGrabScreenPos(o.vertex);
                o.screenuv = screenpos.xy / screenpos.w;
                return o;
            }
            fixed4 _Color;
            fixed _RimEffect;   
            fixed4 frag (v2f i) : COLOR
            {      
                half2 distort = tex2D(_MainTex, i.texcoord).xy;
           
                // distort*2-1 transforms range from 0..1 to -1..1.
                // negative values move to the left, positive to the right.
                half2 offset = (distort.xy * 2 - 1) * _IntensityAndScrolling.xy;
                       
            #if MASK
                
                half  mask = tex2D(_MainTex, i.maskuv).b;          
                offset *= mask;
            #endif                      
                                                                           
                
                half2 uv = i.screenuv + offset;
                half4 color = tex2D(_GrabTexture, uv);
                UNITY_OPAQUE_ALPHA(color.a);
           
           
            #if DEBUGUV
                color.rg = uv;
                color.b = 0;
            #endif
                float t = tex2D(_MainTex, i.texcoord);
                float val = 1 - abs(dot(i.viewDir, i.normal)) * _RimEffect;
 
                return (_Color * _Color.a * val * val * t)+ color;
            }
        ENDCG
    }

}
}