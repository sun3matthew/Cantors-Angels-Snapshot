using UnityEngine;
using UnityEngine.UI;

public abstract class ParticleSystemCompute
{
    protected const float ArbXScale = 47.0f;
    protected const float ArbYScale = 46.6f;

    private const int GroupSize = 32;
    // private const int BaseScreenWidth = 1920;
    // private const int BaseScreenHeight = 1080;
    private const int BaseScreenWidth = 1920 * 2;
    private const int BaseScreenHeight = 1080 * 2;
    protected ComputeShader ParticleShader;

    public RenderTexture RenderTexture;
    protected ComputeBuffer ParticleBuffer { get; private set; }

    protected float Counter;
    protected int EmitIdx;
    protected int MaxParticles;
    private float ForegroundScale;

    public struct Particle{
        public float x, y, vx, vy, ax, ay, Life;
    }

    protected abstract Sprite GetSprite();
    protected abstract void Emit(float dt);

    private RawImage rawImage;
    public ParticleSystemCompute(int maxParticles, Transform parent, float foregroundScale)
    {
        MaxParticles = maxParticles;
        ForegroundScale = foregroundScale;

        string shaderName = GetType().Name;
        shaderName = shaderName.Substring(0, shaderName.Length - "Compute".Length);
        ParticleShader = Object.Instantiate(Resources.Load<ComputeShader>("Compute/" + shaderName));

        Particle[] particles = new Particle[MaxParticles];
        for (int i = 0; i < particles.Length; i++)
            particles[i] = new Particle();
        ParticleBuffer = new ComputeBuffer(particles.Length, sizeof(float) * 7);

        ParticleBuffer.SetData(particles);
        ParticleShader.SetBuffer(0, "Particles", ParticleBuffer);
        ParticleShader.SetInt("NumParticles", particles.Length);

        int screenWidth = BaseScreenWidth;
        int screenHeight = BaseScreenHeight;
        RenderTexture = new RenderTexture(screenWidth, screenHeight, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear){
            filterMode = FilterMode.Point,
            enableRandomWrite = true
        };
        RenderTexture.Create();
        ParticleShader.SetTexture(0, "Result", RenderTexture);
        ParticleShader.SetInts("ResultSize", new int[] { screenWidth, screenHeight });

        Texture2D texture = GetSprite().texture;
        ParticleShader.SetTexture(0, "ParticleTexture", texture);
        ParticleShader.SetInts("ParticleTextureSize", new int[] { texture.width, texture.height });

        ParticleShader.SetFloats("BoardMaxSize", new float[] { BoardRender.Instance.BoardBounds[1], BoardRender.Instance.BoardBounds[3] });

        Counter = Random.value * 0.1f; // soft random to prevent spikes
        EmitIdx = 0;

        GameObject rawImageObject = new("RawImage");
        rawImageObject.transform.SetParent(parent);
        rawImageObject.transform.localPosition = Vector3.zero;
        rawImageObject.AddComponent<RectTransform>();
        rawImage = rawImageObject.AddComponent<RawImage>();
        rawImage.material = new Material(Shader.Find("Sprites/Default"));
        rawImage.texture = RenderTexture;
        rawImage.rectTransform.sizeDelta = new Vector2(screenWidth, screenHeight);
        // rawImage.enabled = false;
    }

    public void Update(Vector3 cameraPosition, float orthographicSize, float dt)
    {
        Emit(dt);

        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = RenderTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = rt;

        ParticleShader.SetFloat("Dt", dt);
        ParticleShader.SetFloat("RandSeed", Random.value);
        ParticleShader.SetFloats("CameraPosition", new float[] { cameraPosition.x, cameraPosition.y });
        ParticleShader.Dispatch(0, MaxParticles/GroupSize, 1, 1);

        float screenCompensation = (float)Screen.width / BaseScreenWidth;
        screenCompensation *= 2;
        screenCompensation *= 1.6f;
        screenCompensation /= ForegroundScale;
        screenCompensation /= rawImage.transform.parent.parent.localScale.x; // Bad code.
        float cameraDiff = CoreCamera.maxZoom/(float)orthographicSize;
        rawImage.transform.localScale = new Vector3(cameraDiff * screenCompensation, cameraDiff * screenCompensation, 1);
    }

    public virtual void Release()
    {
        RenderTexture.Release();
        ParticleBuffer.Release();
    }

    protected Vector2 RandomPosition() => new(Random.value * BoardRender.Instance.BoardBounds[1] * ArbXScale, Random.value * BoardRender.Instance.BoardBounds[3] * ArbYScale);
    protected float[] MaxSize() => new float[] { BoardRender.Instance.BoardBounds[1] * ArbXScale, BoardRender.Instance.BoardBounds[3] * ArbYScale };
    // protected Vector2 RandomPosition() => new(Random.value * 6000, Random.value * 6000);
}