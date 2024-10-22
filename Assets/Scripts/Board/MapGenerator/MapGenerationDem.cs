using UnityEngine;
using Assets.Map;
using System.Threading.Tasks;

public class MapGenerationDem : MonoBehaviour
{
    Map _map;
    const int _textureScale = 50;
    GameObject _selector;
    public bool Regenerate;
    public int Seed;
    public float PerlinCheckValue = 0.3f;
    public Task task;

    void Update()
    {
        // if (_map != null && _map.SelectedCenter != null)
        // {
        //     _selector.transform.localPosition = new Vector3(_map.SelectedCenter.point.x, _map.SelectedCenter.point.y, 1);
        // }
        // if (Regenerate)
        // {
        //     Regenerate = false;
        //     Awake();
        // }
        if(task.IsCompleted)
            new MapTexture(_textureScale).AttachTexture(gameObject, _map);
    }

	void Awake ()
	{
        _selector = GameObject.Find("Selector");
        task = new Task(() => _map = GenerateMap(Seed));
        task.Start();
	}
    // async map generation
    private Map GenerateMap(int seed)
    {
        return new Map(seed);
    }
}