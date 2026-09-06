Shader "OrbitBreaker/BackgroundSector"
{
    Properties
    {
        [PerRendererData] _MainTex ("Background", 2D) = "white" {}
        _HueShift ("Sector hue", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            sampler2D _MainTex;
            float _HueShift;
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color; return o; }
            fixed4 frag(v2f i):SV_Target
            {
                float4 c=tex2D(_MainTex,i.uv);
                float angle=_HueShift*6.2831853;
                float cs=cos(angle), sn=sin(angle);
                float3 axis=normalize(float3(1,1,1));
                c.rgb=saturate(c.rgb*cs+cross(axis,c.rgb)*sn+axis*dot(axis,c.rgb)*(1-cs));
                return c*i.color;
            }
            ENDCG
        }
    }
}
