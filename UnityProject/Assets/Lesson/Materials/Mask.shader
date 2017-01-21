Shader "Tutorial/Textured Colored" {
    Properties {
        //_Color ("Main Color", Color) = (1,1,1,0.5)
        _Main ("Texture", 2D) = "white" { }
        _AlphaTex ("Alpha mask (R)", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range (0,1)) = 0.1 
    }
    SubShader {
    
    	Blend SrcAlpha OneMinusSrcAlpha
    
        Pass {

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"

        float _Cutoff;
        sampler2D _Main;
        sampler2D _AlphaTex;

        struct v2f {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        float4 _Main_ST;

        v2f vert (appdata_base v)
        {
            v2f o;
            o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
            o.uv = TRANSFORM_TEX (v.texcoord, _Main);
            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            fixed4 t = tex2D (_Main, i.uv);
            fixed4 alpha = tex2D (_AlphaTex, i.uv);
            bool show = alpha.a > _Cutoff;
            return fixed4(t.r, t.g, t.g, show ? t.a : 0);
        }
        ENDCG

        }
    }
}