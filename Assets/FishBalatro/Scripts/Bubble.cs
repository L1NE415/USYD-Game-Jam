using UnityEngine;

public class Bubble : MonoBehaviour
{
    public float riseSpeed = 1.5f;
    public float swayAmount = 0.2f;
    public float swaySpeed = 2f;
    public float lifeTime = 2.5f;

    private Animator animator;
    private Vector3 startPos;
    private bool isBursting = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        startPos = transform.position;

        Invoke(nameof(Burst), lifeTime);
    }

    void Update()
    {
        if (isBursting) return;

        float yMove = riseSpeed * Time.deltaTime;
        float xOffset = Mathf.Sin(Time.time * swaySpeed) * swayAmount;

        transform.position += new Vector3(0, yMove, 0);
        transform.position = new Vector3(
            startPos.x + xOffset,
            transform.position.y,
            transform.position.z
        );
    }

    void Burst()
    {
        isBursting = true;
        animator.Play("Bubble_Burst");
    }

    public void DestroyBubble()
    {
        Destroy(gameObject);
    }
}