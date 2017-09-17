Shader "Cg texturing with alpha blending" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	}

		SubShader{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "False" }
		LOD 10
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Lighting Off
		Pass{
		SetTexture[_MainTex]{
		constantColor[_Color]
		combine texture * constant
	}
	}
	}  
}