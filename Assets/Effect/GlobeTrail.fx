// 模糊处理
sampler maskImage : register(s0);

float4x4 uTransform;
float uTime;
struct VSInput
{
    float2 Pos : POSITION0;
    float4 Color : COLOR0;
    float3 Texcoord : TEXCOORD0;
};

struct PSInput
{
    float4 Pos : SV_POSITION;
    float4 Color : COLOR0;
    float3 Texcoord : TEXCOORD0;
};

float4 PSFunc(PSInput input) : COLOR0
{
    float3 coords = input.Texcoord;
    float maskX = -uTime + coords.x;
    float4 maskColor = tex2D(maskImage, float2(maskX, coords.y));
    return input.Color * maskColor.r * 2 * coords.z;
}

PSInput VSFunc(VSInput input)
{
    PSInput output;
    output.Color = input.Color;
    output.Texcoord = input.Texcoord;
    output.Pos = mul(float4(input.Pos, 0, 1), uTransform);
    return output;
}

technique Technique1
{
    pass ColorBar
    {
        VertexShader = compile vs_3_0 VSFunc();
        PixelShader = compile ps_3_0 PSFunc();
    }
}