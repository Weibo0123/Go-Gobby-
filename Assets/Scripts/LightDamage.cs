using UnityEngine;

public class LightDamage : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform player;              // player layout
    [SerializeField] PlayerHealth playerHealth;     // or take the player health

    [Header("Damage")]
    [SerializeField] int damagePerTick = 1;         // how much damage 
    [SerializeField] float ticksPerSecond = 5f;     // how many time 
    [SerializeField] float range = 8f;              // light range 

    [Header("Knockback")]
    [SerializeField] float knockbackStrength = 1.5f; // Knockback
    [SerializeField] bool knockbackAwayFromLight = true;

    [Header("Line of Sight")]
    [SerializeField] LayerMask obstacleMask;        // Layers for the wall 
    [SerializeField] LayerMask playerMask;          // Layer for the player 

    float tickTimer;

    // to acsses the player health if not refrenced 
    void Awake()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerHealth == null)
            playerHealth = player.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // if not stop 
        if (player == null || playerHealth == null) return;

        // cheak if the player is close to the light 
        Debug.Log("player is here ");
        Vector2 origin = transform.position;
        Vector2 target = player.position;
        Vector2 dir = target - origin;
        float dist = dir.magnitude;
        // if not stop 
        if (dist > range) return;

        // 2) Line of Sight: hit the player first 
        RaycastHit2D hit = Physics2D.Raycast(origin, dir.normalized, dist, obstacleMask | playerMask);
        if (hit.collider == null) return;

        bool hitPlayer = ((1 << hit.collider.gameObject.layer) & playerMask) != 0;
        if (!hitPlayer) return; // see if there is wall than dont hit the player  

        // 3) Tick damage (int not float) 
        // to hit the player not just one but for how long he is there 
        tickTimer += Time.deltaTime;
        // how much dimege and how many time between each dimege 
        float tickInterval = 1f / Mathf.Max(0.01f, ticksPerSecond);
        
        if (tickTimer >= tickInterval)
        {
            // set the ticker back 
            tickTimer -= tickInterval;

            // Knockback direction
            Vector2 knockDir;

            if (knockbackAwayFromLight)
                knockDir = (target - origin).normalized; // to puch the player far 
            else
                knockDir = Vector2.up;

            Vector2 knockback = knockDir * knockbackStrength;

            playerHealth.TakeDamage(damagePerTick, knockback);
        }
    }
}