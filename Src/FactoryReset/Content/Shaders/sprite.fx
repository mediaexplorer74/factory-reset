//#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0

float4x4 projectionMatrix;
float4x4 viewMatrix;
float4x4 modelMatrix;
float4 offset;
Texture2D tileset;

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 UV : TEXCOORD;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0; //SV_POSITION;
    float2 UV : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 pos = input.Position;
    pos.xy *= offset.zw;
    output.Position = mul(mul(mul(pos, modelMatrix), viewMatrix), projectionMatrix);
    output.UV = offset.xy + input.UV*offset.zw;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return tileset.Load(int3(input.UV.x, input.UV.y, 0));
}

technique Tile
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};