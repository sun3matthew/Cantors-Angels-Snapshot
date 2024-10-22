using UnityEngine;
public class HazeAmbienceCompute : ParticleSystemCompute
{
    private const float EmitRate = 0.03f;
    private const float DustLife = 90;
    private const int MaxHazeLength = 10000;
    private const int MaxVerticalSize = 30000;

    private int TargetVerticalSize;
    private int CurrentVerticalSize;
    private float CurrentHorizontalSize;
    private Vector2 CurrentPosition;
    private Vector2 CurrentVelocity;
    private Vector2 DeltaVelocity;
    private Vector2 BaseVelocity;
    private Color HazeColor;

    private ComputeBuffer ExtraInfoBuffer;
    float LastReset;
    private struct ExtraInfo{
        // float r, g, b, a;
        int size;
    }
    
    protected override Sprite GetSprite() => Resources.Load<Sprite>("Sprites/Particles/Pixel");
    public HazeAmbienceCompute(Transform parent, float foregroundScale) : base(MaxVerticalSize , parent, foregroundScale){
        // ExtraInfoBuffer = new ComputeBuffer(MaxParticles, sizeof(float) * 4 + sizeof(int));
        ExtraInfoBuffer = new ComputeBuffer(MaxParticles, sizeof(int));
        ExtraInfo[] extraInfos = new ExtraInfo[MaxParticles];
        for (int i = 0; i < extraInfos.Length; i++)
            extraInfos[i] = new ExtraInfo();
        ExtraInfoBuffer.SetData(extraInfos);
        ParticleShader.SetBuffer(0, "ExtraInfoBuffer", ExtraInfoBuffer);

        TargetVerticalSize = 0;
        CurrentVerticalSize = 0;
        CurrentHorizontalSize = 0;
        LastReset = Random.Range(-DustLife, DustLife);
        CurrentPosition = new Vector2();
        CurrentVelocity = new Vector2();
        HazeColor = new Color();
    }
    protected override void Emit(float dt)
    {
        Counter += dt;
        LastReset += dt;
        if (Counter > EmitRate){
            Counter = 0;
            if (CurrentVerticalSize < TargetVerticalSize){
                if (CurrentVerticalSize % 2 == 0){
                    CurrentHorizontalSize = Mathf.Clamp(CurrentHorizontalSize + GetNormal(100), 0, MaxHazeLength * 2);
                    if (CoreRandom.GlobalRange(0, 3) == 0){
                        float offsetX = GetNormal(CurrentHorizontalSize / 32);
                        CurrentPosition.x += offsetX;
                        CurrentVelocity = new Vector2(-offsetX / 16, 0) + BaseVelocity;
                    }
                }

                // float maxOpacity = Mathf.Sin((float)CurrentVerticalSize / TargetVerticalSize * Mathf.PI) * 0.4f;
                float maxOpacity = 0.34f;

                ParticleShader.SetInt("NumberToEmit", 1);
                ParticleShader.SetInt("EmitIdx", EmitIdx);
                ParticleShader.SetFloat("MaxOpacity", maxOpacity);
                ParticleShader.SetFloats("CurrentPosition", new float[] { CurrentPosition.x, CurrentPosition.y + CurrentVerticalSize });
                ParticleShader.SetFloats("CurrentVelocity", new float[] { CurrentVelocity.x, 0 });
                ParticleShader.SetInt("EmitSize", (int)CurrentHorizontalSize);

                CurrentVerticalSize++;

                EmitIdx++;
                if (EmitIdx >= MaxParticles)
                    EmitIdx = 0;

                LastReset = 0; // needed so that Last reset only starts counter after last particle is emitted
            }else if(LastReset > DustLife){
                CurrentPosition = new Vector2(RandomPosition().x, RandomPosition().y / 20f);
                CurrentVerticalSize = 0;
                CurrentHorizontalSize = CoreRandom.GlobalRange(MaxHazeLength * 0.25f, MaxHazeLength);
                TargetVerticalSize = (int)MaxSize()[1];
                float[] hsv = new float[3];
                // hsv[0] = CoreRandom.Range(0.055f, 0.083f);
                hsv[0] = CoreRandom.GlobalRange(0.045f, 0.073f);
                hsv[1] = CoreRandom.GlobalRange(0.7f, 0.9f);
                hsv[2] = CoreRandom.GlobalRange(0.6f, 0.85f);
                HazeColor = Color.HSVToRGB(hsv[0], hsv[1], hsv[2]) / 2.0f;
                DeltaVelocity = new(CoreRandom.GlobalRange(1.6f, 2.2f), CoreRandom.GlobalRange(8.0f, 48.0f));
                BaseVelocity = new(CoreRandom.GlobalRange(-DeltaVelocity.y, DeltaVelocity.y), CoreRandom.GlobalRange(-DeltaVelocity.y, DeltaVelocity.y));
                CurrentVelocity = new Vector2(CoreRandom.GlobalRange(-DeltaVelocity.x, DeltaVelocity.x), 0) + BaseVelocity;

                ParticleShader.SetFloats("HazeColor", new float[] { HazeColor.r, HazeColor.g, HazeColor.b });
            }else{
                ParticleShader.SetInt("NumberToEmit", 0);
            }
        }else{
            ParticleShader.SetInt("NumberToEmit", 0);
        }
    }

    public static float GetNormal(float Max){
        float u1 = 1.0f-CoreRandom.Value();
        float u2 = 1.0f-CoreRandom.Value();
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        randStdNormal *= Max / 3;
        return Mathf.Clamp(randStdNormal, -Max, Max);
    }

    public override void Release()
    {
        base.Release();
        ExtraInfoBuffer.Release();
    }
}