using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement parameters
    [Header("Movement")]
    [SerializeField] float moveSpeed = 7f;
    [SerializeField] float acceleration = 30f;
    [SerializeField] float GroundDeceleration = 20f;
    [SerializeField] float AirDeceleration = 15f;

    [Header("Jump")]
    [SerializeField] LayerMask Ground;
    [SerializeField] float jumpForce = 12f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float checkRadius = 0.2f;
    [SerializeField] float jumpBuffer = 0.3f;
    [SerializeField] float coyoteTime = 0.3f;

    [Header("Gravity")]
    [SerializeField] float fallMultiplier = 2f;
    [SerializeField] float jumpCutMultiplier = 0.5f;
    [SerializeField] float jumpCutSmooth = 30f;

    Vector2 moveInput;
    bool isGrounded;
    bool isJumping;
    float jumpBufferTimer;
    float coyoteTimer;
    bool jumpHeld;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = 0;

        jumpHeld = Input.GetButton("Jump");

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, Ground);

        // Coyote time
        if (isGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        // Jump buffer
        if (Input.GetButtonDown("Jump")) jumpBufferTimer = jumpBuffer;
        else jumpBufferTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        // Horizontal movement
        float targetSpeed = moveInput.x * moveSpeed;

        float accelRate;
        if (Mathf.Abs(targetSpeed) > 0.01f) accelRate = acceleration;
        else accelRate = isGrounded ? GroundDeceleration : AirDeceleration;

        float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);

        // Sprite + run animation
        if (moveInput.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput.x < 0)
        {
            spriteRenderer.flipX = true;
        }

        // Jump (buffer + coyote)
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            isJumping = true;
        }

        HandleGravity();

        // Land reset
        if (isGrounded && rb.linearVelocity.y <= 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            isJumping = false;
        }
    }

    // Handle gravity and jump cut
    void HandleGravity()
    {
        float velY = rb.linearVelocity.y;

        // Jump cut (release jump early)
        if (velY > 0 && !jumpHeld)
        {
            float targetY = velY * jumpCutMultiplier;
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.MoveTowards(velY, targetY, jumpCutSmooth * Time.fixedDeltaTime)
            );
        }
        // Faster falling
        else if (velY < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else
        {
            isJumping = false;
        }
    }

}