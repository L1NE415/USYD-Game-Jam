using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
// The big fish is the player's paid level-transition action. It shows whether
// the player can afford an attack and plays the short attack movement.
public class BigFishAlly : MonoBehaviour
{
    public FishGameManager gameManager;
    public TMP_Text promptText;
    public float attackDuration = 0.55f;
    public float returnDuration = 0.45f;

    private Vector3 homePosition;

    private void Awake()
    {
        homePosition = transform.position;
        SetPrompt(false);
    }

    private void Update()
    {
        if (gameManager == null)
        {
            gameManager = FishGameManager.Instance;
        }

        bool canAttack = gameManager != null && gameManager.CanCallBigFish;
        SetPrompt(gameManager != null);

        if (promptText != null && gameManager != null)
        {
            promptText.text = canAttack
                ? "E: ATTACK\nCost " + gameManager.AttackCost
                : "Need " + gameManager.AttackCost + " score\nPress E when ready";
            promptText.color = canAttack ? new Color(0.8f, 1f, 0.9f) : new Color(1f, 0.72f, 0.72f);
        }
    }

    public IEnumerator PlayAttack(Vector3 targetPosition)
    {
        // Rush toward the fisherman, pause for impact, then return home.
        Vector3 start = homePosition;
        Vector3 end = new Vector3(targetPosition.x - 0.8f, targetPosition.y - 0.3f, transform.position.z);
        yield return MoveTo(start, end, attackDuration);
        yield return new WaitForSeconds(0.12f);
        yield return MoveTo(transform.position, homePosition, returnDuration);
    }

    private IEnumerator MoveTo(Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            transform.position = Vector3.LerpUnclamped(start, end, t);
            yield return null;
        }

        transform.position = end;
    }

    private void SetPrompt(bool visible)
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(visible);
        }
    }
}
