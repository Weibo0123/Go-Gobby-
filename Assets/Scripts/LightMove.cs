using UnityEngine;

public class LightMove : MonoBehaviour
{
    public float speed = 2f;
    public float distance = 3f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * speed) * distance;
        transform.position = startPos + new Vector3(0, y, 0);
    }
}