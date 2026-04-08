sampler uImage0 : register(s0);

float scroll;
float repeats;
float intensity;
float4 tintA;
float4 tintB;
float pixelStepsX;
float pixelStepsY;
float pixelMix;

float4 Main(float4 inputColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
	float y = coords.y;
	float edge = 1.0f - abs(y * 2.0f - 1.0f);
	edge = saturate(edge);

	// Pixel-stepped "air -> color" fade (compiler-safe staircase).
	float edgeBand;
	if (edge < 0.14f) edgeBand = 0.00f;
	else if (edge < 0.28f) edgeBand = 0.18f;
	else if (edge < 0.42f) edgeBand = 0.36f;
	else if (edge < 0.56f) edgeBand = 0.54f;
	else if (edge < 0.70f) edgeBand = 0.72f;
	else if (edge < 0.84f) edgeBand = 0.88f;
	else edgeBand = 1.00f;

	float mixAmount = pixelMix;
	if (mixAmount < 0.0f) mixAmount = 0.0f;
	if (mixAmount > 1.0f) mixAmount = 1.0f;
	float airToColor = edge * (1.0f - mixAmount) + edgeBand * mixAmount;

	float3 grad = lerp(tintA.rgb, tintB.rgb, y);
	float3 color = grad * inputColor.rgb;
	float alpha = inputColor.a * airToColor * intensity;
	return float4(color, alpha);
}

technique Technique1
{
	pass StellarLashTrailPass
	{
		PixelShader = compile ps_2_0 Main();
	}
}
