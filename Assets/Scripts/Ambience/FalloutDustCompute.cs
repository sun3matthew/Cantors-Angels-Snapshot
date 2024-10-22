using UnityEngine;
public class FalloutDustCompute : ParticleSystemCompute
{
    private const float EmitRate = 0.015f;
    private const int DustLife = 14;
    private int NumToEmit;
    protected override Sprite GetSprite() => Resources.Load<Sprite>("Sprites/Particles/Dust");
    public FalloutDustCompute(int numToEmit, Transform parent, float foregroundScale) : base((int)(DustLife / EmitRate * numToEmit), parent, foregroundScale){
        NumToEmit = numToEmit;
    }
    protected override void Emit(float dt)
    {
        Counter += dt;
        if (Counter > EmitRate){
            Counter = 0;
            ParticleShader.SetInt("NumberToEmit", NumToEmit);
            ParticleShader.SetInt("EmitIdx", EmitIdx);
            EmitIdx += NumToEmit;
            if (EmitIdx >= MaxParticles)
                EmitIdx = 0;
        }else{
            ParticleShader.SetInt("NumberToEmit", 0);
        }
    }
}