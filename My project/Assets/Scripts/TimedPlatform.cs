using UnityEngine;
using System.Collections;

public class TimedPlatform : MonoBehaviour
{
    public float activeDuration = 5f;

    Collider2D col;
    SpriteRenderer sr;
    Coroutine timer;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        SetActive(false);
    }

    public void Activate()
    {
        SetActive(true);
        if (timer != null) StopCoroutine(timer);
        timer = StartCoroutine(DeactivateAfter(activeDuration));
    }

    IEnumerator DeactivateAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SetActive(false);
    }

    void SetActive(bool value)
    {
        if (col != null) col.enabled = value;
        if (sr != null) sr.enabled = value;
    }
}
