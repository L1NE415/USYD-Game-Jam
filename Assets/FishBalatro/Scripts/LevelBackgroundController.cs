using UnityEngine;

public class LevelBackgroundController : MonoBehaviour
{
    public SpriteRenderer backgroundRenderer;
    public Sprite[] levelBackgrounds;
    public float extraWorldPadding = 0f;

    private void Awake()
    {
        EnsureRenderer();
    }

    private void Start()
    {
        FishGameManager game = FishGameManager.Instance;
        ApplyLevel(game != null ? game.Level : 1);
    }

    public void ApplyLevel(int level)
    {
        EnsureRenderer();
        if (backgroundRenderer == null || levelBackgrounds == null || levelBackgrounds.Length == 0)
        {
            return;
        }

        int index = Mathf.Clamp(level - 1, 0, levelBackgrounds.Length - 1);
        Sprite background = levelBackgrounds[index];
        if (background == null)
        {
            return;
        }

        backgroundRenderer.sprite = background;
        backgroundRenderer.color = Color.white;
        backgroundRenderer.drawMode = SpriteDrawMode.Simple;
        FitToCamera(background);
    }

    private void EnsureRenderer()
    {
        if (backgroundRenderer == null)
        {
            backgroundRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void FitToCamera(Sprite background)
    {
        Camera camera = Camera.main;
        if (camera == null || !camera.orthographic || background == null)
        {
            return;
        }

        float worldHeight = camera.orthographicSize * 2f + extraWorldPadding;
        float worldWidth = camera.orthographicSize * 2f * camera.aspect + extraWorldPadding;
        Vector2 spriteSize = background.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        transform.localScale = new Vector3(worldWidth / spriteSize.x, worldHeight / spriteSize.y, 1f);
        transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, transform.position.z);
    }
}
