// Target feature level9_1 for Reach profile on DirectX
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1

float4x4 projectionMatrix;
float4x4 viewMatrix;
float4x4 modelMatrix;
float4 offset;
Texture2D tileset;
SamplerState tilesetSampler;
float2 tilesetSize; // width,height in pixels of the tileset texture (passed from engine)

struct VertexShaderInput
{
 float4 Position : POSITION0;
 float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
 float4 Position : SV_POSITION;
 float2 UV : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
 VertexShaderOutput output = (VertexShaderOutput)0;

 float4 pos = input.Position;
 pos.xy *= offset.zw;
 output.Position = mul(mul(mul(pos, modelMatrix), viewMatrix), projectionMatrix);
 // Convert pixel-based source rect (offset.xy, offset.zw) to normalized UVs
 output.UV = (offset.xy + input.UV * offset.zw) / tilesetSize; // output.UV = offset.xy + input.UV*offset.zw;

 return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    // Sample the tileset texture using the adjusted UVs
   return tileset.Sample(tilesetSampler, input.UV); // return tileset.Load(int3(input.UV.x, input.UV.y, 0));
}

technique Tile
{
 pass P0
 {
 VertexShader = compile VS_SHADERMODEL MainVS();
 PixelShader = compile PS_SHADERMODEL MainPS();
 }
};
