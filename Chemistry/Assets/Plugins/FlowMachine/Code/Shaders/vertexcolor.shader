Shader "Vertex Colors" {
	SubShader {
		Lighting Off
		CGPROGRAM
		#pragma surface surf Lambert
		struct Input {
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			o.Emission = IN.color;
		}
		ENDCG

	}
}