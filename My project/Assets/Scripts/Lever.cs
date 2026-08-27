using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("Objetos a activar")]
    public TimedPlatform[] targets;
    public KeyCode interactKey = KeyCode.E;

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (Input.GetKeyDown(interactKey))
        {
            foreach (TimedPlatform target in targets)
                if (target != null) target.Activate();
        }
    }
}
