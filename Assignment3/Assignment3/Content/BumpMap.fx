float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float3 CameraPosition;
float3 LightPosition;

float AmbientColor;
float AmbientIntensity;
float4 DiffuseColor;
float DiffuseIntensity;
float4 SpecularColor;
float SpecularIntensity;
float Shininess;

float height = 1;
float2 UVScale;

texture normalMap;
//texture EnvironmentMap;
int control;

sampler tsampler1 = sampler_state {
	texture = <normalMap>;
	magfilter = LINEAR; // None, POINT, LINEAR, Anisotropic
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap; // Clamp, Mirror, MirrorOnce, Wrap, Border
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
	float4 Normal: NORMAL0;
	float4 Tangent: TANGENT0;
	float4 Binormal: BINORMAL0;
	float2 TexCoord: TEXCOORD0;
};

struct VertexShaderOutput {
	float4 Position: POSITION0;
	float3 Normal: TEXCOORD0;
	float3 Tangent: TEXCOORD1;
	float3 Binormal: TEXCOORD2;
	float2 TexCoord: TEXCOORD3;
	float3 Position3D: TEXCOORD4;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	output.Normal = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
	output.Tangent = normalize(mul(input.Tangent, WorldInverseTranspose).xyz);
	output.Binormal = normalize(mul(input.Binormal, WorldInverseTranspose).xyz);
	output.TexCoord = input.TexCoord * UVScale;
	output.Position3D = worldPosition.xyz;
	return output;
}

float4 PSFunction1(VertexShaderOutput input) : COLOR0{
	return tex2D(tsampler1, input.TexCoord);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0{
	float3 L = normalize(LightPosition - input.Position3D);
	float3 V = normalize(CameraPosition - input.Position3D);
	float3 N = normalize(input.Normal);
	float3 T = normalize(input.Tangent);
	float3 B = normalize(input.Binormal);
	float3 H = normalize(L + V);
	//calculate the normal Tex
	float3 normalTex = tex2D(tsampler1, input.TexCoord).xyz; //0.0 - 1.0
	//convert it from [0, 1] to [-1, 1]
	normalTex = 2.0 * (normalTex - float3 (0.5, 0.5, 0.5)); //-1.0 - 1.0

	//float3 bumpNormal = normalize(N + normalTex.x * T + normalTex.y * B);

	float3x3 TangentToWorld;
	TangentToWorld[0] = input.Tangent;
	TangentToWorld[1] = input.Binormal;
	TangentToWorld[2] = input.Normal;

	float3 bumpNormal = mul(normalTex, TangentToWorld);
	float4 diffuse = DiffuseColor * DiffuseIntensity * max(0, (dot(bumpNormal, L))); 
	diffuse.a = 1.0;
	float4 specular = SpecularColor * SpecularIntensity * pow(saturate(dot(H, bumpNormal)), Shininess); 
	specular.a = 1.0;

	switch (control) {
		case 1:
			return tex2D(tsampler1, input.TexCoord);
		case 2:
			return float4(bumpNormal, 1);
		case 3:
			return diffuse + specular;
		case 4:

		default:
			return float4(0, 0, 0, 0);
	}
	//return diffuse + specular;
}

technique Bump {
	pass Pass1 {
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}