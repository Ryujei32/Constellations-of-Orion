// Flame mask texture - red channel used as alpha cutout
sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

// Bright inner flame color (top of flame)
float3 uColor;
// Mid-flame color, blended toward bottom
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
// Elapsed time; drives upward noise scrolling
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
// Outer/edge flame color, blended at lower alpha regions
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
matrix uWorldViewProjection;
// x: noise scroll speed
// y: Voronoi scale
// z: gradient noise scale
// w: vertical falloff steepness
float4 uShaderSpecificData;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    output.Position = mul(input.Position, uWorldViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

float mask(float2 uv) {
    // Center UVs: [-0.5, 0.5]

    // Top half: ellipse (wider than tall)
    // Bottom half: circle (uniform radius)
    float rx = 0.3;          // horizontal radius (both halves)
    float ryTop = 0.5;       // vertical radius, top half (ellipse)
    float ryBot = 0.3;         // vertical radius, bottom half (circle = rx)
    
    float2 p = uv - float2(0.5, 0.4);

    float ry = p.y < 0.0 ? ryBot : ryTop;

    // Ellipse SDF: (x/rx)^2 + (y/ry)^2 <= 1
    float d = (p.x * p.x) / (rx * rx) + (p.y * p.y) / (ry * ry);

    return d * d;  // 1 inside, 0 outside
}

float2 hash2(float2 p)
{
    p = float2(dot(p, float2(127.1, 311.7)),
               dot(p, float2(269.5, 183.3)));
    return frac(sin(p) * 43758.5453);
}

float voronoi(float2 uv)
{
    float2 cell = floor(uv);
    float2 f = frac(uv);
    float minDist = 1.0;

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 neighbor = float2(float(x), float(y));
            float2 vpoint = hash2(cell + neighbor);
            float2 diff = neighbor + vpoint - f;
            minDist = min(minDist, length(diff));
        }
    }

    return minDist;
}

float2 gradientNoise_dir(float2 p)
{
    p = fmod(p, 289.0);
    float x = fmod((34.0 * p.x + 1.0) * p.x, 289.0) + p.y;
    x = fmod((34.0 * x + 1.0) * x, 289.0);
    x = frac(x / 41.0) * 2.0 - 1.0;
    return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
}

float gradientNoise(float2 p)
{
    float2 ip = floor(p);
    float2 fp = frac(p);
    float d00 = dot(gradientNoise_dir(ip), fp);
    float d01 = dot(gradientNoise_dir(ip + float2(0, 1)), fp - float2(0, 1));
    float d10 = dot(gradientNoise_dir(ip + float2(1, 0)), fp - float2(1, 0));
    float d11 = dot(gradientNoise_dir(ip + float2(1, 1)), fp - float2(1, 1));
    fp = fp * fp * fp * (fp * (fp * 6.0 - 15.0) + 10.0);
    return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates.xy;
    float opacity = length(input.Color) * uOpacity;

    float y_speed = uShaderSpecificData.x;
    float v_scale = uShaderSpecificData.y;
    float g_scale = uShaderSpecificData.z;
    float height  = uShaderSpecificData.w;

    float2 ytile = float2(0, uTime * -y_speed);
    float vnoise = 1.0 - voronoi(coords * v_scale + ytile);
    vnoise *= vnoise;
    float gnoise = pow(0.75 - gradientNoise(coords * g_scale + ytile), 2.0);
    float gv = clamp(vnoise * gnoise, 0.0, 1.0);

    float yfalloff = height * pow(coords.y, 2.0);
    float mask_val = mask(coords);

    float alpha = 1.0 - clamp(gv * yfalloff + mask_val, 0.0, 1.0);
    alpha *= 1.5;

    float3 flameColor = lerp(uColor, uSecondaryColor, coords.y);
    flameColor = lerp(flameColor, uLightSource, coords.y);
    flameColor = lerp(flameColor, uLightSource, 1.0 - alpha);

    return float4(flameColor, 1.0) * alpha * opacity;
}

technique Technique1
{
    pass Flame
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}