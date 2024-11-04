using UnityEngine;

public class SimplePlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;      // Speed for left and right movement
    public float jumpForce = 10f;     // Force applied when jumping

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Move();
        if (Input.GetButtonDown("Jump") && Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            Jump();
        }
    }

    void Move()
    {
        float moveInput = Input.GetAxis("Horizontal"); // Left (-1) and Right (1) movement
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y); // Apply horizontal movement
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Apply jump force
    }
}
