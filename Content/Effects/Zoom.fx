sampler2D input : register(s0);

float2 Zoom;

float2 Offset;

static const float2 Center = float2(0.5, 0.5);

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
	float2 middle = Center + Offset;
	return tex2D(input , ((coords.xy - middle) / Zoom) + middle); 
}

technique Technique1
{
    pass ZoomPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}