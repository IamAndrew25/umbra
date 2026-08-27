using UnityEngine;
using System.Collections.Generic;

public class PatrolEnemy : MonoBehaviour
{
    [Header("Patrulla")]
    public float speed = 2f;

    [Header("Salto vertical (araña). 0 = lagarto/insecto.")]
    public float hopForce = 0f;
    public float hopInterval = 2f;
    public float groundCheckDistance = 0.6f;

    public LayerMask groundLayer;

    Rigidbody2D rb;
    int dir = 1;
    float hopTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        hopTimer = hopInterval;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        if (hopForce > 0f)
        {
            bool grounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
            hopTimer -= Time.fixedDeltaTime;
            if (grounded && hopTimer <= 0f)
            {
                rb.linearVelocity = new Vector2(dir * speed, hopForce);
                hopTimer = hopInterval;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            GameManager.Instance.OnPlayerDeath(collision.collider.gameObject);
            return;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                dir = -dir;
                break;
            }
        }
    }
}
