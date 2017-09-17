// flow machine was created by Serious Games Interactive 2011
// Created with Unity version 3.3.0f4
//
// Original idea, preliminary coding and initial Js implementation: 	Sam Hagelund
// C# rewrite and optimizations:										Christian Franzen
//

Shader "Env/Water FlowEngine" {
Properties {
	_Color ("Main Color", Color ) = ( 0.5,0.5,0.5,1.0 )
	_SpecColor ("Specular Color", Color) = (1, 1, 1, 1) 
	_MainTex ("Main Texture", 2D ) = "" {}
	_BumpMap ("Normals", 2D ) = "bump" {}
	_NoiseMap ("NoiseMap", 2D ) = "" {}
	_FlowSpeed ("Flow Speed", Float ) = 1.0
}
/////////////////////////////////
//// SHADER MODEL 3.0
/////////////////////////////////
SubShader {
Tags { "RenderType"="Opaque" "Queue"="Transparent" }
Colormask RGB
CGPROGRAM
#pragma target 3.0
#pragma surface surf BlinnPhong vertex:vert alpha

struct Input {
	float4 color : COLOR;
	float2 uv_MainTex;
	float2 uv_BumpMap;
	float2 uv_NoiseMap;
	float4 screenPos;
};

void vert (inout appdata_full v) {
	float3 poslimit = mul(_World2Object, float4(0,300,0,1)).xyz;
	v.vertex.y += mul((float3x3)_World2Object, float3(0,1,0)).y * v.color.b * 8.5;
	v.vertex.y = min( v.vertex.y, poslimit.y );
	v.normal = mul((float3x3)_World2Object, float3(0,1,0));
}

uniform sampler2D _MainTex, _BumpMap, _NoiseMap;
uniform float4 _Color;
uniform float _FlowSpeed;

void surf (Input IN, inout SurfaceOutput o) {
	// Unpack flow direction
	float2 vcol = IN.color.xy * 2 - 1;
	//vcol.x = -vcol.x;
	vcol.xy *= 0.3;
	// Noise
	half noise = tex2D( _NoiseMap, IN.uv_NoiseMap ) * 0.4;
	// Flow layers
	float layer1 = fmod( _Time.y * _FlowSpeed + noise, 1.0 );
	float layer2 = fmod( _Time.y * _FlowSpeed + noise + 0.5, 1.0 );
	
	// Layer blend factor
	float layerblend = abs( layer1 * 2 - 1 );
	
	// Layer uvs
	float2 uv1 = vcol * layer1;
	float2 uv2 = vcol * layer2;
	
	// Diffuse
	float3 tex1 = tex2D( _MainTex, IN.uv_MainTex + uv1 ).xyz;
	float3 tex2 = tex2D( _MainTex, IN.uv_MainTex + uv2 ).xyz;
	float3 texcol = lerp( tex1, tex2, layerblend );
	
	// Base normal
	float4 norm1 = tex2D( _BumpMap, IN.uv_BumpMap + uv1 );
	float4 norm2 = tex2D( _BumpMap, IN.uv_BumpMap + uv2 );
	float3 normal = UnpackNormal( lerp( norm1, norm2, layerblend ));
	
	// Alpha blend
	half alpha = IN.color.z * 10;
	
	// Foaming
	half foam = dot( abs(IN.color.xy * 2 - 1), 0.5 );
	foam = saturate( foam * 4.5 );
	
	//o.Emission = foam;
	o.Albedo = lerp( _Color.rgb + texcol * foam, texcol * foam, foam );
	o.Normal = normal * float3( foam, foam, 1.0 );
	o.Gloss = 1.0;
	o.Specular = 0.4;
	o.Alpha = IN.color.b * 80;
}
ENDCG
}
/////////////////////////////////
//// SHADER MODEL 2.0
/////////////////////////////////
//SubShader {
//Tags { "RenderType"="Opaque" }
//CGPROGRAM
//#pragma surface surf BlinnPhong
//
//struct Input {
//	float4 color : COLOR;
//	float2 uv_MainTex;
//	float2 uv_NoiseMap;
//};
//
//void vert (inout appdata_full v) {
//	float3 poslimit = mul(_World2Object, float4(0,30,0,1)).xyz;
//	v.vertex.y += mul((float3x3)_World2Object, float3(0,1,0)).y * v.color.b * 100;
//	v.vertex.y = min( v.vertex.y, poslimit.y );
//	v.normal = mul((float3x3)_World2Object, float3(0,1,0));
//}
//
//uniform sampler2D _MainTex, _NoiseMap;
//uniform float4 _Color;
//uniform float _FlowSpeed;
//
//void surf (Input IN, inout SurfaceOutput o) {
//	// Unpack flow direction
//	float2 vcol = IN.color.xy * 2 - 1;
//	vcol.x = -vcol.x;
//	
//	// Noise
//	half noise = tex2D( _NoiseMap, IN.uv_NoiseMap ) * 0.4;
//	
//	// Flow layers
//	float layer1 = fmod( _Time.y * _FlowSpeed + noise, 1.0 );
//	float layer2 = fmod( _Time.y * _FlowSpeed + 0.5 + noise, 1.0 );
//	
//	// Layer blend factor
//	float layerblend = abs( layer1 * 2 - 1 );
//	
//	// Layer uvs
//	float2 uv1 = vcol * layer1 * 0.5;
//	float2 uv2 = vcol * layer2 * 0.5;
//	
//	// Diffuse
//	float3 tex1 = tex2D( _MainTex, IN.uv_MainTex + uv1 ).xyz;
//	float3 tex2 = tex2D( _MainTex, IN.uv_MainTex + uv2 ).xyz;
//	float3 texcol = lerp( tex1, tex2, layerblend );
//	
//	// Foaming
//	half foam = dot( IN.color.xy, 0.5 );
//	foam = saturate( foam * foam );
//	
//	//o.Emission = foam;
//	o.Albedo = lerp( _Color.rgb + texcol * foam, texcol * foam, foam );
//	o.Gloss = texcol.r;
//	o.Specular = 0.9;
//	o.Alpha = 1.0;
//}
//ENDCG
//}
//Fallback "Diffuse"
}