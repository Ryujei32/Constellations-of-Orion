sampler uImage0 : register(s0);

float2 sourcePosition0;
float radius0;
float thickness0;
float intensity0;

float2 sourcePosition1;
float radius1;
float thickness1;
float intensity1;

float2 sourcePosition2;
float radius2;
float thickness2;
float intensity2;

float2 DistortOne(float2 uv, float2 sourcePosition, float radius, float thickness, float intensity)
{
	float2 delta = uv - sourcePosition;
	float dist = length(delta);
	float safeThickness = max(thickness, 0.0001f);
	float distortMask = 1.0f - saturate(abs(dist - radius) / safeThickness);
	float2 direction = delta / max(dist, 0.0001f);
	return direction * distortMask * intensity;
}

float4 Main(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
	float2 uv = coords;
	float2 distortion = DistortOne(uv, sourcePosition0, radius0, thickness0, intensity0);
	distortion += DistortOne(uv, sourcePosition1, radius1, thickness1, intensity1);
	distortion += DistortOne(uv, sourcePosition2, radius2, thickness2, intensity2);
	float2 distortedUV = uv + distortion;
	return tex2D(uImage0, distortedUV);
}

technique Technique1
{
	pass GelBubbleOutlineDistortionPass
	{
		PixelShader = compile ps_2_0 Main();
	}
}
