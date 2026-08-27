using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// UMBRA — Constructor automático de los 5 niveles (menú: Umbra → Construir todos los niveles).
/// Crea las escenas en Assets/Scenes y las agrega a Build Settings.
/// Asigna sprites desde Assets/Sprites/ automáticamente.
/// </summary>
public static class UmbraLevelBuilder
{
    const string ScenesDir = "Assets/Scenes/";

    // Sprites del bosque (imágenes del usuario): capas de fondo parallax
    // Nombres reales de las imágenes JPEG en Assets/Sprites/
    static readonly string[] ForestBgKeys = { "1", "2", "3", "4" };
    static Sprite[] s_forestBg;
    const string SpritesDir = "Assets/Sprites/";
    static readonly string[] LevelNames = { "Nivel1_Bosque", "Nivel2_Ruinas", "Nivel3_Fabrica", "Nivel4_Cavernas", "Nivel5_Escape" };

    // Cache de sprites: nombres clave -> Sprite importado en Unity.
    static Dictionary<string, Sprite> s_sprites;

    /// Devuelve un GameObject con un SpriteRenderer asignado (con sprite real) o null si no hay sprite para esa categoría.
    // Mapeo de clase de juego -> nombre de sprite para lookup en s_sprites.
    static readonly string[] ClassToSpriteKey = { "Player", "Ground", "Box", "Hazard", "Crystal", "Fog" };

    /// Devuelve un SpriteRenderer asignado (con sprite real desde el cache) o null si no hay match.
    static GameObject AddVisual(GameObject go, string spriteNameHint)
    {
        var sr = go.GetComponent<SpriteRenderer>() ?? go.AddComponent<SpriteRenderer>();
        if (s_sprites == null || !s_sprites.TryGetValue(spriteNameHint, out Sprite sp)) return go;
        sr.sprite = sp;
        sr.color = Color.white;
        // Ajustar tamaño visual basado en collider para que colliding sea aproximadamente correcto.
        Collider2D col = go.GetComponent<Collider2D>();
        if (col != null)
        {
            BoxCollider2D bc = col as BoxCollider2D;
            float colliderPixels = bc == null ? 1f : Mathf.Max(bc.size.x, bc.size.y);
            int spritePxW = Mathf.Max(1, Mathf.FloorToInt(sp.rect.width));
            sr.size = new Vector2(colliderPixels, colliderPixels * sp.rect.height / spritePxW);
        }
        return go;
    }

    /// Carga por nombre de clase los primeros 6 sprites encontrados en SpritesDir.
    static void LoadSprites()
    {
        s_sprites = new Dictionary<string, Sprite>();
        string[] guids = AssetDatabase.FindAssets("t:sprite", new[] { SpritesDir });
        for (int i = 0; i < guids.Length && i < ClassToSpriteKey.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sp != null) s_sprites[ClassToSpriteKey[i]] = sp;
        }
    }

    static void LoadForestBackgrounds()
    {
        s_forestBg = new Sprite[4];
        for (int i = 0; i < 4; i++)
        {
            // Buscar por nombre del archivo JPEG sin extension
            string[] guids = AssetDatabase.FindAssets(ForestBgKeys[i]);
            if (guids.Length == 0) continue;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!assetPath.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase)) continue;
                s_forestBg[i] = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (s_forestBg[i] != null) break;
            }
        }
    }

    [MenuItem("Umbra/Construir todos los niveles")]
    static void BuildAll()
    {
        LoadSprites();
        LoadForestBackgrounds();
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (!EnsureScenesFolder()) return;

        for (int i = 0; i < LevelNames.Length; i++)
        {
            string name = LevelNames[i];
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = name;
            BuildCommon(scene);
            switch (i)
            {
                case 0: BuildLevel1(scene); break;
                case 1: BuildLevel2(scene); break;
                case 2: BuildLevel3(scene); break;
                case 3: BuildLevel4(scene); break;
                case 4: BuildLevel5(scene); break;
            }
            string path = ScenesDir + name + ".unity";
            EditorSceneManager.SaveScene(scene, path);
        }
        SyncBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("Umbra: niveles construidos y agregados a Build Settings.");
    }

    [MenuItem("Umbra/Limpiar niveles")]
    static void Clean()
    {
        foreach (string name in LevelNames)
        {
            string path = ScenesDir + name + ".unity";
            AssetDatabase.DeleteAsset(path);
        }
        Debug.Log("Umbra: escenas eliminadas de Assets/Scenes.");
    }

    static bool EnsureScenesFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        return true;
    }

    static void BuildCommon(Scene scene)
    {
        // Systems: GameManager + Canvas con ScreenFader
        GameObject systems = new GameObject("Systems");
        systems.AddComponent<GameManager>();
        Canvas canvas = new GameObject("Canvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.transform.SetParent(systems.transform);
        Image img = new GameObject("FaderImage").AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 1f);
        // No se necesita un sprite para el FaderImage (es negro sólido)

        // AudioSource ambiental opcional
        systems.AddComponent<AudioSource>().playOnAwake = true;

        // Main Camera con CameraFollow 
        GameObject cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        cam.AddComponent<Camera>();
        cam.AddComponent<CameraFollow>();
    }

    static GameObject NewPlayer(Vector2 pos)
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = pos;
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        CapsuleCollider2D col = player.AddComponent<CapsuleCollider2D>();
        // Dirección de capsule (1=vertical/horizontal, 2=y/x/z)
        col.direction = 0; // horizontal box collider style for side-scroller
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerAnimatorController>();
        // Add visual sprite + renderer
        AddVisual(player, "Player");
        // GroundCheck hijo (en los pies)
        GameObject gc = new GameObject("GroundCheck");
        gc.transform.SetParent(player.transform, false);
        gc.transform.position = new Vector2(pos.x, pos.y - 0.9f);
        player.GetComponent<PlayerController>().groundCheck = gc.transform;
        return player;
    }

    static GameObject NewGround(string name, Vector2 pos, Vector2 size, string spriteType = "Ground")
    {
        GameObject g = new GameObject(name);
        g.transform.position = pos;
        BoxCollider2D c = g.AddComponent<BoxCollider2D>();
        c.size = size;
        AddVisual(g, spriteType);
        return g;
    }

    static GameObject NewTrigger(string name, Vector2 pos, Vector2 size, System.Type componentType, string spriteKey = null)
    {
        GameObject g = new GameObject(name);
        g.transform.position = pos;
        BoxCollider2D c = g.AddComponent<BoxCollider2D>();
        c.size = size;
        c.isTrigger = true;
        if (spriteKey != null) AddVisual(g, spriteKey);
        g.AddComponent(componentType);
        return g;
    }

    static GameObject NewPatrolEnemy(string name, Vector2 pos, float speed, float hopForce = 0f, float hopInterval = 2f)
    {
        GameObject e = new GameObject(name);
        e.transform.position = pos;
        Rigidbody2D rb = e.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        e.AddComponent<BoxCollider2D>();
        AddVisual(e, "Box"); // Enemy usa placeholder cuadrado tipo Limbo
        PatrolEnemy p = e.AddComponent<PatrolEnemy>();
        p.speed = speed;
        p.hopForce = hopForce;
        p.hopInterval = hopInterval;
        return e;
    }

    static GameObject NewMovingPlatform(string name, Vector2[] points, float speed)
    {
        GameObject p = new GameObject(name);
        MovingPlatform mp = p.AddComponent<MovingPlatform>();
        mp.points = points;
        mp.speed = speed;
        BoxCollider2D c = p.AddComponent<BoxCollider2D>();
        AddVisual(p, "Ground"); // Plataformas móviles usan sprite Ground
        return p;
    }

    static GameObject NewTogglePlatform(string name, Vector2 pos, float interval, float phaseOffset)
    {
        GameObject p = new GameObject(name);
        p.transform.position = pos;
        BoxCollider2D c = p.AddComponent<BoxCollider2D>();
        TogglePlatform tp = p.AddComponent<TogglePlatform>();
        tp.interval = interval;
        tp.phaseOffset = phaseOffset;
        AddVisual(p, "Ground"); // Plataformas alternantes usan Ground
        return p;
    }

    static GameObject NewTimedPlatform(string name, Vector2 pos, float duration)
    {
        GameObject p = new GameObject(name);
        p.transform.position = pos;
        BoxCollider2D c = p.AddComponent<BoxCollider2D>();
        TimedPlatform tp = p.AddComponent<TimedPlatform>();
        tp.activeDuration = duration;
        AddVisual(p, "Ground"); // Plataformas temporales usan Ground
        return p;
    }

    static GameObject NewLever(string name, Vector2 pos, TimedPlatform[] targets)
    {
        GameObject l = new GameObject(name);
        l.transform.position = pos;
        BoxCollider2D c = l.AddComponent<BoxCollider2D>();
        c.isTrigger = true;
        AddVisual(l, "Fog"); // Palanca usa placeholder tipo Fog
        Lever lever = l.AddComponent<Lever>();
        lever.targets = targets;
        return l;
    }

    static GameObject NewParallaxLayer(string name, float yOffset, float parallaxFactor, int forestBgIndex)
    {
        GameObject layer = new GameObject(name);
        SpriteRenderer sr = layer.AddComponent<SpriteRenderer>();
        // Fondo full-screen: 40x25 unidades para camara ortográfica estándar (aspecto 16:9)
        layer.transform.localScale = new Vector3(40f, 25f, 1f);
        if (forestBgIndex >= 0 && forestBgIndex < s_forestBg.Length && s_forestBg[forestBgIndex] != null)
        {
            sr.sprite = s_forestBg[forestBgIndex];
            sr.transform.position = new Vector3(20f, yOffset, -10 - forestBgIndex);
        }
        else
        {
            Debug.LogWarning($"Umbra: No se encontró sprite forest background #{forestBgIndex}. Verificar nombre en Assets/Sprites/.");
        }
        // Hacer el objeto hijo de un contenedor de fondo para parallax
        GameObject bgHolder = new GameObject("BackgroundLayer " + name);
        bgHolder.transform.position = layer.transform.position;
        layer.transform.SetParent(bgHolder.transform, false);
        return layer;
    }

    static void BuildLevel1(Scene scene)
    {
        // Capas de fondo forestal parallax (de atrás hacia adelante)
        GameObject bgLayer1 = new GameObject("BG_LayerDeeps");
        bgLayer1.tag = "Untagged";
        SpriteRenderer sd = bgLayer1.AddComponent<SpriteRenderer>();
        if (s_forestBg != null && s_forestBg[0] != null)
        {
            sd.sprite = s_forestBg[0];
            sd.transform.position = new Vector3(20, 0, -20); // Lejos: más atrás en Z
            sd.transform.localScale = new Vector3(40f, 25f, 1f); // Pantalla completa aprox
        }

        GameObject bgLayer2 = new GameObject("BG_LayerMid");
        SpriteRenderer sm = bgLayer2.AddComponent<SpriteRenderer>();
        if (s_forestBg != null && s_forestBg[1] != null)
        {
            sm.sprite = s_forestBg[1];
            sm.transform.position = new Vector3(20, 0, -10); // Medio: más adelante en Z
            sm.transform.localScale = new Vector3(40f, 25f, 1f);
        }

        GameObject bgLayer3 = new GameObject("BG_LayerClose");
        SpriteRenderer sc = bgLayer3.AddComponent<SpriteRenderer>();
        if (s_forestBg != null && s_forestBg[2] != null)
        {
            sc.sprite = s_forestBg[2];
            sc.transform.position = new Vector3(20, 0, -5);  // Cerca: más adelante en Z
            sc.transform.localScale = new Vector3(40f, 25f, 1f);
        }

        NewPlayer(new Vector2(0, 1));
        NewGround("Ground_Main", new Vector2(10, 0), new Vector2(30, 1));
        // Zig-zag troncos
        NewGround("Tronco1", new Vector2(8, 1), new Vector2(2, 0.5f));
        NewGround("Tronco2", new Vector2(11, 2), new Vector2(2, 0.5f));
        NewGround("Tronco3", new Vector2(14, 3), new Vector2(2, 0.5f));
        NewGround("Tronco4", new Vector2(17, 2), new Vector2(2, 0.5f));
        NewGround("Pasillo", new Vector2(37, 0), new Vector2(40, 1));
        NewTrigger("Checkpoint", new Vector2(55, 1), new Vector2(1, 2), typeof(Checkpoint));
        NewTrigger("GoalZone", new Vector2(58, 1), new Vector2(1, 3), typeof(GoalZone));
    }

    static void BuildLevel2(Scene scene)
    {
        NewPlayer(new Vector2(0, 1));
        NewGround("Inicio", new Vector2(0, 0), new Vector2(20, 1));
        NewGround("Plataforma1", new Vector2(12, 2), new Vector2(1.5f, 0.5f));
        NewGround("Plataforma2", new Vector2(15, 3), new Vector2(1.5f, 0.5f));
        NewGround("Plataforma3", new Vector2(24, 2), new Vector2(1.5f, 0.5f));
        NewTrigger("Hazard_Pinchos", new Vector2(34, 0), new Vector2(8, 1), typeof(Hazard), "Hazard");
        NewGround("Final", new Vector2(40, 0), new Vector2(20, 1));
        NewTrigger("Checkpoint", new Vector2(30, 1), new Vector2(1, 2), typeof(Checkpoint));
        NewPatrolEnemy("Lagarto", new Vector2(42, 1), 1.5f);
        NewTrigger("GoalZone", new Vector2(46, 1), new Vector2(1, 3), typeof(GoalZone));
    }

    static void BuildLevel3(Scene scene)
    {
        GameObject player = NewPlayer(new Vector2(0, 1));
        NewGround("Inicio", new Vector2(0, 0), new Vector2(20, 1));
        // Cajas empujables (Hermes asignará layer "Pushable") -- usar sprite Box
        NewGround("Box1", new Vector2(10, 1), new Vector2(1, 1), "Box");
        NewGround("Box2", new Vector2(11, 1), new Vector2(1, 1), "Box");
        // Palanca + puente temporal 5s
        TimedPlatform tp = NewTimedPlatform("Puente5s", new Vector2(20, 1), 5f).GetComponent<TimedPlatform>();
        NewLever("Palanca", new Vector2(15, 1), new TimedPlatform[] { tp });
        NewGround("PostPuente", new Vector2(30, 0), new Vector2(20, 1));
        NewPatrolEnemy("Insecto", new Vector2(32, 1), 3.5f);
        NewTrigger("Checkpoint", new Vector2(25, 1), new Vector2(1, 2), typeof(Checkpoint));
        NewTrigger("GoalZone", new Vector2(38, 1), new Vector2(1, 3), typeof(GoalZone));
    }

    static void BuildLevel4(Scene scene)
    {
        GameObject player = NewPlayer(new Vector2(0, 1));
        NewGround("Inicio", new Vector2(0, 0), new Vector2(15, 1));
        // Plataformas alternantes
        NewTogglePlatform("Alt1", new Vector2(12, 1), 1.5f, 0f);
        NewTogglePlatform("Alt2", new Vector2(14, 2), 1.5f, 0.5f);
        NewTogglePlatform("Alt3", new Vector2(16, 3), 1.5f, 1f);
        // Cristal decorativo con visual real
        GameObject c = new GameObject("Cristal");
        c.transform.position = new Vector2(14, 2.5f);
        AddVisual(c, "Crystal");
        // Araña (enemy con Box sprite)
        NewPatrolEnemy("Arana", new Vector2(18, 1), 2f, 6f, 1.5f);
        NewTrigger("Checkpoint", new Vector2(16, 1), new Vector2(1, 2), typeof(Checkpoint));
        NewTrigger("GoalZone", new Vector2(20, 1), new Vector2(1, 3), typeof(GoalZone));
    }

    static void BuildLevel5(Scene scene)
    {
        GameObject player = NewPlayer(new Vector2(0, 1));
        // Fase A: plataformas móviles
        NewGround("Start", new Vector2(0, 0), new Vector2(10, 1));
        NewMovingPlatform("Mov1", new Vector2[] { new Vector2(8, 2), new Vector2(14, 2) }, 2f);
        NewMovingPlatform("Mov2", new Vector2[] { new Vector2(16, 3), new Vector2(22, 3) }, 2f);
        NewTrigger("CheckA", new Vector2(24, 1), new Vector2(1, 2), typeof(Checkpoint));
        // Fase B: 3 cajas empujables -- usar sprite Box
        NewGround("Box1", new Vector2(26, 1), new Vector2(1, 1), "Box");
        NewGround("Box2", new Vector2(27, 1), new Vector2(1, 1), "Box");
        NewGround("Box3", new Vector2(28, 1), new Vector2(1, 1), "Box");
        TimedPlatform tp1 = NewTimedPlatform("TP1", new Vector2(31, 1), 5f).GetComponent<TimedPlatform>();
        TimedPlatform tp2 = NewTimedPlatform("TP2", new Vector2(33, 2), 5f).GetComponent<TimedPlatform>();
        NewLever("Palanca1", new Vector2(28, 1), new TimedPlatform[] { tp1 });
        NewLever("Palanca2", new Vector2(29, 2), new TimedPlatform[] { tp2 });
        NewTrigger("CheckB", new Vector2(35, 1), new Vector2(1, 2), typeof(Checkpoint));
        // Fase C: escalada vertical + personaje misterioso
        NewTogglePlatform("AltA", new Vector2(36, 2), 1.4f, 0.2f);
        NewTogglePlatform("AltB", new Vector2(38, 3), 1.4f, 0.6f);
        NewTogglePlatform("AltC", new Vector2(40, 4), 1.4f, 1.0f);
        NewGround("Torre", new Vector2(42, 5), new Vector2(4, 10));
        NewTrigger("GoalZoneFinal", new Vector2(42, 6), new Vector2(1, 3), typeof(GoalZone));
        // Personaje misterioso (decoración) -- visual Fog para silueta oscura
        GameObject m = new GameObject("Misterioso");
        m.transform.position = new Vector2(42, 7);
        AddVisual(m, "Fog");
    }

    static void SyncBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>();
        foreach (string name in LevelNames)
            scenes.Add(new EditorBuildSettingsScene(ScenesDir + name + ".unity", true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
