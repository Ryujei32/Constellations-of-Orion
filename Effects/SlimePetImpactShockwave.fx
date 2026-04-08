sampler uImage0 : register(s0);

float progress;
float intensity;
float2 sourcePosition;

float4 Main(float4 unused : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
	float2 uv = coords;
	float2 delta = uv - sourcePosition;
	float dist = length(delta);
	float ringWidth = 0.2f;

	float ring = 1.0f - saturate(abs(dist - progress) / ringWidth);
	float2 direction = delta / max(dist, 0.0001f);
	float2 distortion = direction * ring * intensity * 0.18f;
	float2 distortedUV = uv + distortion;
	return tex2D(uImage0, distortedUV);
}

technique Technique1
{
	pass SlimePetImpactShockwavePass
	{
		PixelShader = compile ps_2_0 Main();
	}
}
