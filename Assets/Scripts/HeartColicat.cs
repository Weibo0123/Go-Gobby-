using UnityEngine;

public class HeartColicat : MonoBehaviour
{
    public float RotationSpeed;
    public GameObject OnCollectEffect;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, 0f, RotationSpeed * Time.deltaTime);
    }

   

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {

            Destroy(gameObject);

            // Instantiate the particle effect
            Instantiate(OnCollectEffect, transform.position, transform.rotation);

        }

    }
}
