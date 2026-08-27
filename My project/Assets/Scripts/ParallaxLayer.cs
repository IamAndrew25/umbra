using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 1)]
    public float factor = 0.5f;

    float startX;
    float camStartX;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
        startX = transform.position.x;
        camStartX = cam.transform.position.x;
    }

    void LateUpdate()
    {
        if (cam == null) return;
        float dx = cam.transform.position.x - camStartX;
        float offset = dx * (1f - factor);
        transform.position = new Vector3(startX + offset, transform.position.y, transform.position.z);
    }
}
