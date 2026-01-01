Shader "Custom/TerrainShader"
{
    Properties{

		_TexArr("Textures", 2DArray) = "" {}
		_TexScale("Texture Scale", Float) = 1

	}

    SubShader
    {
        Tags {"Render Type" = "Opaque"}
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.5
        #pragma require 2darray

		UNITY_DECLARE_TEX2DARRAY(_TexArr);
		float _TexScale;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            float2 uv_TexArr;
            float2 uv_BumpMap;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 scaledWorldPos = IN.worldPos / _TexScale; 
			float3 pWeight = abs(IN.worldNormal); 
			pWeight /= pWeight.x + pWeight.y + pWeight.z; 

			int texIndex = floor(IN.uv_TexArr.x + 0.1);
			float3 projected; 

			projected = float3(scaledWorldPos.y, scaledWorldPos.z, texIndex);
			float3 xP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.x;

			projected = float3(scaledWorldPos.x, scaledWorldPos.z, texIndex);
			float3 yP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.y;

			projected = float3(scaledWorldPos.x, scaledWorldPos.y, texIndex);
			float3 zP = UNITY_SAMPLE_TEX2DARRAY(_TexArr, projected) * pWeight.z;

			o.Albedo = xP + yP + zP;
        }

        ENDCG
    }
    Fallback "Diffuse"
}
