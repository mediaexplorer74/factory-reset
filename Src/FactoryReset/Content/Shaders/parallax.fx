// Target feature level9_1 for Reach profile on DirectX
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1

//sampler2D parallax;
Texture2D parallax;
SamplerState tileSampler;

float2 viewSize;
float2 viewPos;
float viewScale;
float2 parallaxSize; // passed from engine, replaces GetDimensions usage

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 mapCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = input.Position;
 
    // Inverse-map the coordinate as we start from view space but want to get into world space.
    output.mapCoord = (input.UV * viewSize + viewPos) / viewScale;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    // Use provided parallax texture size instead of GetDimensions
    int2 mapWH = parallaxSize;
    int2 mapXY = int2(floor(input.mapCoord));
 
    // Flip around to be bottom-bound.
    mapXY.y = mapWH.y - mapXY.y - 1;
 
    // Wrap and clamp
    mapXY.x = abs(mapXY.x) % mapWH.x;
    mapXY.y = clamp(mapXY.y, 0, mapWH.y - 1);
 
    // Use tex2D with sampler2D for compatibility
    //return tex2D(parallax, float2(mapXY) / float2(mapWH)); 
    //return parallax.Load(int3(mapXY, 0));
    return parallax.Sample(tileSampler, float2(mapXY) / float2(mapWH));
}

technique Tile
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
