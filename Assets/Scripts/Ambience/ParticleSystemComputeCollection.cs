using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemComputeCollection{
    public static List<ParticleSystemCompute> ParticleSystems;
    // private static OverlayTexture OverlayTexture;
    public static Transform Parent;
    public static void SoftInitialize(Transform parent){
        Transform particleSystemParent = new GameObject("ParticleSystems").transform;
        particleSystemParent.SetParent(parent);
        particleSystemParent.localPosition = new Vector3(0, 0, 0);
        particleSystemParent.localScale = new Vector3(1, 1, 1);

        Parent = particleSystemParent;
    }
    public static void Initialize(){
        Parent.SetAsFirstSibling();
        ParticleSystems = new List<ParticleSystemCompute>{
            new FalloutDustCompute(1000, Parent, 0.90f),
            new HazeAmbienceCompute(Parent, 0.95f),
            new GodRaysCompute(Parent, 0.95f),

            new FalloutDustCompute(500, Parent, 0.7f),
            new HazeAmbienceCompute(Parent, 0.8f),
            new GodRaysCompute(Parent, 0.85f),

            new FalloutDustCompute(250, Parent, 0.5f),
            new HazeAmbienceCompute(Parent, 0.7f),

            new FalloutDustCompute(50, Parent, 0.35f),
            new HazeAmbienceCompute(Parent, 0.5f),
        };

        // Prewarm
        float seconds = 60;
        float dt = 0.1f;
        for (int i = 0; i < seconds / dt; i++)
            Update(dt);
    }
    public static void Update(float dt){
        Vector2 newCameraPosition = new(CoreCamera.Camera.transform.position.x, CoreCamera.Camera.transform.position.y);
        float orthographicSize = CoreCamera.Camera.orthographicSize;
        foreach(ParticleSystemCompute particleSystem in ParticleSystems)
            particleSystem.Update(newCameraPosition, orthographicSize, dt);
        // OverlayTexture.Texture = ParticleSystems[0].RenderTexture;
    }

    public static void Release(){
        foreach(ParticleSystemCompute particleSystem in ParticleSystems)
            particleSystem.Release();
    }
}