using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Helps teammates land in the actual Fish Balatro scene after cloning/opening
// the project. Unity often opens a fresh project on an empty "Untitled" scene,
// even when Build Settings already point at Main.unity.
[InitializeOnLoad]
public static class FishBalatroStartupSceneLoader
{
    private const string MainScenePath = "Assets/Scenes/Main.unity";

    static FishBalatroStartupSceneLoader()
    {
        EditorApplication.delayCall += OpenMainSceneIfEditorStartedBlank;
    }

    [MenuItem("Game Jam/Fish Balatro/Open Main Scene")]
    public static void OpenMainScene()
    {
        if (File.Exists(MainScenePath))
        {
            EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        }
    }

    [MenuItem("Game Jam/Fish Balatro/Repair Missing Sprite Artwork")]
    public static void ApplySpriteArtwork()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.path.Replace("\\", "/") != MainScenePath)
        {
            return;
        }

        bool changed = false;
        changed |= ApplySprite("Water Background", "water_panel", Color.white);
        changed |= ApplySprite("Boat", "boat", Color.white);
        changed |= ApplySprite("Fisherman", "fisherman", Color.white);
        changed |= ApplySprite("Player Fish", "player_fish", Color.white);
        changed |= ApplySprite("Net Sprite", "net", new Color(0.62f, 0.95f, 1f, 0.86f));
        changed |= ApplySprite("Big Fish Ally", "Fish_Large_1.png", Color.white);
        changed |= ApplySprite("Big Fish Ally", "big_fish", Color.white);

        if (changed)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log("Fish Balatro missing sprite artwork repaired in Main.unity.");
        }
    }

    private static void OpenMainSceneIfEditorStartedBlank()
    {
        EditorApplication.delayCall -= OpenMainSceneIfEditorStartedBlank;

        if (EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(MainScenePath))
        {
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(activeScene.path) || !LooksLikeDefaultBlankScene(activeScene))
        {
            return;
        }

        OpenMainScene();
    }

    private static bool LooksLikeDefaultBlankScene(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        if (roots.Length == 0)
        {
            return true;
        }

        return roots.Length == 1 && roots[0].name == "Main Camera";
    }

    private static bool ApplySprite(string objectName, string spriteName, Color color)
    {
        GameObject sceneObject = FindInActiveScene(objectName);
        if (sceneObject == null)
        {
            return false;
        }

        SpriteRenderer renderer = sceneObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            return false;
        }

        if (renderer.sprite != null)
        {
            return false;
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/FishBalatro/Art/Generated/" + spriteName + ".png");
        if (sprite == null)
        {
            return false;
        }

        renderer.sprite = sprite;
        bool changed = true;
        if (renderer.color != color)
        {
            renderer.color = color;
            changed = true;
        }

        return changed;
    }

    private static GameObject FindInActiveScene(string objectName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        foreach (GameObject root in activeScene.GetRootGameObjects())
        {
            GameObject found = FindChildRecursive(root.transform, objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static GameObject FindChildRecursive(Transform current, string objectName)
    {
        if (current.name == objectName)
        {
            return current.gameObject;
        }

        for (int i = 0; i < current.childCount; i++)
        {
            GameObject found = FindChildRecursive(current.GetChild(i), objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
