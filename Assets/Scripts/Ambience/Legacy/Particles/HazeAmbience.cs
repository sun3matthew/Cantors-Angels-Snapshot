using System.Collections.Generic;
using UnityEngine;

public class HazeAmbience : Particle
{
    public static Sprite[] sprites;
    private const float DustLife = 120;
    private const int MaxHazeLength = 1400;

    private int Size;
    private float MaxOpacity;
    private Color HazeColor;
    public HazeAmbience(float x, float y, int size, float maxOpacity, Color hazeColor, Vector2 velocity) : base(x, y, DustLife){
        // Velocity = new Vector2(Random.Range(-0.2f, 0.2f), 0);
        Velocity = velocity;
        HazeColor = hazeColor;
        MaxOpacity = maxOpacity;
        Size = size;
    }
    public static List<Particle> Emit(float counter){
        if (counter > 3){

            List<Particle> particles = new();
            float startX = Random.Range(BoardRender.Instance.BoardBounds[0], BoardRender.Instance.BoardBounds[1]);
            float startY = Random.Range(BoardRender.Instance.BoardBounds[2], BoardRender.Instance.BoardBounds[3]);
            // Debug.Log("Emitting Haze at " + startX + ", " + startY);

            float startSize = CoreRandom.GlobalRange(MaxHazeLength * 0.25f, MaxHazeLength * 0.75f);
            float verticalSize = CoreRandom.GlobalRange(300, 1000);
            float currentX = startX;


            float[] hsv = new float[3];
            hsv[0] = CoreRandom.GlobalRange(0.055f, 0.083f);
            hsv[1] = CoreRandom.GlobalRange(0.6f, 0.8f);
            hsv[2] = CoreRandom.GlobalRange(0.6f, 0.85f);
            Color HazeColor = Color.HSVToRGB(hsv[0], hsv[1], hsv[2]);
            Vector2 deltaVelocity = new Vector2(CoreRandom.GlobalRange(0.6f, 1.2f), CoreRandom.GlobalRange(0.0f, 0.2f));
            Vector2 baseVelocity = new Vector2(CoreRandom.GlobalRange(-deltaVelocity.y, deltaVelocity.y), CoreRandom.GlobalRange(-deltaVelocity.y, deltaVelocity.y));

            Vector2 velocity = new Vector2(CoreRandom.GlobalRange(-deltaVelocity.x, deltaVelocity.x), 0) + baseVelocity;
            for (int i = 0; i < verticalSize; i++){
                float newY = startY + i * (CoreAnimator.SrScale / 100f);
                if (i % 2 == 0){
                    startSize = Mathf.Clamp(startSize + GetNormal(20), 0, MaxHazeLength - 1);
                    if (CoreRandom.GlobalRange(0, 3) == 0){
                        float offsetX = GetNormal(startSize * (CoreAnimator.SrScale / 100f) / 4);
                        currentX = Mathf.Clamp(currentX + offsetX, BoardRender.Instance.BoardBounds[0], BoardRender.Instance.BoardBounds[1]);
                        velocity = new Vector2(-offsetX / 16, 0) + baseVelocity;
                        if (newY > BoardRender.Instance.BoardBounds[3])
                            break;
                    }
                }
                
                float maxOpacity = Mathf.Sin(i / verticalSize * Mathf.PI) * 0.30f;
                particles.Add(new HazeAmbience(currentX, newY, (int)startSize, maxOpacity, HazeColor, velocity));
            }
            return particles;
        }
        return null;
    }
    public static float GetNormal(float Max){
        float u1 = 1.0f-CoreRandom.Value();
        float u2 = 1.0f-CoreRandom.Value();
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        randStdNormal *= Max / 3;
        return Mathf.Clamp(randStdNormal, -Max, Max);
    }

    public override void Update(float dt)
    {
        base.Update(dt);
    }

    static HazeAmbience()
    {
        Sprite baseSprite = Resources.Load<Sprite>("Sprites/Particles/Pixel");
        sprites = new Sprite[MaxHazeLength];
        for (int i = 0; i < MaxHazeLength; i++)
            sprites[i] = SpritePostProcess.ScaleBy(baseSprite, i + 1, 1);
    }
    public override Sprite GetSprite() => sprites[Size];
    public override Color GetColor() => new (HazeColor.r, HazeColor.g, HazeColor.b, (0.5f - Mathf.Abs((Life / DustLife) - 0.5f)) * 2f * MaxOpacity);
    // public override Color GetColor() => new (0.33f, 0.28f, 0.26f, Mathf.Clamp((0.5f - Mathf.Abs((Life / DustLife) - 0.5f)) * 4.0f, 0, 1));
    // public override Color GetColor() => new (0.87f, 0.42f, 0.24f, (0.5f - Mathf.Abs((Life / DustLife) - 0.5f)) * 1.6f);
    public override int GetSortingOrder() => 9;
}