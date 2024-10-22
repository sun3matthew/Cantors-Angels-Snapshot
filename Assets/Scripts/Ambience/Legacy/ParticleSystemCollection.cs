using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ParticleSystemCollection{
    public static List<ParticleSystem> ParticleSystems;
    public static void Initialize(){
        RealParticle.Initialize();

        ParticleSystems = new List<ParticleSystem>{
            new (SpiceDust.Emit),
        };

        // Prewarm
        float seconds = 60;
        float dt = 0.5f;
        for (int i = 0; i < seconds / dt; i++)
            Update(dt);

    }
    public static void Update(float dt){
        Vector2 newCameraPosition = new(CoreCamera.Camera.transform.position.x, CoreCamera.Camera.transform.position.y);
        foreach(ParticleSystem particleSystem in ParticleSystems)
            particleSystem.Update(newCameraPosition, dt);

        // int count = 0;
        // foreach(ParticleSystem particleSystem in ParticleSystems)
        //     count += particleSystem.GetTotalParticles();
        // Debug.Log(count);
    }
}