using UnityEngine;

public class TogglePlatform : MonoBehaviour
{
    public float interval = 1.5f;
    public float phaseOffset = 0f;

    Collider2D col;
    SpriteRenderer sr;

    void Start()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        InvokeRepeating(nameof(Toggle), phaseOffset + interval, interval);
    }

    void Toggle()
    {
        bool next = !col.enabled;
        col.enabled = next;
        if (sr != null) sr.enabled = next;
    }
}
