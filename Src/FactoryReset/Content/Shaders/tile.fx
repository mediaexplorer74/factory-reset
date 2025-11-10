#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1

float4x4 viewMatrix;
float4x4 modelMatrix;

Texture2D tileset;
Texture2D tilemap;

int tileSize;
float2 viewSize;

SamplerState tilemapSampler;
float2 tilemapSize; // width,height in tiles of the tilemap texture (set from C#)

SamplerState tilesetSampler;
float2 tilesetSize; // width,height in pixels of the tileset texture (set from C#)


struct VertexShaderInput
{
 float4 Position : POSITION0;
 float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
 float4 Position : SV_POSITION;
 float2 mapCoord : TEXCOORD0; // pixel-space coordinates into the logical map
 float2 tileIndexF : TEXCOORD1; // tile index in float form
 float2 offsetF : TEXCOORD2; // pixel offset inside tile (float)
 float tileValid : TEXCOORD3; //1.0 if inside bounds,0.0 otherwise
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
 VertexShaderOutput output = (VertexShaderOutput)0;

 output.Position = input.Position;

 // Inverse-map the coordinate as we start from view space but want to get into world space.
 float2 mapCoord = mul(mul(float4(input.UV * viewSize,0,1), viewMatrix), modelMatrix).xy;
 output.mapCoord = mapCoord;

 // Use float math to compute tile index and offset with fewer ops
 float tileSizeF = (float)tileSize;
 float invTile =1.0f / tileSizeF;

 float2 mapXYf = floor(mapCoord);
 // Flip y to bottom-bound
 float2 mapWHf = tilemapSize * tileSizeF;
 mapXYf.y = mapWHf.y - mapXYf.y -1.0f;

 // Bounds check
 if (mapXYf.x <0.0f || mapXYf.y <0.0f || mapXYf.x >= mapWHf.x || mapXYf.y >= mapWHf.y)
 {
 output.tileIndexF = float2(0.0f,0.0f);
 output.offsetF = float2(0.0f,0.0f);
 output.tileValid =0.0f;
 return output;
 }

 // tile index (float) and pixel offset (float)
 float2 tileIndexF = floor(mapXYf * invTile);
 float2 offsetF = mapXYf - tileIndexF * tileSizeF;

 output.tileIndexF = tileIndexF;
 output.offsetF = offsetF;
 output.tileValid =1.0f;

 return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
 if (input.tileValid <0.5f)
 return float4(0,0,0,0);

 // Sample tilemap in pixel shader (allowed)
 float2 tileUV = (input.tileIndexF +0.5f) / tilemapSize;
 float4 tile = tilemap.Sample(tilemapSampler, tileUV);
 if (tile.a <=0.0f)
 return float4(0,0,0,0);

 // Compute tileset sample UV
 float2 basePixel = tile.rg *255.0f * (float)tileSize;
 float2 samplePixel = basePixel + input.offsetF;
 float2 sampleUV = samplePixel / tilesetSize;
 return tileset.Sample(tilesetSampler, sampleUV);
}

technique Tile
{
 pass P0
 {
 VertexShader = compile VS_SHADERMODEL MainVS();
 PixelShader = compile PS_SHADERMODEL MainPS();
 }
};