using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Editor-only scene generator for the Fish Balatro starting point. Use it when
// the generated art, prefabs, or Main scene need to be rebuilt from scratch.
public static class FishBalatroSceneBuilder
{
    private const string Root = "Assets/FishBalatro";
    private const string ArtPath = Root + "/Art/Generated";
    private const string PrefabPath = Root + "/Prefabs";
    private const string MainScenePath = "Assets/Scenes/Main.unity";
    private const float PixelsPerUnit = 16f;
    private const float UiPixelsPerUnit = 100f;

    private static readonly Color32 Clear = new Color32(0, 0, 0, 0);

    [MenuItem("Game Jam/Fish Balatro/Rebuild Main Scene")]
    public static void BuildMainScene()
    {
        // This intentionally writes only Main.unity. The repository is now a
        // Fish Balatro starter project, so there is no separate legacy scene.
        EnsureFolders();
        Dictionary<string, Sprite> sprites = GenerateSprites();
        BaitPickup[] baitPrefabs = CreateBaitPrefabs(sprites);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        FishGameManager gameManager = new GameObject("FishGameManager").AddComponent<FishGameManager>();
        gameManager.popupFont = LoadFont();

        CreateCamera();
        CreateEnvironment(sprites);

        FishermanController fisherman = CreateFisherman(sprites);
        FishPlayerController player = CreatePlayer(sprites);
        FishingLineView line = CreateFishingLine(gameManager, fisherman, player);
        Transform captureTools = new GameObject("Capture Tools").transform;
        NetSweepHazard netSweep = CreateNetSweepHazard(sprites["net"], captureTools);
        ClawShotHazard clawShot = CreateClawShotHazard(sprites["claw"], captureTools);
        ElectricWaveHazard electricWave = CreateElectricWaveHazard(captureTools);
        BaitSpawner spawner = CreateBaitSpawner(gameManager, player, baitPrefabs);
        BigFishAlly bigFish = CreateBigFish(sprites, gameManager);
        FishUIController ui = CreateUi(sprites);
        Transform respawn = CreateMarker("PlayerRespawn", new Vector3(0f, -1.2f, 0f));

        gameManager.player = player;
        gameManager.fisherman = fisherman;
        gameManager.fishingLine = line;
        gameManager.netSweep = netSweep;
        gameManager.clawShot = clawShot;
        gameManager.electricWave = electricWave;
        gameManager.baitSpawner = spawner;
        gameManager.bigFish = bigFish;
        gameManager.ui = ui;
        gameManager.playerRespawn = respawn;

        player.gameManager = gameManager;
        spawner.gameManager = gameManager;
        spawner.player = player;
        bigFish.gameManager = gameManager;

        EditorSceneManager.SaveScene(scene, MainScenePath);
        SetSceneFirstInBuildSettings(MainScenePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Fish Balatro main scene rebuilt at " + MainScenePath);
    }

    [MenuItem("Game Jam/Fish Balatro/Add Environment Animation Props")]
    public static void AddEnvironmentAnimationProps()
    {
        EnsureFolders();
        Dictionary<string, Sprite> sprites = GenerateSprites();

        if (SceneManager.GetActiveScene().path.Replace("\\", "/") != MainScenePath)
        {
            EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        }

        CreateEnvironmentAnimationProps(sprites);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Fish Balatro wave and seaweed animation props added.");
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory(ArtPath);
        Directory.CreateDirectory(PrefabPath);
        Directory.CreateDirectory("Assets/Scenes");
    }

    private static Dictionary<string, Sprite> GenerateSprites()
    {
        // Placeholder pixel art is generated in code so the repository starts
        // with no external art dependency. Replace these PNGs later with final art.
        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        sprites["ui_square"] = SaveSprite("ui_square", 16, 16, pixels =>
        {
            Rect(pixels, 16, 16, 0, 0, 16, 16, new Color32(255, 255, 255, 255));
        });

        sprites["ui_score_panel"] = SaveUiPanelSprite("ui_score_panel", 142, 78, pixels =>
        {
            DrawHudPanel(pixels, 142, 78, new Color32(96, 173, 222, 255));
        });

        sprites["ui_multiplier_panel"] = SaveUiPanelSprite("ui_multiplier_panel", 142, 78, pixels =>
        {
            DrawHudPanel(pixels, 142, 78, new Color32(255, 191, 48, 255));
        });

        sprites["ui_alert_panel"] = SaveUiPanelSprite("ui_alert_panel", 420, 60, pixels =>
        {
            DrawAlertPanel(pixels, 420, 60);
        });

        sprites["ui_controls_panel"] = SaveUiPanelSprite("ui_controls_panel", 260, 118, pixels =>
        {
            DrawHudPanel(pixels, 260, 118, new Color32(255, 86, 75, 255));
        });

        sprites["ui_keycap"] = SaveUiPanelSprite("ui_keycap", 62, 28, pixels =>
        {
            Rect(pixels, 62, 28, 0, 0, 61, 27, new Color32(30, 42, 58, 235));
            Rect(pixels, 62, 28, 5, 5, 51, 17, new Color32(51, 65, 82, 235));
            StrokeRect(pixels, 62, 28, 1, 1, 58, 24, new Color32(9, 15, 24, 255), 3);
            StrokeRect(pixels, 62, 28, 4, 4, 52, 18, new Color32(98, 126, 148, 255), 2);
        });

        sprites["ui_alert_segment"] = SaveSprite("ui_alert_segment", 26, 24, pixels =>
        {
            Rect(pixels, 26, 24, 1, 1, 24, 22, new Color32(92, 26, 34, 255));
            Rect(pixels, 26, 24, 3, 4, 20, 17, new Color32(188, 45, 51, 255));
            Rect(pixels, 26, 24, 4, 5, 18, 5, new Color32(255, 84, 74, 255));
        }, UiPixelsPerUnit, new Vector4(2f, 2f, 2f, 2f));

        sprites["water_panel"] = SaveSprite("water_panel", 32, 32, pixels =>
        {
            Rect(pixels, 32, 32, 0, 0, 32, 32, new Color32(8, 62, 96, 255));
            Rect(pixels, 32, 32, 0, 22, 32, 2, new Color32(18, 96, 128, 255));
            Rect(pixels, 32, 32, 0, 10, 32, 1, new Color32(13, 74, 112, 255));
            for (int i = 0; i < 32; i += 7)
            {
                Rect(pixels, 32, 32, i, 26, 3, 1, new Color32(47, 142, 168, 255));
            }
        });

        sprites["player_fish"] = SaveSprite("player_fish", 32, 18, pixels =>
        {
            Triangle(pixels, 32, 18, new Vector2Int(2, 9), new Vector2Int(10, 15), new Vector2Int(10, 3), new Color32(238, 132, 58, 255));
            Ellipse(pixels, 32, 18, 18, 9, 12, 6, new Color32(255, 181, 64, 255));
            Ellipse(pixels, 32, 18, 21, 10, 7, 4, new Color32(255, 205, 86, 255));
            Triangle(pixels, 32, 18, new Vector2Int(16, 14), new Vector2Int(20, 17), new Vector2Int(19, 12), new Color32(255, 226, 104, 255));
            Rect(pixels, 32, 18, 25, 10, 2, 2, new Color32(18, 31, 42, 255));
            Rect(pixels, 32, 18, 11, 8, 2, 2, new Color32(180, 79, 50, 255));
        });

        sprites["big_fish"] = LoadGeneratedSprite("Fish_Large_1.png.png") ?? SaveSprite("big_fish", 72, 36, pixels =>
        {
            Triangle(pixels, 72, 36, new Vector2Int(3, 18), new Vector2Int(22, 32), new Vector2Int(22, 4), new Color32(31, 119, 130, 255));
            Ellipse(pixels, 72, 36, 41, 18, 26, 13, new Color32(43, 166, 170, 255));
            Ellipse(pixels, 72, 36, 50, 19, 15, 8, new Color32(86, 210, 192, 255));
            Rect(pixels, 72, 36, 61, 19, 3, 3, new Color32(12, 26, 34, 255));
            Rect(pixels, 72, 36, 58, 9, 9, 2, new Color32(9, 24, 31, 255));
            Rect(pixels, 72, 36, 60, 11, 2, 3, new Color32(240, 250, 245, 255));
            Rect(pixels, 72, 36, 64, 11, 2, 3, new Color32(240, 250, 245, 255));
        });

        sprites["boat"] = SaveSprite("boat", 128, 36, pixels =>
        {
            Rect(pixels, 128, 36, 24, 13, 78, 9, new Color32(128, 76, 42, 255));
            Triangle(pixels, 128, 36, new Vector2Int(14, 22), new Vector2Int(24, 13), new Vector2Int(24, 22), new Color32(128, 76, 42, 255));
            Triangle(pixels, 128, 36, new Vector2Int(114, 22), new Vector2Int(102, 13), new Vector2Int(102, 22), new Color32(128, 76, 42, 255));
            Rect(pixels, 128, 36, 31, 22, 64, 4, new Color32(184, 114, 58, 255));
            Rect(pixels, 128, 36, 48, 8, 22, 3, new Color32(72, 45, 34, 255));
        });

        sprites["fisherman"] = SaveSprite("fisherman", 34, 52, pixels =>
        {
            Ellipse(pixels, 34, 52, 17, 39, 7, 7, new Color32(229, 181, 132, 255));
            Rect(pixels, 34, 52, 8, 45, 18, 4, new Color32(52, 42, 32, 255));
            Rect(pixels, 34, 52, 12, 21, 10, 15, new Color32(224, 66, 58, 255));
            Rect(pixels, 34, 52, 9, 17, 5, 10, new Color32(44, 71, 101, 255));
            Rect(pixels, 34, 52, 21, 17, 5, 10, new Color32(44, 71, 101, 255));
            Line(pixels, 34, 52, 24, 31, 33, 46, new Color32(68, 45, 30, 255));
            Rect(pixels, 34, 52, 14, 40, 2, 2, new Color32(18, 18, 18, 255));
            Rect(pixels, 34, 52, 20, 40, 2, 2, new Color32(18, 18, 18, 255));
        });

        sprites["water_wave"] = SaveSprite("water_wave", 160, 24, pixels =>
        {
            for (int x = 0; x < 160; x++)
            {
                int crestY = 12 + Mathf.RoundToInt(Mathf.Sin(x * 0.18f) * 4f);
                int lowerY = 15 + Mathf.RoundToInt(Mathf.Sin(x * 0.18f + 1.8f) * 3f);
                Rect(pixels, 160, 24, x, crestY, 1, 2, new Color32(117, 224, 246, 215));
                Rect(pixels, 160, 24, x, lowerY, 1, 2, new Color32(52, 151, 188, 145));

                int sparkle = x % 24;
                if (sparkle >= 3 && sparkle <= 7)
                {
                    Rect(pixels, 160, 24, x, crestY - 2, 1, 1, new Color32(220, 255, 255, 220));
                }
            }
        });

        sprites["seaweed"] = SaveSprite("seaweed", 48, 64, pixels =>
        {
            Color32[] bladeColors =
            {
                new Color32(25, 118, 82, 255),
                new Color32(32, 148, 96, 255),
                new Color32(61, 181, 118, 255),
                new Color32(17, 95, 72, 255)
            };

            int[] baseXs = { 8, 15, 22, 29, 37 };
            for (int i = 0; i < baseXs.Length; i++)
            {
                int baseX = baseXs[i];
                int topY = 12 + (i % 3) * 6;
                Color32 color = bladeColors[i % bladeColors.Length];
                int previousX = baseX;
                int previousY = 58;

                for (int step = 1; step < 7; step++)
                {
                    int y = Mathf.Max(topY, 58 - step * 7);
                    int x = baseX + Mathf.RoundToInt(Mathf.Sin(step * 0.9f + i) * 4f);
                    Line(pixels, 48, 64, previousX, previousY, x, y, color);
                    Line(pixels, 48, 64, previousX + 1, previousY, x + 1, y, color);
                    previousX = x;
                    previousY = y;
                }

                Line(pixels, 48, 64, baseX, 42, baseX - 5, 35, color);
                Line(pixels, 48, 64, baseX + 1, 34, baseX + 7, 28, color);
            }

            Rect(pixels, 48, 64, 4, 58, 40, 4, new Color32(31, 86, 64, 210));
        });

        sprites["claw"] = SaveSprite("claw", 24, 24, pixels =>
        {
            Line(pixels, 24, 24, 12, 22, 12, 7, new Color32(224, 230, 235, 255));
            Line(pixels, 24, 24, 12, 7, 5, 2, new Color32(224, 230, 235, 255));
            Line(pixels, 24, 24, 12, 7, 19, 2, new Color32(224, 230, 235, 255));
            Line(pixels, 24, 24, 5, 2, 4, 8, new Color32(224, 230, 235, 255));
            Line(pixels, 24, 24, 19, 2, 20, 8, new Color32(224, 230, 235, 255));
            Line(pixels, 24, 24, 9, 6, 15, 6, new Color32(164, 176, 188, 255));
            Rect(pixels, 24, 24, 11, 18, 3, 3, new Color32(138, 148, 158, 255));
        });

        sprites["net"] = SaveSprite("net", 128, 72, pixels =>
        {
            Color32 rope = new Color32(158, 236, 248, 230);
            Color32 edge = new Color32(214, 250, 255, 245);
            int width = 128;
            int height = 72;

            Line(pixels, width, height, 2, 2, 125, 2, edge);
            Line(pixels, width, height, 2, 69, 125, 69, edge);
            Line(pixels, width, height, 2, 2, 2, 69, edge);
            Line(pixels, width, height, 125, 2, 125, 69, edge);

            for (int x = 20; x <= 108; x += 22)
            {
                Line(pixels, width, height, x, 3, x, 68, rope);
            }

            for (int y = 18; y <= 54; y += 18)
            {
                Line(pixels, width, height, 3, y, 124, y, rope);
            }

            Line(pixels, width, height, 2, 2, 125, 69, rope);
            Line(pixels, width, height, 125, 2, 2, 69, rope);
        });

        sprites["worm"] = SaveExternalBaitSprite("bait_worm", 20, 16, 93f, pixels =>
        {
            Ellipse(pixels, 20, 16, 8, 8, 6, 4, new Color32(236, 120, 92, 255));
            Ellipse(pixels, 20, 16, 13, 8, 5, 4, new Color32(249, 151, 118, 255));
            Rect(pixels, 20, 16, 15, 10, 1, 1, new Color32(40, 24, 24, 255));
        });

        sprites["shrimp"] = SaveExternalBaitSprite("bait_shrimp", 22, 18, 28f, pixels =>
        {
            Ellipse(pixels, 22, 18, 11, 9, 8, 5, new Color32(255, 124, 102, 255));
            Triangle(pixels, 22, 18, new Vector2Int(4, 9), new Vector2Int(0, 13), new Vector2Int(1, 6), new Color32(255, 176, 138, 255));
            Rect(pixels, 22, 18, 17, 10, 2, 2, new Color32(24, 28, 32, 255));
            Line(pixels, 22, 18, 15, 6, 20, 1, new Color32(255, 176, 138, 255));
        });

        sprites["glow_bug"] = SaveExternalBaitSprite("bait_glow_bug", 20, 20, 62f, pixels =>
        {
            Ellipse(pixels, 20, 20, 10, 10, 8, 8, new Color32(94, 255, 148, 110));
            Ellipse(pixels, 20, 20, 10, 10, 4, 5, new Color32(165, 255, 132, 255));
            Rect(pixels, 20, 20, 9, 4, 2, 12, new Color32(37, 95, 64, 255));
        });

        sprites["small_fish_bait"] = SaveExternalBaitSprite("bait_small_fish", 26, 16, 64f, pixels =>
        {
            Triangle(pixels, 26, 16, new Vector2Int(1, 8), new Vector2Int(8, 13), new Vector2Int(8, 3), new Color32(62, 140, 206, 255));
            Ellipse(pixels, 26, 16, 15, 8, 9, 5, new Color32(86, 172, 226, 255));
            Rect(pixels, 26, 16, 21, 9, 2, 2, new Color32(18, 22, 28, 255));
        });

        sprites["golden_shrimp"] = SaveExternalBaitSprite("bait_golden_shrimp", 24, 20, 25f, pixels =>
        {
            Ellipse(pixels, 24, 20, 12, 10, 9, 5, new Color32(255, 212, 42, 255));
            Triangle(pixels, 24, 20, new Vector2Int(4, 10), new Vector2Int(0, 15), new Vector2Int(1, 5), new Color32(255, 242, 122, 255));
            Rect(pixels, 24, 20, 18, 11, 2, 2, new Color32(55, 36, 12, 255));
            Rect(pixels, 24, 20, 5, 17, 2, 2, new Color32(255, 255, 210, 255));
            Rect(pixels, 24, 20, 20, 3, 2, 2, new Color32(255, 255, 210, 255));
        });

        sprites["fake_bait"] = SaveExternalBaitSprite("bait_fake", 22, 18, 69f, pixels =>
        {
            Ellipse(pixels, 22, 18, 11, 9, 8, 5, new Color32(142, 74, 118, 255));
            Rect(pixels, 22, 18, 5, 6, 11, 2, new Color32(71, 34, 62, 255));
            Rect(pixels, 22, 18, 15, 10, 2, 2, new Color32(255, 72, 72, 255));
        });

        return sprites;
    }

    private static BaitPickup[] CreateBaitPrefabs(Dictionary<string, Sprite> sprites)
    {
        return new[]
        {
            CreateBaitPrefab(FishBaitType.Worm, sprites["worm"]),
            CreateBaitPrefab(FishBaitType.Shrimp, sprites["shrimp"]),
            CreateBaitPrefab(FishBaitType.GlowBug, sprites["glow_bug"]),
            CreateBaitPrefab(FishBaitType.SmallFish, sprites["small_fish_bait"]),
            CreateBaitPrefab(FishBaitType.GoldenShrimp, sprites["golden_shrimp"]),
            CreateBaitPrefab(FishBaitType.FakeBait, sprites["fake_bait"])
        };
    }

    private static BaitPickup CreateBaitPrefab(FishBaitType type, Sprite sprite)
    {
        // Prefabs are regenerated with labels so designers can edit them in the
        // Inspector after running the scene builder.
        string path = PrefabPath + "/Bait_" + type + ".prefab";
        GameObject root = new GameObject("Bait_" + type);
        SpriteRenderer spriteRenderer = root.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        // Keep sprite renderers white so the PNG artwork shows as-authored.
        // Tinting here made the art look like flat color blocks in the scene.
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 20;
        AssignSpriteMaterial(spriteRenderer);

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.55f;

        BaitPickup pickup = root.AddComponent<BaitPickup>();
        pickup.baitType = type;
        pickup.spriteRenderer = spriteRenderer;

        TextMeshPro label = CreateWorldText("Label", BaitPickup.GetStats(type).shortLabel, new Vector3(0f, -0.72f, 0f), 1.35f, Color.white, 32, root.transform);
        label.rectTransform.sizeDelta = new Vector2(3.2f, 1.4f);
        pickup.label = label;

        PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        return prefab.GetComponent<BaitPickup>();
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.backgroundColor = new Color(0.02f, 0.13f, 0.2f);
        camera.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void CreateEnvironment(Dictionary<string, Sprite> sprites)
    {
        // The MVP arena is intentionally simple: water bounds, surface danger,
        // and tutorial text. Obstacles can be added later once the core loop is
        // tuned.
        CreateSpriteObject("Water Background", sprites["water_panel"], Vector3.zero, new Vector3(18f, 10.4f, 1f), -20);
        CreateEnvironmentAnimationProps(sprites);

        GameObject surface = CreateSpriteObject("Water Surface", sprites["ui_square"], new Vector3(0f, 3.6f, 0f), new Vector3(17.5f, 0.08f, 1f), 2);
        surface.GetComponent<SpriteRenderer>().color = new Color(0.48f, 0.9f, 1f, 0.8f);

        CreateBoundary("Wall Left", new Vector2(-8.94f, -0.35f), new Vector2(0.4f, 8.2f));
        CreateBoundary("Wall Right", new Vector2(8.94f, -0.35f), new Vector2(0.4f, 8.2f));
        CreateBoundary("Sea Floor", new Vector2(0f, -5.1f), new Vector2(18f, 0.2f));

        CreateWorldText("Tutorial", "Steal bait for score. Press E to attack the fisherman.", new Vector3(0f, -4.05f, 0f), 1.05f, new Color(0.78f, 0.96f, 1f), 60, null);
    }

    private static void CreateEnvironmentAnimationProps(Dictionary<string, Sprite> sprites)
    {
        if (GameObject.Find("Water Wave") == null)
        {
            GameObject waveRoot = new GameObject("Water Wave");
            CreateSpriteObject("Water Wave Sprite", sprites["water_wave"], new Vector3(0f, 3.28f, 0f), new Vector3(1.7f, 1f, 1f), 4, waveRoot.transform);
        }

        if (GameObject.Find("Seaweed") == null)
        {
            GameObject seaweedRoot = new GameObject("Seaweed");
            CreateSpriteObject("Seaweed Left", sprites["seaweed"], new Vector3(-6.15f, -3.55f, 0f), new Vector3(0.76f, 0.82f, 1f), 5, seaweedRoot.transform);
            CreateSpriteObject("Seaweed Mid", sprites["seaweed"], new Vector3(-1.6f, -3.65f, 0f), new Vector3(0.62f, 0.72f, 1f), 5, seaweedRoot.transform);
            CreateSpriteObject("Seaweed Right", sprites["seaweed"], new Vector3(5.75f, -3.5f, 0f), new Vector3(0.82f, 0.9f, 1f), 5, seaweedRoot.transform);
        }
    }

    private static FishPlayerController CreatePlayer(Dictionary<string, Sprite> sprites)
    {
        GameObject playerObject = CreateSpriteObject("Player Fish", sprites["player_fish"], new Vector3(0f, -1.2f, 0f), Vector3.one, 30);
        Rigidbody2D rb = playerObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        CircleCollider2D collider = playerObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.43f;
        FishPlayerController player = playerObject.AddComponent<FishPlayerController>();
        return player;
    }

    private static FishermanController CreateFisherman(Dictionary<string, Sprite> sprites)
    {
        GameObject root = new GameObject("Fisherman Rig");
        root.transform.position = new Vector3(0f, 4.0f, 0f);

        GameObject boatObject = CreateSpriteObject("Boat", sprites["boat"], new Vector3(0f, 0f, 0f), Vector3.one, 10, root.transform);
        boatObject.transform.localPosition = new Vector3(0f, 0f, 0f);

        GameObject bodyObject = CreateSpriteObject("Fisherman", sprites["fisherman"], new Vector3(0.38f, 0.68f, 0f), Vector3.one, 12, root.transform);
        bodyObject.transform.localPosition = new Vector3(0.38f, 0.68f, 0f);

        Transform anchor = CreateMarker("Line Anchor", new Vector3(1.25f, 0.22f, 0f), root.transform);

        TextMeshPro exclamation = CreateWorldText("Notice", "!", new Vector3(0.5f, 1.55f, 0f), 3.8f, Color.red, 70, root.transform);
        exclamation.transform.localPosition = new Vector3(0.5f, 1.55f, 0f);

        TextMeshPro name = CreateWorldText("FishermanName", "Fisherman 1", new Vector3(0f, 1.55f, 0f), 0.85f, Color.white, 65, root.transform);
        name.transform.localPosition = new Vector3(-0.85f, 1.48f, 0f);
        name.alignment = TextAlignmentOptions.Center;

        FishermanController fisherman = root.AddComponent<FishermanController>();
        fisherman.lineAnchor = anchor;
        fisherman.boatRenderer = boatObject.GetComponent<SpriteRenderer>();
        fisherman.fishermanRenderer = bodyObject.GetComponent<SpriteRenderer>();
        fisherman.exclamationText = exclamation;
        fisherman.nameText = name;
        fisherman.SetVariant(FishFishermanType.Net, 1);
        return fisherman;
    }

    private static FishingLineView CreateFishingLine(FishGameManager gameManager, FishermanController fisherman, FishPlayerController player)
    {
        GameObject lineObject = new GameObject("Fishing Line");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            lineRenderer.sharedMaterial = new Material(shader);
        }

        FishingLineView line = lineObject.AddComponent<FishingLineView>();
        line.gameManager = gameManager;
        line.fisherman = fisherman;
        line.player = player;
        return line;
    }

    private static BaitSpawner CreateBaitSpawner(FishGameManager gameManager, FishPlayerController player, BaitPickup[] baitPrefabs)
    {
        GameObject spawnerObject = new GameObject("Bait Spawner");
        BaitSpawner spawner = spawnerObject.AddComponent<BaitSpawner>();
        spawner.gameManager = gameManager;
        spawner.player = player;
        spawner.baitPrefabs = baitPrefabs;
        spawner.spawnMin = new Vector2(-6.8f, -2.8f);
        spawner.spawnMax = new Vector2(6.9f, 2.45f);
        spawner.baseMaxBaits = 6;
        spawner.maxActiveBaits = 8;
        spawner.spawnInterval = 1.1f;
        spawner.minDistanceFromPlayer = 1.8f;
        spawner.minDistanceBetweenBaits = 1.55f;
        spawner.bigFishBlockCenter = new Vector2(-6.35f, -2.85f);
        spawner.bigFishBlockSize = new Vector2(5.25f, 2.8f);
        spawner.baseLevelBaitBudget = 16;
        spawner.baitBudgetPerLevel = 3;
        spawner.maxLevelBaitBudget = 24;
        spawner.emergencyBaitBudget = 4;
        return spawner;
    }

    private static NetSweepHazard CreateNetSweepHazard(Sprite netSprite, Transform parent)
    {
        GameObject netObject = new GameObject("Net Sweep Pivot");
        netObject.transform.SetParent(parent, false);
        NetSweepHazard netSweep = netObject.AddComponent<NetSweepHazard>();
        netSweep.pivotPosition = new Vector3(0f, 3.7f, 0f);
        netSweep.netSize = new Vector2(8f, 4.5f);
        netSweep.netLocalOffset = new Vector2(0f, -3.9f);
        netSweep.swingAngle = 62f;
        netSweep.warningSeconds = 0.7f;
        netSweep.sweepSeconds = 3.1f;
        netSweep.recoverSeconds = 0.25f;
        netSweep.netSprite = netSprite;

        GameObject netSpriteObject = CreateSpriteObject("Net Sprite", netSprite, netSweep.netLocalOffset, Vector3.one, netSweep.sortingOrder, netObject.transform, new Color(0.62f, 0.95f, 1f, 0.86f));
        BoxCollider2D collider = netSpriteObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = netSweep.netSize;
        collider.enabled = false;
        netSweep.netRenderer = netSpriteObject.GetComponent<SpriteRenderer>();

        netObject.SetActive(false);
        return netSweep;
    }

    private static ClawShotHazard CreateClawShotHazard(Sprite clawSprite, Transform parent)
    {
        GameObject clawObject = new GameObject("Claw Shot Hazard");
        clawObject.transform.SetParent(parent, false);
        ClawShotHazard clawShot = clawObject.AddComponent<ClawShotHazard>();
        clawShot.clawSprite = clawSprite;
        clawShot.pivotPosition = new Vector3(0f, 3.45f, 0f);
        clawShot.shotLength = 11f;
        clawShot.warningSeconds = 0.45f;
        clawShot.shotSeconds = 0.75f;
        clawShot.betweenShotsSeconds = 0.18f;

        LineRenderer path = CreateLineRenderer("Claw Path", clawObject.transform, clawShot.sortingOrder);
        path.positionCount = 0;

        GameObject clawHead = CreateSpriteObject("Claw Head", clawSprite, Vector3.zero, Vector3.one, clawShot.sortingOrder + 1, clawObject.transform);
        clawHead.GetComponent<SpriteRenderer>().enabled = false;

        clawObject.SetActive(false);
        return clawShot;
    }

    private static ElectricWaveHazard CreateElectricWaveHazard(Transform parent)
    {
        GameObject electricObject = new GameObject("Electric Wave Hazard");
        electricObject.transform.SetParent(parent, false);
        ElectricWaveHazard electricWave = electricObject.AddComponent<ElectricWaveHazard>();
        electricWave.xMin = -8f;
        electricWave.xMax = 8f;
        electricWave.topY = 2.8f;
        electricWave.bottomY = -3.2f;
        electricWave.waveSpacing = 1.35f;
        electricWave.warningSeconds = 0.35f;
        electricWave.activeSeconds = 0.35f;
        electricWave.afterWaveSeconds = 0.18f;

        int waveCount = Mathf.Max(1, Mathf.FloorToInt((electricWave.topY - electricWave.bottomY) / electricWave.waveSpacing) + 1);
        for (int i = 0; i < waveCount; i++)
        {
            CreateLineRenderer("Electric Wave " + (i + 1), electricObject.transform, electricWave.sortingOrder);
        }

        electricObject.SetActive(false);
        return electricWave;
    }

    private static BigFishAlly CreateBigFish(Dictionary<string, Sprite> sprites, FishGameManager gameManager)
    {
        GameObject bigFishObject = CreateSpriteObject("Big Fish Ally", sprites["big_fish"], new Vector3(-6.65f, -3.15f, 0f), Vector3.one, 22);
        CircleCollider2D collider = bigFishObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 1.05f;

        BigFishAlly ally = bigFishObject.AddComponent<BigFishAlly>();
        ally.gameManager = gameManager;

        TextMeshPro prompt = CreateWorldText("BigFishPrompt", "Need score\nfor big fish", new Vector3(0f, 1.25f, 0f), 0.85f, Color.white, 65, bigFishObject.transform);
        prompt.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        ally.promptText = prompt;
        return ally;
    }

    private static FishUIController CreateUi(Dictionary<string, Sprite> sprites)
    {
        // Runtime UI is generated here so Main.unity can be rebuilt without
        // hand-placing every TextMeshPro label again.
        TMP_FontAsset font = LoadFont();

        GameObject canvasObject = new GameObject("Fish UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        FishUIController ui = canvasObject.AddComponent<FishUIController>();
        Sprite uiSprite = sprites["ui_square"];
        ui.scorePanelSprite = sprites["ui_score_panel"];
        ui.multiplierPanelSprite = sprites["ui_multiplier_panel"];
        ui.alertPanelSprite = sprites["ui_alert_panel"];
        ui.controlsPanelSprite = sprites["ui_controls_panel"];
        ui.keycapSprite = sprites["ui_keycap"];
        ui.alertSegmentSprite = sprites["ui_alert_segment"];
        CreateWorldControlsHint(sprites["ui_controls_panel"], ui, font);
        RectTransform scoreRoot = CreateUiContainer(canvasObject.transform, "Score UI");
        RectTransform alertRoot = CreateUiContainer(canvasObject.transform, "Alert UI");

        ui.levelText = CreateUiText(canvasObject.transform, "LevelText", "Level 1", new Vector2(0f, 1f), new Vector2(24f, -22f), new Vector2(280f, 46f), 30f, Color.white, TextAlignmentOptions.Left, font);
        ui.totalScoreText = CreateUiText(scoreRoot, "TotalScoreText", "Total: 0", new Vector2(0f, 1f), new Vector2(24f, -70f), new Vector2(320f, 46f), 30f, Color.white, TextAlignmentOptions.Left, font);
        ui.currentRunText = CreateUiText(scoreRoot, "CurrentRunText", "At Risk: 0", new Vector2(0f, 1f), new Vector2(24f, -118f), new Vector2(320f, 46f), 30f, new Color(0.75f, 1f, 0.82f), TextAlignmentOptions.Left, font);
        ui.multiplierText = CreateUiText(scoreRoot, "MultiplierText", "Mult x1  Next x1", new Vector2(0f, 1f), new Vector2(24f, -166f), new Vector2(460f, 46f), 28f, new Color(1f, 0.9f, 0.62f), TextAlignmentOptions.Left, font);

        CreateBar(alertRoot, "AlertBar", new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(540f, 34f), uiSprite, new Color(0.18f, 0.08f, 0.08f, 0.86f), new Color(1f, 0.62f, 0.18f), out ui.alertFill);
        ui.alertText = CreateUiText(alertRoot, "AlertText", "Alert 0%", new Vector2(0.5f, 1f), new Vector2(0f, -78f), new Vector2(360f, 34f), 26f, Color.white, TextAlignmentOptions.Center, font);

        ui.attackCostText = CreateUiText(canvasObject.transform, "AttackCostText", "Press E Attack: 240", new Vector2(1f, 1f), new Vector2(-24f, -34f), new Vector2(430f, 46f), 28f, new Color(0.72f, 1f, 0.9f), TextAlignmentOptions.Right, font);
        ui.comboText = CreateUiText(canvasObject.transform, "ComboText", "", new Vector2(0.5f, 0f), new Vector2(0f, 112f), new Vector2(980f, 44f), 28f, new Color(1f, 0.92f, 0.56f), TextAlignmentOptions.Center, font);
        ui.statusText = CreateUiText(canvasObject.transform, "StatusText", "Steal bait for score. Press E to attack the fisherman.", new Vector2(0.5f, 0f), new Vector2(0f, 58f), new Vector2(1120f, 48f), 30f, Color.white, TextAlignmentOptions.Center, font);

        GameObject netPanel = new GameObject("NetSweepPanel", typeof(RectTransform));
        netPanel.transform.SetParent(canvasObject.transform, false);
        RectTransform netRect = netPanel.GetComponent<RectTransform>();
        netRect.anchorMin = new Vector2(0.5f, 0f);
        netRect.anchorMax = new Vector2(0.5f, 0f);
        netRect.pivot = new Vector2(0.5f, 0f);
        netRect.anchoredPosition = new Vector2(0f, 166f);
        netRect.sizeDelta = new Vector2(520f, 80f);

        CreateBar(netPanel.transform, "NetSweepBar", new Vector2(0.5f, 0.5f), new Vector2(0f, -14f), new Vector2(500f, 34f), uiSprite, new Color(0.1f, 0.08f, 0.12f, 0.86f), new Color(0.44f, 0.94f, 1f), out ui.netSweepFill);
        ui.netSweepText = CreateUiText(netPanel.transform, "NetSweepText", "Net Sweep 0%", new Vector2(0.5f, 0.5f), new Vector2(0f, 24f), new Vector2(500f, 34f), 28f, Color.white, TextAlignmentOptions.Center, font);
        ui.netSweepPanel = netPanel;
        netPanel.SetActive(false);

        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem));
        }

        return ui;
    }

    private static RectTransform CreateUiContainer(Transform parent, string name)
    {
        GameObject container = new GameObject(name, typeof(RectTransform));
        container.transform.SetParent(parent, false);

        RectTransform rect = container.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        return rect;
    }

    private static TextMeshProUGUI CreateUiText(Transform parent, string name, string value, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, float fontSize, Color color, TextAlignmentOptions alignment, TMP_FontAsset font)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(anchor.x, anchor.y);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    private static void CreateBar(Transform parent, string name, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, Sprite sprite, Color backgroundColor, Color fillColor, out Image fill)
    {
        GameObject background = new GameObject(name, typeof(RectTransform), typeof(Image));
        background.transform.SetParent(parent, false);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = anchor;
        backgroundRect.anchorMax = anchor;
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = anchoredPosition;
        backgroundRect.sizeDelta = size;

        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.sprite = sprite;
        backgroundImage.color = backgroundColor;
        backgroundImage.type = Image.Type.Simple;

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(background.transform, false);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(4f, 4f);
        fillRect.offsetMax = new Vector2(-4f, -4f);

        fill = fillObject.GetComponent<Image>();
        fill.sprite = sprite;
        fill.color = fillColor;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 0f;
    }

    private static void CreateWorldControlsHint(Sprite panelSprite, FishUIController ui, TMP_FontAsset font)
    {
        GameObject root = new GameObject("Controls Hint");
        root.transform.position = new Vector3(6.35f, -3.72f, 0f);

        GameObject panel = CreateSpriteObject("Panel", panelSprite, new Vector3(0.96f, -0.65f, 0f), new Vector3(1.25f, 1.15f, 1f), 6, root.transform, new Color(1f, 1f, 1f, 0.56f));

        TextMeshPro text = CreateWorldText("Text", "<color=#ff5a50>ESCAPE!</color>\nWASD / ARROWS   SWIM\nSHIFT   BURST\nE   ATTACK\nR   RESTART", new Vector3(0.98f, -0.65f, 0f), 1.8f, Color.white, 7, root.transform);
        text.font = font;
        text.richText = true;
        text.rectTransform.sizeDelta = new Vector2(3.2f, 1.2f);

        ui.controlsPanel = root;
        ui.controlsText = text;
        ui.controlsWorldPanelRenderer = panel.GetComponent<SpriteRenderer>();
    }

    private static GameObject CreateSpriteObject(string name, Sprite sprite, Vector3 position, Vector3 scale, int sortingOrder, Transform parent = null, Color? tint = null)
    {
        GameObject obj = new GameObject(name);
        if (parent != null)
        {
            obj.transform.SetParent(parent, false);
        }

        obj.transform.position = parent == null ? position : parent.TransformPoint(position);
        obj.transform.localScale = scale;

        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = tint ?? Color.white;
        renderer.sortingOrder = sortingOrder;
        AssignSpriteMaterial(renderer);
        return obj;
    }

    private static LineRenderer CreateLineRenderer(string name, Transform parent, int sortingOrder)
    {
        GameObject lineObject = new GameObject(name);
        lineObject.transform.SetParent(parent, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 0;
        line.numCapVertices = 3;
        line.sortingOrder = sortingOrder;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            line.sharedMaterial = new Material(shader);
        }

        return line;
    }

    private static Color GetBaitTint(FishBaitType type)
    {
        switch (type)
        {
            case FishBaitType.Shrimp:
                return new Color(1f, 0.48f, 0.4f);
            case FishBaitType.GlowBug:
                return new Color(0.5f, 1f, 0.45f);
            case FishBaitType.SmallFish:
                return new Color(0.42f, 0.8f, 1f);
            case FishBaitType.GoldenShrimp:
                return new Color(1f, 0.86f, 0.18f);
            case FishBaitType.FakeBait:
                return new Color(0.9f, 0.25f, 0.65f);
            default:
                return new Color(1f, 0.52f, 0.42f);
        }
    }

    private static TextMeshPro CreateWorldText(string name, string value, Vector3 position, float fontSize, Color color, int sortingOrder, Transform parent)
    {
        TMP_FontAsset font = LoadFont();
        GameObject obj = new GameObject(name);
        if (parent != null)
        {
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
        }
        else
        {
            obj.transform.position = position;
        }

        TextMeshPro text = obj.AddComponent<TextMeshPro>();
        text.font = font;
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.rectTransform.sizeDelta = new Vector2(7f, 1.5f);

        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        renderer.sortingOrder = sortingOrder;
        return text;
    }

    private static Transform CreateMarker(string name, Vector3 position, Transform parent = null)
    {
        GameObject marker = new GameObject(name);
        if (parent != null)
        {
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = position;
        }
        else
        {
            marker.transform.position = position;
        }

        return marker.transform;
    }

    private static void CreateBoundary(string name, Vector2 position, Vector2 size)
    {
        GameObject boundary = new GameObject(name);
        boundary.transform.position = position;
        BoxCollider2D collider = boundary.AddComponent<BoxCollider2D>();
        collider.size = size;
    }

    private static void SetSceneFirstInBuildSettings(string firstScenePath)
    {
        // Keep Main.unity as the only required startup scene while preserving
        // any extra scenes a teammate may intentionally add later.
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(firstScenePath, true)
        };

        foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
        {
            if (!string.Equals(existing.path, firstScenePath, StringComparison.OrdinalIgnoreCase))
            {
                scenes.Add(existing);
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static TMP_FontAsset LoadFont()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font == null)
        {
            font = TMP_Settings.defaultFontAsset;
        }

        return font;
    }

    private static void AssignSpriteMaterial(SpriteRenderer renderer)
    {
        Material material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        if (material != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static Sprite SaveUiPanelSprite(string name, int width, int height, Action<Color32[]> draw)
    {
        return SaveSprite(name, width, height, draw, UiPixelsPerUnit, new Vector4(8f, 8f, 8f, 8f));
    }

    private static Sprite SaveExternalBaitSprite(string name, int width, int height, float existingPixelsPerUnit, Action<Color32[]> draw)
    {
        return SaveSprite(name, width, height, draw, PixelsPerUnit, existingPixelsPerUnit, Vector4.zero);
    }

    private static Sprite SaveSprite(string name, int width, int height, Action<Color32[]> draw)
    {
        return SaveSprite(name, width, height, draw, PixelsPerUnit, Vector4.zero);
    }

    private static Sprite SaveSprite(string name, int width, int height, Action<Color32[]> draw, float pixelsPerUnit, Vector4 spriteBorder)
    {
        return SaveSprite(name, width, height, draw, pixelsPerUnit, pixelsPerUnit, spriteBorder);
    }

    private static Sprite SaveSprite(string name, int width, int height, Action<Color32[]> draw, float generatedPixelsPerUnit, float existingPixelsPerUnit, Vector4 spriteBorder)
    {
        string path = ArtPath + "/" + name + ".png";
        if (File.Exists(path))
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter existingImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (existingImporter != null)
            {
                existingImporter.textureType = TextureImporterType.Sprite;
                existingImporter.spriteImportMode = SpriteImportMode.Single;
                existingImporter.spritePixelsPerUnit = existingPixelsPerUnit;
                existingImporter.spriteBorder = spriteBorder;
                existingImporter.filterMode = FilterMode.Point;
                existingImporter.textureCompression = TextureImporterCompression.Uncompressed;
                existingImporter.mipmapEnabled = false;
                existingImporter.SaveAndReimport();
            }

            Sprite existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existingSprite != null)
            {
                return existingSprite;
            }
        }

        Color32[] pixels = new Color32[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Clear;
        }

        draw(pixels);

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels32(pixels);
        texture.Apply();

        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = generatedPixelsPerUnit;
        importer.spriteBorder = spriteBorder;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite LoadGeneratedSprite(string fileName)
    {
        string path = ArtPath + "/" + fileName;
        if (!File.Exists(path))
        {
            return null;
        }

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null)
        {
            return sprite;
        }

        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is Sprite nestedSprite)
            {
                return nestedSprite;
            }
        }

        return null;
    }

    private static void DrawHudPanel(Color32[] pixels, int width, int height, Color32 accent)
    {
        Rect(pixels, width, height, 5, 5, width - 5, height - 5, new Color32(4, 8, 13, 150));
        Rect(pixels, width, height, 0, 0, width - 7, height - 7, new Color32(13, 23, 34, 238));
        Rect(pixels, width, height, 7, 7, width - 21, height - 21, new Color32(22, 38, 54, 220));
        StrokeRect(pixels, width, height, 1, 1, width - 9, height - 9, new Color32(9, 15, 24, 255), 4);
        StrokeRect(pixels, width, height, 5, 5, width - 17, height - 17, new Color32(49, 75, 96, 255), 3);
        Line(pixels, width, height, 12, 9, width - 28, 9, accent);
    }

    private static void DrawAlertPanel(Color32[] pixels, int width, int height)
    {
        Rect(pixels, width, height, 5, 5, width - 5, height - 5, new Color32(4, 8, 13, 150));
        Rect(pixels, width, height, 0, 0, width - 7, height - 7, new Color32(13, 23, 34, 238));
        Rect(pixels, width, height, 7, 7, width - 21, height - 21, new Color32(22, 38, 54, 220));
        StrokeRect(pixels, width, height, 1, 1, width - 9, height - 9, new Color32(9, 15, 24, 255), 4);
        StrokeRect(pixels, width, height, 5, 5, width - 17, height - 17, new Color32(49, 75, 96, 255), 3);
    }

    private static void StrokeRect(Color32[] pixels, int width, int height, int x, int y, int rectWidth, int rectHeight, Color32 color, int thickness)
    {
        Rect(pixels, width, height, x, y, rectWidth, thickness, color);
        Rect(pixels, width, height, x, y + rectHeight - thickness, rectWidth, thickness, color);
        Rect(pixels, width, height, x, y, thickness, rectHeight, color);
        Rect(pixels, width, height, x + rectWidth - thickness, y, thickness, rectHeight, color);
    }

    private static void Rect(Color32[] pixels, int width, int height, int x, int y, int rectWidth, int rectHeight, Color32 color)
    {
        int minX = Mathf.Clamp(x, 0, width);
        int minY = Mathf.Clamp(y, 0, height);
        int maxX = Mathf.Clamp(x + rectWidth, 0, width);
        int maxY = Mathf.Clamp(y + rectHeight, 0, height);

        for (int py = minY; py < maxY; py++)
        {
            for (int px = minX; px < maxX; px++)
            {
                pixels[py * width + px] = color;
            }
        }
    }

    private static void Ellipse(Color32[] pixels, int width, int height, int centerX, int centerY, int radiusX, int radiusY, Color32 color)
    {
        for (int py = centerY - radiusY; py <= centerY + radiusY; py++)
        {
            for (int px = centerX - radiusX; px <= centerX + radiusX; px++)
            {
                if (px < 0 || px >= width || py < 0 || py >= height)
                {
                    continue;
                }

                float nx = (px - centerX) / (float)radiusX;
                float ny = (py - centerY) / (float)radiusY;
                if (nx * nx + ny * ny <= 1f)
                {
                    pixels[py * width + px] = color;
                }
            }
        }
    }

    private static void Triangle(Color32[] pixels, int width, int height, Vector2Int a, Vector2Int b, Vector2Int c, Color32 color)
    {
        int minX = Mathf.Clamp(Mathf.Min(a.x, Mathf.Min(b.x, c.x)), 0, width - 1);
        int maxX = Mathf.Clamp(Mathf.Max(a.x, Mathf.Max(b.x, c.x)), 0, width - 1);
        int minY = Mathf.Clamp(Mathf.Min(a.y, Mathf.Min(b.y, c.y)), 0, height - 1);
        int maxY = Mathf.Clamp(Mathf.Max(a.y, Mathf.Max(b.y, c.y)), 0, height - 1);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2 p = new Vector2(x, y);
                if (PointInTriangle(p, a, b, c))
                {
                    pixels[y * width + x] = color;
                }
            }
        }
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = Sign(p, a, b);
        float area2 = Sign(p, b, c);
        float area3 = Sign(p, c, a);
        bool hasNegative = area < 0f || area2 < 0f || area3 < 0f;
        bool hasPositive = area > 0f || area2 > 0f || area3 > 0f;
        return !(hasNegative && hasPositive);
    }

    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    private static void Line(Color32[] pixels, int width, int height, int x0, int y0, int x1, int y1, Color32 color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
            {
                pixels[y0 * width + x0] = color;
            }

            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}
