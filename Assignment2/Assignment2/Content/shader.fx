float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float3 CameraPosition;

texture DecalMap;
bool hasTexture;
texture EnvironmentMap;
float Reflectivity; //percentage of reflected color vs original color
float Refractivity;

float3 EtaRatio;
float FresnelPower;
float FresnelScale;
float FresnelBias;

sampler tsampler1 = sampler_state {
	texture = <DecalMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

samplerCUBE SkyBoxSampler = sampler_state {
	texture = <EnvironmentMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VertexShaderInput {
	float4 Position: POSITION0;
	float2 TexCoord: TEXCOORD0;
	float4 Normal: NORMAL0;
};

struct VertexShaderOutput {
	float4 Position: POSITION0;
	float2 TexCoord: TEXCOORD0;
	float3 R: TEXCOORD1;
};

struct DispersionOutput {
	float4 Position: POSITION0;
	float3 R: TEXCOORD0;
	float ReflectionFactor : COLOR;
	float3 TRed: TEXCOORD1;
	float3 TGreen: TEXCOORD2;
	float3 TBlue: TEXCOORD3;
};

struct FresnelOutput {
	float4 Position: POSITION0;
	float3 R: TEXCOORD0;
	float3 T: TEXCOORD1;
	float rC : COLOR;
};

VertexShaderOutput ReflectionVSFunction(VertexShaderInput input) {
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.TexCoord = input.TexCoord;
	float3 N = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
	float3 I = normalize(worldPosition.xyz - CameraPosition);
	output.R = reflect(I, N);
	//output.R = refract(I, N, 0.90);
	return output;
}

float4 ReflectionPSFunction(VertexShaderOutput input) : COLOR0{
	float4 reflectedColor = texCUBE(SkyBoxSampler, input.R);
	if (hasTexture) {
		float4 decalColor = tex2D(tsampler1, input.TexCoord);
		return lerp(decalColor, reflectedColor, Reflectivity);
	}
	return reflectedColor;
}

VertexShaderOutput RefractionVSFunction(VertexShaderInput input) {
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.TexCoord = input.TexCoord;
	float3 N = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
	float3 I = normalize(worldPosition.xyz - CameraPosition);
	output.R = refract(I, N, 0.90);
	return output;
}

float4 RefractionPSFunction(VertexShaderOutput input) : COLOR0{
	float4 refractedColor = texCUBE(SkyBoxSampler, input.R);
	if (hasTexture) {
		float4 decalColor = tex2D(tsampler1, input.TexCoord);
		return lerp(decalColor, refractedColor, Refractivity);
	}
	return refractedColor;
}

DispersionOutput DispersionVSFunction(VertexShaderInput input) {
	DispersionOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	//output.TexCoord = input.TexCoord;
	float3 N = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
	float3 I = normalize(worldPosition.xyz - CameraPosition);
	output.R = reflect(I, N);
	output.TRed = refract(I, N, EtaRatio.x);
	output.TGreen = refract(I, N, EtaRatio.y);
	output.TBlue = refract(I, N, EtaRatio.z);
	output.ReflectionFactor = FresnelBias + FresnelScale * pow(max(0, 1 + dot(I, N)), FresnelPower);
	return output;
}

float4 DispersionPSFunction(DispersionOutput input) : COLOR0 {
	float4 reflectedColor = texCUBE(SkyBoxSampler, input.R);
	float4 refractedColor;
	refractedColor.r = texCUBE(SkyBoxSampler, input.TRed).r;
	refractedColor.g = texCUBE(SkyBoxSampler, input.TGreen).g;
	refractedColor.b = texCUBE(SkyBoxSampler, input.TBlue).b;
	refractedColor.a = 1;
	return lerp(refractedColor, reflectedColor, input.ReflectionFactor);
}

FresnelOutput FresnelVSFunction(VertexShaderInput input) {
	FresnelOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	//output.TexCoord = input.TexCoord;
	float3 N = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
	float3 I = normalize(worldPosition.xyz - CameraPosition);
	output.R = reflect(I, N);
	output.T = refract(I, N, 0.90);
	output.rC = max(0, min(1, FresnelBias + pow(max(0, 1 + dot(I, N)), FresnelPower)));
	return output;
}

float4 FresnelPSFunction(FresnelOutput input) : COLOR0 {
	float4 reflectedColor = texCUBE(SkyBoxSampler, input.R);
	float4 refractedColor = texCUBE(SkyBoxSampler, input.T);
	return input.rC * reflectedColor + (1 - input.rC) * refractedColor;
}

technique Reflection {
	pass Pass1 {
		VertexShader = compile vs_4_0 ReflectionVSFunction();
		PixelShader = compile ps_4_0 ReflectionPSFunction();
	}
}

technique Refraction {
	pass Pass1 {
		VertexShader = compile vs_4_0 RefractionVSFunction();
		PixelShader = compile ps_4_0 RefractionPSFunction();
	}
};

technique Dispersion {
	pass Pass1 {
		VertexShader = compile vs_4_0 DispersionVSFunction();
		PixelShader = compile ps_4_0 DispersionPSFunction();
	}
};

technique Fresnel {
	pass Pass1 {
		VertexShader = compile vs_4_0 FresnelVSFunction();
		PixelShader = compile ps_4_0 FresnelPSFunction();
	}
};