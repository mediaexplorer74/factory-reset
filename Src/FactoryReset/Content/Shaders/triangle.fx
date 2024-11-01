//#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0

float4x4 projectionMatrix;
float4x4 viewMatrix;
float4x4 modelMatrix;
float4 color;

struct VertexShaderInput
{
   float4 Position : POSITION;
   uint VertexID: SV_VertexID;
};

struct VertexShaderOutput
{
    float4 Position : POSITION; //SV_POSITION;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 pos = input.Position;

    output.Position = mul(mul(mul(pos, modelMatrix), viewMatrix), projectionMatrix);

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return color;
}

technique Triangle
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
