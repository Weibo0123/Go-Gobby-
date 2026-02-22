using UnityEngine;

public class FireballControler : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] private float speed = 5f;

    [SerializeField] private float destroyX = -25f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.flipX = true;
    }

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}