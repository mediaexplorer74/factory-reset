// Target feature level9_1 for Reach profile on DirectX
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1

float4x4 projectionMatrix;
float4x4 viewMatrix;
float4x4 modelMatrix;

// Removed dynamic vertex generation for Reach profile compatibility
// Parameters retained for potential future use
float2 angles;
float radius;
int triangles;

struct VertexShaderInput
{
   float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 pos = input.Position;
    output.Position = mul(mul(mul(pos, modelMatrix), viewMatrix), projectionMatrix);

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    return float4(1, 0, 0, 0.5);
}

technique Cone
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
