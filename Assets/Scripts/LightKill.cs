using UnityEngine;

public class LightKill : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player died");
            Destroy(other.gameObject);
        }
    }
}
