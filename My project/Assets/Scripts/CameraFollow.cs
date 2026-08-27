using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float damping = 3f;
    public Vector2 offset = new Vector2(0f, 1.5f);

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 dest = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, dest, damping * Time.deltaTime);
    }
}
