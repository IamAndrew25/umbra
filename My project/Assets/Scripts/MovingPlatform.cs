using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Puntos del recorrido (posiciones de mundo, ej: (0,0) y (5,0))")]
    public Vector2[] points;
    public float speed = 2f;

    int index = 0;

    void FixedUpdate()
    {
        if (points == null || points.Length < 2) return;

        Vector2 target = points[index];
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.fixedDeltaTime);

        if ((Vector2)transform.position == target)
            index = (index + 1) % points.Length;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
            collision.transform.SetParent(transform);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
            collision.transform.SetParent(null);
    }
}
