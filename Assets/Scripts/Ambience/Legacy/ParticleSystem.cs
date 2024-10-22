using System.Collections.Generic;
using UnityEngine;
using System;

public class ParticleSystem
{
    // private LinkedList<Particle> Particles;
    private const int ChunkSize = 10;
    private const int ChunksX = 12;
    private const int ChunksY = 8;

    private LinkedList<Particle>[,] ParticleChunks;

    private Vector2Int RenderedChunkIndex;
    private Func<float, List<Particle>> CreateParticle;
    private float Counter;
    public const int ChunkBuffer = 8;
    
    private const int ProcessingSegments = 1;
    private int ProcessingSegment = 0;
    public ParticleSystem(Func<float, List<Particle>> CreateParticle)
    {
        int BoardSizeX = (int)BoardRender.Instance.BoardBounds[1] + 1;
        int BoardSizeY = (int)BoardRender.Instance.BoardBounds[3] + 1;
        ParticleChunks = new LinkedList<Particle>[BoardSizeX / ChunkSize + ChunkBuffer * 2, BoardSizeY / ChunkSize + ChunkBuffer * 2];
        for (int i = 0; i < ParticleChunks.GetLength(0); i++)
            for (int j = 0; j < ParticleChunks.GetLength(1); j++)
                ParticleChunks[i, j] = new LinkedList<Particle>();

        RenderedChunkIndex = new Vector2Int(0, 0);

        this.CreateParticle = CreateParticle;
    }

    public void Clear()
    {
        ForEachRenderedParticle((particle) => particle.AssociatedParticle.Return());
        for (int i = 0; i < ParticleChunks.GetLength(0); i++)
            for (int j = 0; j < ParticleChunks.GetLength(1); j++)
                ParticleChunks[i, j].Clear();
    }

    public void Update(Vector2 newPosition, float dt)
    {
        EmitParticles(dt);
        List<RealParticle> particlesToReturn = UpdateParticles(dt);
        foreach (RealParticle particle in particlesToReturn)
            particle.Return();
        ReRender(newPosition);
    }
    private void EmitParticles(float dt)
    {
        Counter += dt;
        List<Particle> newParticles = CreateParticle(Counter);
        if (newParticles != null){
            Counter = 0;
            foreach (Particle newParticle in newParticles){
                Vector2Int chunkIndex = ToChunkIndex(newParticle.GetPosition());
                ParticleChunks[chunkIndex.x, chunkIndex.y].AddLast(newParticle);
            }
        }
    }
    private void ReRender(Vector2 newPosition){
        Vector2Int newChunkIndex = ToChunkIndex(newPosition);
        if (RenderedChunkIndex != newChunkIndex)
        {
            List<Vector2Int> oldChunkIndices = RenderedChunkIndices();
            RenderedChunkIndex = newChunkIndex;
            List<Vector2Int> newChunkIndices = RenderedChunkIndices();

            List<Vector2Int> culledChunkIndices = Subtract(oldChunkIndices, newChunkIndices);
            List<Vector2Int> renderedChunkIndices = Subtract(newChunkIndices, oldChunkIndices);
            foreach (Vector2Int chunkIndex in culledChunkIndices)
                foreach (Particle particle in ParticleChunks[chunkIndex.x, chunkIndex.y]){
                    particle.AssociatedParticle?.Return();  
                    particle.AssociatedParticle = null;
                }
            foreach (Vector2Int chunkIndex in renderedChunkIndices)
                foreach (Particle particle in ParticleChunks[chunkIndex.x, chunkIndex.y])
                    particle.AssociatedParticle = RealParticle.Get().BindTo(particle);

        }
        List<Vector2Int> renderedChecks = RenderedChunkIndices();
        foreach (Vector2Int chunkIndex in renderedChecks){
            foreach (Particle particle in ParticleChunks[chunkIndex.x, chunkIndex.y]){
                particle.AssociatedParticle ??= RealParticle.Get().BindTo(particle);
                particle.AssociatedParticle.Transform.position = particle.GetPosition();
                particle.AssociatedParticle.Sr.sprite = particle.GetSprite();
                particle.AssociatedParticle.Sr.color = particle.GetColor();
            }
        }
    }

    private struct ParticleBind{
        public int x;
        public int y;
        public Particle particle;
        public ParticleBind(int x, int y, Particle particle){
            this.x = x;
            this.y = y;
            this.particle = particle;
        }
    }
    private List<RealParticle> UpdateParticles(float dt)
    {
        List<RealParticle> realParticlesToReturn = new();
        LinkedList<ParticleBind> particlesToRemove = new();
        LinkedList<ParticleBind> particlesToAdd = new();
        for(int x = 0; x < ParticleChunks.GetLength(0); x++){
            for(int y = 0; y < ParticleChunks.GetLength(1); y++){
                int idx = x + y * ParticleChunks.GetLength(0);
                if (idx % ProcessingSegments != ProcessingSegment)
                    continue;
                LinkedList<Particle> particles = ParticleChunks[x, y];
                foreach (Particle particle in particles){
                    particle.Update(dt * ProcessingSegments);

                    if (particle.IsDead()){
                        particlesToRemove.AddLast(new ParticleBind(x, y, particle));
                    }else{
                        Vector2Int newParticleChunkIndex = ToChunkIndex(particle.GetPosition());
                        if (x != newParticleChunkIndex.x && y != newParticleChunkIndex.y){
                            if (!IsRenderedChunk(newParticleChunkIndex)){ // Entering a culled chunk
                                if (particle.AssociatedParticle != null){
                                    realParticlesToReturn.Add(particle.AssociatedParticle);
                                    particle.AssociatedParticle = null;
                                }
                            } 
                            particlesToRemove.AddLast(new ParticleBind(x, y, particle));
                            particlesToAdd.AddLast(new ParticleBind(newParticleChunkIndex.x, newParticleChunkIndex.y, particle));
                        }
                    }
                }
            }
        }

        foreach (ParticleBind particleBind in particlesToRemove){
            ParticleChunks[particleBind.x, particleBind.y].Remove(particleBind.particle);
            if (particleBind.particle.AssociatedParticle != null){
                realParticlesToReturn.Add(particleBind.particle.AssociatedParticle);
                particleBind.particle.AssociatedParticle = null;
            }
        }
        foreach (ParticleBind particleBind in particlesToAdd)
            ParticleChunks[particleBind.x, particleBind.y].AddLast(particleBind.particle);

        ProcessingSegment = (ProcessingSegment + 1) % ProcessingSegments;
        return realParticlesToReturn;
    }

    public int GetTotalParticles()
    {
        int totalParticles = 0;
        for (int i = 0; i < ParticleChunks.GetLength(0); i++)
            for (int j = 0; j < ParticleChunks.GetLength(1); j++)
                totalParticles += ParticleChunks[i, j].Count;
        return totalParticles;
    }

    private List<Vector2Int> Subtract(List<Vector2Int> oldChunkIndices, List<Vector2Int> newChunkIndices)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        foreach (Vector2Int chunkIndex in oldChunkIndices)
            if (!newChunkIndices.Contains(chunkIndex))
                result.Add(chunkIndex);
        return result;
    }
    private List<Vector2Int> RenderedChunkIndices()
    {
        List<Vector2Int> chunkIndices = new List<Vector2Int>();
        int x0 = RenderedChunkIndex.x - ChunksX / 2;
        int x1 = RenderedChunkIndex.x + ChunksX / 2;
        int y0 = RenderedChunkIndex.y - ChunksY / 2;
        int y1 = RenderedChunkIndex.y + ChunksY / 2;

        for (int i = x0; i < x1; i++)
            for (int j = y0; j < y1; j++)
                if (i >= 0 && i < ParticleChunks.GetLength(0) && j >= 0 && j < ParticleChunks.GetLength(1))
                    chunkIndices.Add(new Vector2Int(i, j));
        return chunkIndices;
    }
    // private Vector2Int ToChunkIndex(Vector2 position) => new Vector2Int((int)(position.x / ChunkSize), (int)(position.y / ChunkSize));
    private Vector2Int ToChunkIndex(Vector2 position) => new Vector2Int((int)(position.x / ChunkSize + ChunkBuffer), (int)(position.y / ChunkSize + ChunkBuffer));
    private bool IsRenderedChunk(Vector2Int chunkIndex) => Mathf.Abs(chunkIndex.x - RenderedChunkIndex.x) < ChunksX / 2 && Mathf.Abs(chunkIndex.y - RenderedChunkIndex.y) < ChunksY / 2;
    private void ForEachRenderedParticle(Action<Particle> action)
    {
        foreach (Vector2Int chunkIndex in RenderedChunkIndices())
            foreach (Particle particle in ParticleChunks[chunkIndex.x, chunkIndex.y])
                action(particle);
    }
}
