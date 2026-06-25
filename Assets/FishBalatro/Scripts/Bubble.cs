using UnityEngine;

public class Bubble : MonoBehaviour
{
    public float riseSpeed = 0.55f;
    public float swayAmount = 0.12f;
    public float swaySpeed = 1.2f;
    public float surfaceY = 3.55f;
    public float lifeTime = 6.5f;

    private Animator animator;
    private Vector3 startPos;
    private float age;
    private float phase;
    private bool isBursting = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        startPos = transform.position;
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        if (isBursting) return;

        age += Time.deltaTime;
        float yPosition = startPos.y + age * riseSpeed;
        float xOffset = Mathf.Sin(age * swaySpeed + phase) * swayAmount;

        transform.position = new Vector3(
            startPos.x + xOffset,
            yPosition,
            transform.position.z
        );

        if (yPosition >= surfaceY || age >= lifeTime)
        {
            Burst();
        }
    }

    void Burst()
    {
        if (isBursting)
        {
            return;
        }

        isBursting = true;
        if (animator != null)
        {
            animator.Play("Bubble_Burst");
        }
        else
        {
            DestroyBubble();
        }
    }

    public void DestroyBubble()
    {
        Destroy(gameObject);
    }
}
