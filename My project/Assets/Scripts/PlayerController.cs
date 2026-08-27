using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public float pushDragFactor = 0.6f; // velocidad al empujar cajas

    [Header("Chequeo de suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public LayerMask pushableLayer;

    Rigidbody2D rb;
    PlayerAnimatorController anim;

    float moveInput;
    bool jumpPressed;
    bool grounded;
    bool pushingBox;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        anim = GetComponent<PlayerAnimatorController>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space)) jumpPressed = true;
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Detectar caja a empujar (para animación + ajuste de velocidad)
        Vector2 dir = Vector2.right * moveInput;
        pushingBox = false;
        if (moveInput != 0 && grounded)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, 0.6f, pushableLayer);
            if (hit.collider != null) pushingBox = true;
        }

        anim?.SetState(Mathf.Abs(moveInput), grounded, rb.linearVelocity.y, pushingBox);
    }

    void FixedUpdate()
    {
        float speedFactor = pushingBox ? moveSpeed * pushDragFactor : moveSpeed;
        rb.linearVelocity = new Vector2(moveInput * speedFactor, rb.linearVelocity.y);

        if (jumpPressed && grounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        jumpPressed = false;
    }
}
