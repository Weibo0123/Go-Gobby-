using UnityEngine;                
using System.Collections;          

public class PlayerController : MonoBehaviour
{
    // =========================
    // Movement parameters
    // =========================
    [Header("Movement")]                                  
    [SerializeField] float moveSpeed = 7f;                
    [SerializeField] float acceleration = 30f;            
    [SerializeField] float groundDeceleration = 20f;      
    [SerializeField] float airDeceleration = 15f;         

    // =========================
    // Jump parameters
    // =========================
    [Header("Jump")]                                      
    [SerializeField] LayerMask groundLayer;               // LayerMask 
    [SerializeField] float jumpForce = 12f;               
    [SerializeField] Transform groundCheck;                    
    [SerializeField] float checkRadius = 0.2f;                 
    [SerializeField] float jumpBuffer = 0.15f;                 
    [SerializeField] float coyoteTime = 0.15f;                    

    // =========================
    // Gravity parameters
    // =========================
    [Header("Gravity")]                                   
    [SerializeField] float gravityScale = 3f;             //  Rigidbody2D 
    [SerializeField] float fallMultiplier = 2f;           //  (feels better)
    [SerializeField] float jumpCutMultiplier = 0.5f;      // control the jmap 
    [SerializeField] float jumpCutSmooth = 30f;          

    // =========================
    // Knockback parameters
    // =========================
    [Header("Knockback Control")]                         
    [SerializeField] float controlRecoverTime = 0.2f;     // how many time it take to get the control back

    // =========================
    // Runtime variables
    // =========================
    Vector2 moveInput;                                    //  (x right / left)
    bool isGrounded;                                      
    float jumpBufferTimer;                                
    float coyoteTimer;                                    
    bool jumpHeld;                                             
    bool jumpDown;                                        

    float controlMultiplier = 1f;                         // (control knockback)

    // =========================
    // States
    // =========================
    enum PlayerState { Normal, Knockback }               
    PlayerState currentState = PlayerState.Normal;          

    // =========================
    // Components
    // =========================
    Rigidbody2D rb;                                       // Rigidbody2D for the player  
    SpriteRenderer spriteRenderer;                        // SpriteRenderer to   
    Animator animator;                                    // Animator عشان الأنيميشن

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();                 // امسك Rigidbody2D من نفس الـ GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();  // امسك SpriteRenderer من نفس الـ GameObject
        animator = GetComponent<Animator>();              // امسك Animator من نفس الـ GameObject
    }

    void Start()
    {
        rb.gravityScale = gravityScale;                   // ✅ استخدم gravityScale (يحذف Warning)
    }

    void Update()
    {
        HandleInputs();                                   // اقرأ input كل فريم (لازم في Update)

        // افحص هل اللاعب لامس الأرض (OverlapCircle حوالين groundCheck)
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,                         // مكان دائرة الفحص
            checkRadius,                                  // حجم الدائرة
            groundLayer                                   // الطبقات اللي تعتبر "أرض"
        );

        // لو على الأرض: رجّع coyoteTimer للمدة كاملة
        if (isGrounded)
            coyoteTimer = coyoteTime;                     // نقدر ننط حتى لو خرجنا من الأرض بشوية
        else
            coyoteTimer -= Time.deltaTime;                // قلل العداد مع الزمن

        // لو ضغط Jump هذا الفريم: شغّل bufferTimer
        if (jumpDown)
            jumpBufferTimer = jumpBuffer;                 // خزّن ضغطة القفز شوية
        else
            jumpBufferTimer -= Time.deltaTime;            // قلل العداد مع الزمن

        // امنع العدادات إنها تنزل تحت الصفر (تنضيف)
        if (coyoteTimer < 0f) coyoteTimer = 0f;           // clamp
        if (jumpBufferTimer < 0f) jumpBufferTimer = 0f;   // clamp

        // ✅ أنيميشن: شغّل/اقفل running بناءً على الحركة + grounded
        bool isRunning = Mathf.Abs(moveInput.x) > 0.01f && isGrounded;  // بيجري فقط وهو على الأرض وبيتتحرك
        animator.SetBool("isRunning", isRunning);         // ابعت للأنيميتر

        // ✅ أنيميشن القفز: لو مش grounded يبقى jumping
        animator.SetBool("isJumping", !isGrounded);       // لو في الهوا → Jump animation
    }

    void FixedUpdate()
    {
        // لو في knockback: منوقف تحكم طبيعي
        if (currentState == PlayerState.Knockback)        // لو حالة Knockback
            return;                                       // اخرج

        HandleMovement();                                 // حركة يمين/شمال (فيزياء)
        HandleJump();                                     // قفز (فيزياء)
        HandleGravity();                                  // تعديل الجاذبية (فيزياء)
    }

    void HandleInputs()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");     // -1 شمال, 0, +1 يمين
        moveInput.y = 0f;                                 // مش محتاجين y هنا
        jumpHeld = Input.GetButton("Jump");               // هل Space مضغوط وممسوك؟
        jumpDown = Input.GetButtonDown("Jump");           // هل Space اتضغط هذا الفريم؟
    }

    void HandleMovement()
    {
        // السرعة اللي عايزين نوصلها (target)
        float targetSpeed = moveInput.x * moveSpeed * controlMultiplier; // controlMultiplier يقلل التحكم وقت الضرب

        // حدد accelRate: تسريع لو بتحرك، تباطؤ لو سايب
        float accelRate;                                  // متغير للتسارع/التباطؤ

        if (Mathf.Abs(targetSpeed) > 0.01f)               // لو فعلاً عايز تتحرك
            accelRate = acceleration;                     // سرّع
        else
            accelRate = isGrounded ? groundDeceleration : airDeceleration; // بطّئ حسب أرض/هوا

        // حرّك سرعة x تدريجياً لحد targetSpeed
        float newSpeedX = Mathf.MoveTowards(
            rb.linearVelocity.x,                          // السرعة الحالية x
            targetSpeed,                                  // السرعة الهدف
            accelRate * Time.fixedDeltaTime               // قد ايه تتحرك ناحية الهدف
        );

        // طبق السرعة الجديدة على Rigidbody
        rb.linearVelocity = new Vector2(newSpeedX, rb.linearVelocity.y); // حافظ على y زي ما هو

        // قلب الشخصية حسب الاتجاه
        if (moveInput.x > 0.01f)                          // لو رايح يمين
            spriteRenderer.flipX = false;                 // خليها تبص يمين
        else if (moveInput.x < -0.01f)                    // لو رايح شمال
            spriteRenderer.flipX = true;                  // خليها تبص شمال
    }

    void HandleJump()
    {
        // لو عندنا jumpBuffer شغال + coyote شغال → اعمل قفزة
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)     // الشرطين مع بعض
        {
            // نفّذ القفزة
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); // حط سرعة y للقفز

            // صفّر التايمرز عشان ما يعملش قفزات متعددة
            jumpBufferTimer = 0f;                         // خلصنا البافر
            coyoteTimer = 0f;                             // خلصنا الكايوتي
        }

        // لو على الأرض وسرعة y نازلة/صفر: ثبّت y = 0 (تنضيف صغير)
        if (isGrounded && rb.linearVelocity.y <= 0f)      // واقف على الأرض ومش طالع
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // خلي y صفر عشان مايبقاش فيه “اهتزاز”
        }
    }

    void HandleGravity()
    {
        float velY = rb.linearVelocity.y;                 // سرعة y الحالية

        // لو طالع لفوق
        if (velY > 0f)
        {
            // لو سبت زرار القفز بدري → قلل الطلوع (Jump Cut)
            if (!jumpHeld)
            {
                float targetY = velY * jumpCutMultiplier; // السرعة الهدف بعد القص
                float newY = Mathf.MoveTowards(           // نزّل y تدريجي
                    velY,
                    targetY,
                    jumpCutSmooth * Time.fixedDeltaTime
                );

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, newY); // طبق y الجديدة
            }
        }
        // لو نازل لتحت
        else if (velY < 0f)
        {
            // زود السقوط (Fall Multiplier)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    // =========================
    // Knockback
    // =========================
    public void ApplyKnockback(Vector2 force, float duration)
    {
        if (currentState == PlayerState.Knockback)        // لو بالفعل في Knockback
            return;                                       // متعملش واحد تاني

        currentState = PlayerState.Knockback;             // ادخل حالة Knockback
        StartCoroutine(KnockbackCoroutine(force, duration)); // ابدأ Coroutine
    }

    IEnumerator KnockbackCoroutine(Vector2 force, float duration)
    {
        controlMultiplier = 0f;                           // اقفل التحكم
        rb.linearVelocity = Vector2.zero;                 // صفّر السرعة قبل الضربة
        rb.AddForce(force, ForceMode2D.Impulse);          // طبّق قوة knockback

        yield return new WaitForSeconds(duration);        // استنى مدة الضربة

        currentState = PlayerState.Normal;                // رجّع الحالة عادي

        float t = 0f;                                     // عداد للرجوع التدريجي
        while (t < controlRecoverTime)                    // طول ما لسه بيرجع
        {
            t += Time.deltaTime;                          // زود الزمن
            controlMultiplier = Mathf.Lerp(0f, 1f, t / controlRecoverTime); // رجّع التحكم تدريجي
            yield return null;                            // استنى فريم
        }

        controlMultiplier = 1f;                           // تأكيد إن التحكم رجع كامل
    }

    // =========================
    // Debug (Ground Check visualization)
    // =========================
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;                  // لو مفيش GroundCheck
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius); // ارسم دائرة الفحص في Scene
    }


    void OnDestroy()
    {
        Debug.Log($"{name} DESTROYED => {GetType().Name}");
    }
}