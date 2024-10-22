using UnityEngine;
using UnityEngine.Pool;

public class RealParticle : IPoolable
{
    public static UniversalPool<RealParticle> Pool;
    public static Transform Parent;
    public static void Initialize()
    {
        Pool = new UniversalPool<RealParticle>();
        Parent = new GameObject("Particles").transform;
    }

    public Particle AttachedParticle;
    public Transform Transform;
    public SpriteRenderer Sr;

    public void Instantiate()
    {
        Transform = new GameObject("Particle").transform;
        Transform.SetParent(Parent);
        Transform.localScale = new Vector3(CoreAnimator.SrScale, CoreAnimator.SrScale, 1);

        Sr = Transform.gameObject.AddComponent<SpriteRenderer>();
        Sr.color = Color.white;
        Sr.sprite = null;
        Sr.sortingLayerName = "FX";
        Sr.sortingOrder = 1;
        Sr.enabled = false;
    }

    public RealParticle BindTo(Particle particle)
    {
        AttachedParticle = particle;

        Transform.position = particle.GetPosition();
        Sr.sprite = particle.GetSprite();
        Sr.sortingOrder = particle.GetSortingOrder();
        Sr.color = particle.GetColor();

        // TODO God Rays are hard to make, I think you have to simulate each pixel ray. Game runs at 100 fps rn so you have quite to spare.
        return this;
    }

    public void Activate() => Sr.enabled = true;
    public void Deactivate() => Sr.enabled = false;
    public void Return() => Pool.Return(this);
    public static RealParticle Get() => Pool.Get();
}