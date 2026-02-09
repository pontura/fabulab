Shader "Custom/SpriteOutlineUnity6"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size (px)", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Sprite"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            float4 _OutlineColor;
            float _OutlineSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // Si el pixel existe, dibujarlo normal
                if (col.a > 0.01)
                    return col;

                // Calcular texel size manualmente
                float2 texel = float2(
                    1.0 / _ScreenParams.x,
                    1.0 / _ScreenParams.y
                ) * _OutlineSize;

                float alpha = 0;
                alpha += tex2D(_MainTex, i.uv + float2( texel.x, 0)).a;
                alpha += tex2D(_MainTex, i.uv + float2(-texel.x, 0)).a;
                alpha += tex2D(_MainTex, i.uv + float2(0,  texel.y)).a;
                alpha += tex2D(_MainTex, i.uv + float2(0, -texel.y)).a;

                if (alpha > 0)
                    return fixed4(_OutlineColor.rgb, _OutlineColor.a);

                return col;
            }
            ENDCG
        }
    }
}
