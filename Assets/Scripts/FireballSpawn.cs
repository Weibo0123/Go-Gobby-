using UnityEngine;

public class FireballSpawn : MonoBehaviour
{
    public GameObject fireballPrefab;

    public Transform spawnPoint;

    [SerializeField] float spawnInterval = 3f;

    float lastSpawnTime = 0f;

    void Update()
    {
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            SpawnFireball();
            lastSpawnTime = Time.time;
        }
    }

    void SpawnFireball()
    {
        Instantiate(fireballPrefab, spawnPoint.position, Quaternion.identity);
    }
}