using System.Collections.Generic;
using UnityEngine;

public class FalloutDust : Particle
{
    public static Sprite sprite;
    private const float DustLife = 14f;
    public FalloutDust() : base(CoreRandom.GlobalRange(BoardRender.Instance.BoardBounds[0], BoardRender.Instance.BoardBounds[1]), CoreRandom.GlobalRange(BoardRender.Instance.BoardBounds[2], BoardRender.Instance.BoardBounds[3]) + 10, DustLife){
        Velocity = new Vector2(CoreRandom.GlobalRange(-0.2f, 0.2f), CoreRandom.GlobalRange(-0.8f, 0.4f));
    }
    public static List<Particle> Emit(float counter){
        if (counter > 0.1f){
            List<Particle> particles = new();
            for (int i = 0; i < 150; i++)
                particles.Add(new FalloutDust());
            return particles;
        }
        return null;
    }
    public override void Update(float dt)
    {
        base.Update(dt);
        // if (Life < 4){ //! idk if you should do this, artistic vision vs realism/worldBuilding
        //     Velocity = new Vector2(0, 0);
        //     Acceleration = new Vector2(0, 0);
        // }
        // else
            Acceleration = new Vector2(CoreRandom.GlobalRange(-0.4f, 0.4f), CoreRandom.GlobalRange(-0.3f, 0.1f));
    }

    static FalloutDust()
    {
        sprite = Resources.Load<Sprite>("Sprites/Particles/Dust");
    }
    public override Sprite GetSprite() => sprite;
    public override Color GetColor() => new (0.23f, 0.18f, 0.16f, Mathf.Clamp((0.5f - Mathf.Abs((Life / DustLife) - 0.5f)) * 4.0f, 0, 1));
    // public override Color GetColor() => new (0.33f, 0.28f, 0.26f, Mathf.Clamp((0.5f - Mathf.Abs((Life / DustLife) - 0.5f)) * 4.0f, 0, 1));
    // public override Color GetColor() => new (0.87f, 0.42f, 0.24f, (0.5f - Mathf.Abs((Life / DustLife) - 0.5f)) * 1.6f);
    public override int GetSortingOrder() => 2;
}