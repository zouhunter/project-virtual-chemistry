Shader "Custom/Lines"
{
	SubShader
	{
		Pass
		{
		    LOD 200
			//Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			//ZTest Always
			Cull Off
			Fog { Mode Off }

			BindChannels
			{
			  Bind "vertex", vertex
			  Bind "color", color
			}
		}
	}
}