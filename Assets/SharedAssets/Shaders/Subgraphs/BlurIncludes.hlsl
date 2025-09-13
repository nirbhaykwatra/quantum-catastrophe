// gaussian blur function adapted from Daniel Ilett
void GaussianBlur_float(UnityTexture2D Texture, float2 UV, float Blur, UnitySamplerState Sampler, float2 TexelSize, out float3 RGB, out float Alpha)
{
    float4 col = float4(0.0, 0.0, 0.0, 0.0);
    float kernelSum = 0.0;

    int upper = ((Blur - 1) / 2);
    int lower = -upper;

    for (int x = lower; x <= upper; ++x)
    {
        for (int y = lower; y <= upper; ++y)
        {
            kernelSum ++;

            float2 offset = float2(TexelSize.x * x, TexelSize.y * y);
            col += Texture.Sample(Sampler, UV + offset);
        }
    }

    col /= kernelSum;
    RGB = float3(col.r, col.g, col.b);
    Alpha = col.a;
}