using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D player_rb;
    public float move_speed;
    public float horizontal_readout;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player_rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        player_rb.linearVelocity = new Vector2(Input.GetAxis("Horizontal") * move_speed, Input.GetAxis("Vertical") * move_speed);
        if (Input.GetAxis("Horizontal") < 0)
        {
            gameObject.GetComponent<SpriteRenderer>().flipY = true;
        } else if (Input.GetAxis("Horizontal") > 0)
        {
            gameObject.GetComponent<SpriteRenderer>().flipY = false;
        }
    }
}
