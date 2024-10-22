using UnityEngine;

public class CopyYToZ : MonoBehaviour
{
    public float Offset;
    void Update() => transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y + Offset);
}
