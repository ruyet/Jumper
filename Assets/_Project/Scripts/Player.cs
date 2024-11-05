using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float MovementSpeed = 5f;   // Horizontal movement speed
    public float JumpForce = 7f;       // Jump force for the player

    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private bool IsFacingRight = true;

    // Ground check variables
    [SerializeField] private Transform groundCheck;    // Reference to an empty GameObject at player's feet
    [SerializeField] private float groundCheckRadius = 0.1f; // Radius for ground detection
    [SerializeField] private LayerMask groundLayer;    // Layer to represent the ground

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 2.0f;  // Adjust gravity to make jumping feel more grounded
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Get horizontal input
        float inputX = Input.GetAxisRaw("Horizontal");

        // Move the player by setting the Rigidbody's velocity
        _rigidbody.velocity = new Vector2(inputX * MovementSpeed, _rigidbody.velocity.y);

        // Jump if the player is grounded and the jump button is pressed
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            _rigidbody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
        }

        // Check and flip the direction of the player based on input
        if (inputX > 0 && !IsFacingRight)
        {
            Flip();
        }
        else if (inputX < 0 && IsFacingRight)
        {
            Flip();
        }

        // Set animation parameters
        _animator.SetBool("IsInAir", !IsGrounded());   // Set to true if not grounded
        _animator.SetBool("IsWalking", inputX != 0);    // Set to true if there's horizontal input
    }

    private bool IsGrounded()
    {
        // Check if the player is on the ground using OverlapCircle
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void Flip()
    {
        // Flip the direction of the player
        IsFacingRight = !IsFacingRight;

        // Scale the player in the x-axis to turn it around
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
