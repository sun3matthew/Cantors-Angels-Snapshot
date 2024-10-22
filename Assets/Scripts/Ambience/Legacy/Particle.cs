using System;
using UnityEngine;
public abstract class Particle
{
    public RealParticle AssociatedParticle;
    
    protected Vector2 Position;
    protected Vector2 Velocity;
    protected Vector2 Acceleration;
    protected float Life;
    public abstract Sprite GetSprite();
    public abstract int GetSortingOrder();
    public virtual Color GetColor() => Color.white;
    public Particle(float x, float y, float life)
    {
        Position = new Vector2(x, y);
        Velocity = new Vector2();
        Acceleration = new Vector2();
        Life = life;
    }

    public virtual void Update(float dt)
    {
        Velocity += Acceleration * dt;
        Position += Velocity * dt;
        Life -= dt;
    }

    public bool IsDead() => Life <= 0;
    public Vector3 GetPosition() => new Vector3(Position.x, Position.y, 1);
}