using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    Image panel;

    void Awake()
    {
        // Sin este guard, el Systems duplicado de cada escena pisa Instance
        // y luego se destruye → Instance queda apuntando a un objeto muerto.
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        panel = GetComponent<Image>();
        SetAlpha(0f);
    }

    public void Flash(float seconds) { StartCoroutine(DoFlash(seconds)); }
    public void FadeOut(float seconds, Action onBlack) { StartCoroutine(DoFadeOut(seconds, onBlack)); }
    public void FadeIn(float seconds) { StartCoroutine(DoFadeIn(seconds)); }

    IEnumerator DoFlash(float s)
    {
        yield return FadeTo(1f, s * 0.5f);
        yield return FadeTo(0f, s * 0.5f);
    }

    IEnumerator DoFadeOut(float s, Action onBlack)
    {
        yield return FadeTo(1f, s * 0.5f);
        onBlack?.Invoke();
    }

    IEnumerator DoFadeIn(float s)
    {
        yield return FadeTo(0f, s);
    }

    IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (panel == null) yield break;
        float start = panel.color.a;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(start, targetAlpha, t / duration));
            yield return null;
        }
        SetAlpha(targetAlpha);
    }

    void SetAlpha(float a)
    {
        if (panel == null) return;
        Color c = panel.color;
        c.a = a;
        panel.color = c;
    }
}
