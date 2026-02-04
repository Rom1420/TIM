Shader "Custom/OverlayTint"
{
    Properties
    {
        _OverlayColor ("Overlay Color", Color) = (0,1,0,0.35)
    }
    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        // Aide contre le z-fighting
        Offset -1, -1

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _OverlayColor;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _OverlayColor;
            }
            ENDCG
        }
    }
}
