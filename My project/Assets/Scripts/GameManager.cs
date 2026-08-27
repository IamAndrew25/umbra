using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Vidas")]
    public int maxLives = 3;

    [Header("Transición (diseño: 2s de oscuridad)")]
    public float transitionSeconds = 2f;

    int lives;
    Vector2 activeCheckpoint;
    bool hasCheckpoint = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        lives = maxLives;
    }

    void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        lives = maxLives;
        // Resetear checkpoint: si el jugador muere antes de tocar uno en el
        // nuevo nivel, no debe reaparecer en coordenadas del nivel anterior.
        hasCheckpoint = false;
        ScreenFader.Instance?.FadeIn(1f);
    }

    public void RegisterCheckpoint(Vector2 position)
    {
        activeCheckpoint = position;
        hasCheckpoint = true;
    }

    public void OnPlayerDeath(GameObject player)
    {
        lives--;
        if (lives <= 0) lives = maxLives;

        PlayerAnimatorController anim = player.GetComponent<PlayerAnimatorController>();
        anim?.PlayDeath();

        ScreenFader.Instance?.Flash(0.4f);
        if (hasCheckpoint)
        {
            // Desparentar por si murió sobre una MovingPlatform,
            // si no el jugador seguiría pegado a la plataforma al reaparecer.
            player.transform.SetParent(null);
            player.transform.position = activeCheckpoint;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    public void LoadNextLevel()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        ScreenFader.Instance?.FadeOut(transitionSeconds, () =>
        {
            if (next < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(next);
        });
    }
}
