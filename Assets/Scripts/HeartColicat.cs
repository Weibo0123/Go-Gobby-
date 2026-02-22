using UnityEngine;
using UnityEngine.UI;

public class HeartColicat : MonoBehaviour
{
    [SerializeField] private GameObject OnCollectEffect;
    [SerializeField] private int healAmount = 1;

    public float RotationSpeed;
    

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, 0f, RotationSpeed * Time.deltaTime);
    }

   

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            if (!other.CompareTag("Player")) return;

            // 1) زوّد صحة اللاعب + رجّع UI
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.Heal(healAmount);

            // 2) اعمل الإيفكت
            if (OnCollectEffect != null)
                Instantiate(OnCollectEffect, transform.position, Quaternion.identity);

            // 3) دمّر القلب اللي في الأرض
            Destroy(gameObject);
        }

    }
}
