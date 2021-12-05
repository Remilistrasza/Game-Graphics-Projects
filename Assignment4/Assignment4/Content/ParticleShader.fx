float4x4 World;
float4x4 View;
float4x4 Projection;
float3 LightPosition;
float3 CameraPosition;
//float4x4 WorldInverseTranspose;

/*
float4 AmbientColor = (0, 0, 0, 0);
float AmbientIntensity = 0.1f;
float4 DiffuseColor = (1, 1, 1, 1);
float DiffuseIntensity = 0.1f;
float4 SpecularColor = (1, 1, 1, 1);
float SpecularIntensity = 1;
float Shininess = 20;
*/

float4x4 InverseCamera; //Inverse Camera Matrix
texture2D Texture;
int Texture_no;

sampler ParticleSampler : register(s0) = sampler_state {
	texture = <Texture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
};

struct VertexShaderInput
{
	float4 Position: POSITION;
	float2 TexCoord: TEXCOORD0;
	float4 ParticlePosition: POSITION1;
	float4 ParticleParamater: POSITION2; // x: age y: max age
};

struct VertexShaderOutput
{
	float4 Position: POSITION;
	float2 TexCoord: TEXCOORD0;
	float4 Color: COLOR0;
	float4 WorldPosition : TEXCOORD1;
	float4 Normal : TEXCOORD2;
};

VertexShaderOutput ParticleVertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, InverseCamera);
	worldPosition.xyz = worldPosition.xyz * sqrt(input.ParticleParamater.x);
	worldPosition += input.ParticlePosition;

	output.Position = mul(mul(mul(worldPosition, World), View), Projection);
	output.TexCoord = input.TexCoord;
	output.Normal = float4(0, 0, 1, 0);
	output.WorldPosition = worldPosition;
	// x/y scale of color based on age
	output.Color = 1 - input.ParticleParamater.x / input.ParticleParamater.y;
	return output;
}

float4 ParticlePixelShader(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
	float3 L = normalize(LightPosition);
	float3 R = reflect(-L, N);
	//float4 ambient = AmbientColor * AmbientIntensity;
	float4 ambient = float4(0, 0, 0, 1) * 1;
	//float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
	float4 diffuse = 0 * float4(1, 1, 1, 1) * max(0, dot(N, L));
	//float4 specular = SpecularIntensity * SpecularColor * pow(max(0, dot(V, R)), Shininess);
	float4 specular = 1 * float4(1, 1, 1, 1) * pow(max(0, dot(V, R)), 50);

	float4 color1 = tex2D(ParticleSampler, input.TexCoord);
	float4 color2 = saturate(ambient + diffuse + specular);
	float4 color = (Texture_no > 0) ? color1 : color2;
	color *= input.Color;
	return color;
}

technique particle
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 ParticleVertexShader();
		PixelShader = compile ps_4_0 ParticlePixelShader();
	}
}