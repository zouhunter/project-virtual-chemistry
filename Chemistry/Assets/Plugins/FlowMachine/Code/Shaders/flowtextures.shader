// flow machine was created by Serious Games Interactive 2011
// Created with Unity version 3.3.0f4
//
// Original idea, preliminary coding and initial Js implementation: 	Sam Hagelund
// C# rewrite and optimizations:										Christian Franzen
//

Shader "Env/Water FlowProbes" {
Properties {
	_Color ("Main Color", Color ) = ( 0.5,0.5,0.5,1.0 )
	_SpecColor ("Specular Color", Color) = (1, 1, 1, 0)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_MainTex ("Main Texture", 2D ) = "" {}
	_BumpMap ("Normals", 2D ) = "bump" {}
	_NoiseMap ("NoiseMap", 2D ) = "" {}
	_FlowSpeed ("Flow Speed", Float ) = 0.3
	_NormalStrenght("Bump Strenght", Range(0,1) ) = 1
	_Bias("Depth Bias", Range(0,4) ) = 0.5
	_Narrow("Depth Narrowness", Range(1,4) ) = 0.5
	_WaveH ("Wave Height", Float ) = 0.1
}
/////////////////////////////////
//// SHADER MODEL 3.0
/////////////////////////////////
SubShader {
Tags { "RenderType"="Transparent" "Queue"="Transparent" }
CGPROGRAM
#pragma surface surf BlinnPhong vertex:vert alpha
#pragma target 3.0

struct Input {
	float4 color : COLOR;
	float2 uv_MainTex;
	float2 uv_NoiseMap;
	float2 uv_BumpMap;
	float3 worldRefl;
	INTERNAL_DATA
	float4 screenPos;
};

uniform sampler2D _MainTex, _NoiseMap, _BumpMap;
sampler2D _CameraDepthTexture;
uniform float4 _Color;
uniform float _FlowSpeed, _WaveH, _Shininess, _NormalStrenght, _Bias, _Narrow;

// Fast sincos
float SmoothCurve( float x ) {  
  return x * x *( 3.0 - 2.0 * x );  
}  
float TriangleWave( float x ) {  
  return abs( frac( x + 0.5 ) * 2.0 - 1.0 );  
}  
float SmoothTriangleWave( float x ) {  
  return SmoothCurve( TriangleWave( x ) );  
}

void vert (inout appdata_full v) {
	
	// Wave animation
	float time = _Time.y * _FlowSpeed;
	float vhpos = v.vertex.x + v.vertex.z;
	float hoffset = SmoothTriangleWave( time + vhpos * 0.125 );
	hoffset += SmoothTriangleWave( time + vhpos * 0.361 );
	hoffset += SmoothTriangleWave( time + vhpos * 0.076 );
	
	v.vertex.y += hoffset * _WaveH * _FlowSpeed;
	
	v.normal = mul((float3x3)_World2Object, float3(0,1,0));
}

void surf (Input IN, inout SurfaceOutput o) {
	
	// Unpack flow direction
	float2 vcol = IN.color.xz * 2 - 1;
	
	// Noise
	half noise = tex2D( _NoiseMap, IN.uv_NoiseMap ).x * 0.33;
	
	// Flow layers
	float layer1 = fmod( _Time.y * _FlowSpeed + noise, 1.0 );
	float layer2 = fmod( _Time.y * _FlowSpeed + noise + 0.5, 1.0 );
	
	// Layer blend factor
	float layerblend = abs( layer1 * 2 - 1 );
	
	// Layer uvs
	float2 uv1 = vcol * layer1 * 0.5;
	float2 uv2 = vcol * layer2 * 0.5;
	
	// Diffuse
	float3 tex1 = tex2D( _MainTex, IN.uv_MainTex + uv1 ).xyz;
	float3 tex2 = tex2D( _MainTex, IN.uv_MainTex + uv2 ).xyz;
	float3 texcol = lerp( tex1, tex2, layerblend );
	
	// Bump
	float4 bump1 = tex2D( _BumpMap, IN.uv_BumpMap + uv1 );
	float4 bump2 = tex2D( _BumpMap, IN.uv_BumpMap + uv2 );
	float4 normal = lerp( bump1, bump2, layerblend );
	float3 normal2 = UnpackNormal(normal);
	normal2 = lerp( float3( 0,0,1 ), normal2, _NormalStrenght );
	
	// Alpha
	float alpha = IN.color.a + 0.25;
	
	//Depth transparency
	float4 cameraDepth = 1;
	float4 ScreenDepthDiff= (LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos)).r) - IN.screenPos.z)*_Narrow;
	float4 Pow=pow(ScreenDepthDiff,_Bias.xxxx);
	float4 Saturate=saturate(Pow);
		
	// Foaming
	half foam = dot( IN.color.xy, 0.5 );
	foam = saturate( foam * foam );
	foam = pow( foam, 3 ) * 8;
	
	o.Albedo = lerp( _Color.rgb + texcol * foam, texcol * foam, foam );
	o.Gloss = texcol.r;
	o.Specular = _Shininess;
	o.Alpha = Saturate.r * _Color.a;
	o.Normal = normal2;
}
ENDCG
}
/////////////////////////////////
//// SHADER MODEL 2.0
/////////////////////////////////
SubShader {
Tags { "RenderType"="Transparent" "Queue"="Transparent" }
CGPROGRAM
#pragma surface surf BlinnPhong vertex:vert alpha

struct Input {
	float4 color : COLOR;
	float2 uv_MainTex;
	float2 uv_NoiseMap;
};

uniform sampler2D _MainTex, _NoiseMap;
uniform float4 _Color;
uniform float _FlowSpeed, _WaveH;

float4 SmoothCurve( float x ) {  
  return x * x *( 3.0 - 2.0 * x );  
}  
float4 TriangleWave( float x ) {  
  return abs( frac( x + 0.5 ) * 2.0 - 1.0 );  
}  
float4 SmoothTriangleWave( float x ) {  
  return SmoothCurve( TriangleWave( x ) );  
}

void vert (inout appdata_full v) {
	
	// Wave animation
	float time = _Time.y * _FlowSpeed;
	float vhpos = v.vertex.x + v.vertex.z;
	float hoffset = SmoothTriangleWave( time + vhpos * 0.775 );
	hoffset += SmoothTriangleWave( time + vhpos * 0.361 );
	hoffset += SmoothTriangleWave( time + vhpos * 0.076 );
	
	v.vertex.y += hoffset * _WaveH * _FlowSpeed;
	
	v.normal = mul((float3x3)_World2Object, float3(0,1,0));
}

void surf (Input IN, inout SurfaceOutput o) {
	// Unpack flow direction
	float2 vcol = IN.color.xz * 2 - 1;
	
	// Noise
	half noise = tex2D( _NoiseMap, IN.uv_NoiseMap ) * 0.33;
	
	// Flow layers
	float layer1 = fmod( _Time.y * _FlowSpeed + noise, 1.0 );
	float layer2 = fmod( _Time.y * _FlowSpeed + 0.5 + noise, 1.0 );
	
	// Layer blend factor
	float layerblend = abs( layer1 * 2 - 1 );
	
	// Layer uvs
	float2 uv1 = vcol * layer1 * 0.5;
	float2 uv2 = vcol * layer2 * 0.5;
	
	// Diffuse
	float3 tex1 = tex2D( _MainTex, IN.uv_MainTex + uv1 ).xyz;
	float3 tex2 = tex2D( _MainTex, IN.uv_MainTex + uv2 ).xyz;
	float3 texcol = lerp( tex1, tex2, layerblend );
	
	// Foaming
	half foam = dot( IN.color.xy, 0.5 );
	foam = saturate( foam * foam );
	
	//o.Emission = foam;
	o.Albedo = lerp( _Color.rgb + texcol * foam, texcol * foam, foam );
	o.Gloss = texcol.r;
	o.Specular = 0.078125;
	o.Alpha = IN.color.a + 0.5;
}
ENDCG
}
Fallback "Diffuse"
}