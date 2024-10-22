using UnityEngine;

public class GodRaysCompute : ParticleSystemCompute
{
    private const float EmitRate = 1f;
    private const float DustLife = 90;
    private const int MaxGodRayWidth = 2000;

    private int HorizontalSize;
    private int CurrentHorizontalSize;
    private Vector2 CurrentPosition;
    public float Angle;

    private ComputeBuffer ExtraInfoBuffer;

    // private static Vector2 Center;

    private float SubCounter;

    protected override Sprite GetSprite() => Resources.Load<Sprite>("Sprites/Particles/Pixel");
    private struct ExtraInfo{
        float MaxOpacity;
        float Angle;
    }

    // ? spawn it more often close to churches
    public GodRaysCompute(Transform parent, float foregroundScale) : base(MaxGodRayWidth * 2, parent, foregroundScale)
    {
        // Center = new Vector2(BoardRender.Instance.BoardBounds[1] * ArbXScale / 2, BoardRender.Instance.BoardBounds[3] * ArbYScale / 2);
        // Center.y += 100000f;
        CoreRandom coreRandom = new(Board.Instance.Seed);
        Angle = coreRandom.Range(45, 135) * Mathf.Deg2Rad;

        ParticleShader.SetFloat("Angle", Angle);

        HorizontalSize = 0;
        CurrentHorizontalSize = 0;
        CurrentPosition = new Vector2();
        // Angle = 0;
        SubCounter = Random.value * DustLife;

        ExtraInfoBuffer = new ComputeBuffer(MaxParticles, sizeof(float) * 2);
        ExtraInfo[] extraInfos = new ExtraInfo[MaxParticles];
        for (int i = 0; i < extraInfos.Length; i++)
            extraInfos[i] = new ExtraInfo();
        ExtraInfoBuffer.SetData(extraInfos);
        ParticleShader.SetBuffer(0, "ExtraInfoBuffer", ExtraInfoBuffer);
    }
    protected override void Emit(float dt)
    {
        SubCounter += dt;
        if (SubCounter > DustLife + 1)
        {
            SubCounter = 0;
            if (CurrentHorizontalSize >= HorizontalSize){
                CurrentPosition = new Vector2(RandomPosition().x, RandomPosition().y);
                HorizontalSize = (int)CoreRandom.GlobalRange(MaxGodRayWidth * 0.25f, MaxGodRayWidth);
                // Vector2 direction = Center - CurrentPosition;
                // Vector2 direction = CurrentPosition - Center;
                // Angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                CurrentHorizontalSize = 0;

                ParticleShader.SetFloats("CurrentPosition", new float[] { CurrentPosition.x, CurrentPosition.y });
                // ParticleShader.SetFloat("Angle", Angle);

                // Debug.Log("Emitting God Ray at " + CurrentPosition + " with angle " + Angle + " and size " + HorizontalSize);
            }
        }

        Counter += dt;
        if (Counter > 0.1)
        {
            Counter = 0;

            if (CurrentHorizontalSize < HorizontalSize)
            {
                ParticleShader.SetInt("NumberToEmit", 8);
                ParticleShader.SetInt("EmitIdx", EmitIdx);
                ParticleShader.SetInt("CurrentHorizontalSize", CurrentHorizontalSize);

                CurrentHorizontalSize += 8;

                EmitIdx += 8;
                if (EmitIdx >= MaxParticles)
                    EmitIdx = 0;
            }else{
                ParticleShader.SetInt("NumberToEmit", 0);
            }
        }else{
            ParticleShader.SetInt("NumberToEmit", 0);
        }
    }

    public override void Release()
    {
        base.Release();
        ExtraInfoBuffer.Release();
    }

}