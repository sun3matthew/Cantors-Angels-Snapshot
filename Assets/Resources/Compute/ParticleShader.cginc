// #define PIXEL_SCALE 38.66
#define PIXEL_SCALE 24.0

#define ARB_X_SCALE 47.0
#define ARB_Y_SCALE 46.6

struct Particle
{
    float2 Position;
    float2 Velocity;
    float2 Acceleration;
    float Life;
};

// RWStructuredBuffer<ExtraInfo> ExtraInformation;
RWStructuredBuffer<Particle> Particles;
int NumParticles;

RWTexture2D<float4> Result;
uint2 ResultSize;

Texture2D<float4> ParticleTexture;
uint2 ParticleTextureSize;

// StructuredBuffer<ExtraInfo2> InitializationParams;
// EmitSeed;
uint NumberToEmit;
uint EmitIdx;

float2 CameraPosition;
float2 BoardMaxSize;

float Dt;
float RandSeed;

float rand(inout float2 co){
    float value = frac(sin(dot(co.xy, float2(12.9898,78.233))) * 43758.5453);
    co.xy += 1.0;
    return value;
}

float rand(inout float2 co, float min, float max){
    return rand(co) * (max - min) + min;
}

void render(Particle particle, float4 color){
    int x = (int)(particle.Position.x - CameraPosition.x * PIXEL_SCALE) - ParticleTextureSize.x / 2;
    int y = (int)(particle.Position.y - CameraPosition.y * PIXEL_SCALE) - ParticleTextureSize.y / 2;

    for (uint ix = 0; ix < ParticleTextureSize.x; ix++){
        for (uint iy = 0; iy < ParticleTextureSize.y; iy++){
            int px = x + ix;
            int py = y + iy;
            if (px >= 0 && (uint)px < ResultSize.x && py >= 0 && (uint)py < ResultSize.y){
                float4 colorBase = ParticleTexture.Load(int3(ix, iy, 0));
                colorBase = float4(colorBase.r * color.r, colorBase.g * color.g, colorBase.b * color.b, colorBase.a * color.a);
                Result[uint2(px, py)] = lerp(Result[uint2(px, py)], colorBase, colorBase.a);
            }
        }
    }
}

float2 randomPosition(inout float2 seed){
    return float2(rand(seed, 0, BoardMaxSize.x * ARB_X_SCALE), rand(seed, 0, BoardMaxSize.y * ARB_Y_SCALE));
}