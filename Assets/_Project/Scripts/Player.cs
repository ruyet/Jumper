using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float MovementSpeed = 5f;   // Horizontal movement speed
    public float JumpForce = 7f;       // Jump force for the player
    public float JumpForceFromClimb = 5f; // Reduced jump force for jumping off a ladder or rope
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private bool IsFacingRight = true;

    // Ground check variables
    [SerializeField] private Transform groundCheck;    // Reference to an empty GameObject at player's feet
    [SerializeField] private float groundCheckRadius = 0.1f; // Radius for ground detection
    [SerializeField] private LayerMask groundLayer;    // Layer to represent the ground

    // Knockback variables
    private bool isKnockbacked = false;

    // Climbing variables
    private bool isClimbing = false;
    private bool isNearLadderOrRope = false;
    private Transform currentClimbObject;

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

        // Check if player is crouching
        bool isCrouching = Input.GetKey(KeyCode.DownArrow) && inputX == 0;



        // Handle movement if not climbing
        if (!isClimbing)
        {
            // Move the player by setting the Rigidbody's velocity if not crouching
            if (!isCrouching)
            {
                _rigidbody.velocity = new Vector2(inputX * MovementSpeed, _rigidbody.velocity.y);
            }
            else
            {
                // Stop horizontal movement while crouching
                _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
            }

            // Jump if the player is grounded and the jump button (space or alt) is pressed
            if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)) && IsGrounded() && !isCrouching)
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
        }

        // Set animation parameters
        _animator.SetBool("IsInAir", !IsGrounded() && !isClimbing);   // Set to true if not grounded
        _animator.SetBool("IsWalking", inputX != 0 && !isClimbing);    // Set to true if there's horizontal input
        _animator.SetBool("IsCrouching", isCrouching);  // Set crouch state
        _animator.SetBool("IsClimbing", isClimbing);  // Set climbing state
        _animator.SetBool("IsKnockback", isKnockbacked);  // Set knockback state
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            StartCoroutine(HandleKnockback());
        }
        else if (collision.CompareTag("Ladder") || collision.CompareTag("Rope"))
        {
            isNearLadderOrRope = true;
            currentClimbObject = collision.transform;  // Store reference to the ladder or rope
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder") || collision.CompareTag("Rope"))
        {
            isNearLadderOrRope = false;
            isClimbing = false;
            _rigidbody.gravityScale = 2.0f; // Restore gravity
            currentClimbObject = null;  // Clear reference to the ladder or rope
        }
    }

    private IEnumerator HandleKnockback()
    {
        isKnockbacked = true;

        // Apply a small force to simulate knockback jump backward
        _rigidbody.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 5f, JumpForce * 0.7f);

        // Flicker the player's sprite for 2 seconds
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        for (float t = 0; t < 2f; t += 0.1f)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        spriteRenderer.enabled = true;

        // Allow player to move while in knockback state
        yield return new WaitForSeconds(3f);

        isKnockbacked = false;
    }

    private void FixedUpdate()
    {
        if (isNearLadderOrRope && Input.GetKey(KeyCode.UpArrow))
        {
            isClimbing = true;
            _rigidbody.gravityScale = 0;  // Disable gravity while climbing
            // Snap the player to the center of the ladder or rope when starting to climb
            transform.position = new Vector3(currentClimbObject.position.x, transform.position.y, transform.position.z);
        }

        if (isClimbing)
        {
            // Get vertical input for climbing
            float inputY = Input.GetAxisRaw("Vertical");
            _rigidbody.velocity = new Vector2(0, inputY * MovementSpeed);

            // Allow player to jump off the rope or ladder by holding left or right while pressing alt
            if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                isClimbing = false;
                _rigidbody.gravityScale = 2.0f;
                float jumpDirection = Input.GetKey(KeyCode.LeftArrow) ? -1f : 1f;
                _rigidbody.AddForce(new Vector2(jumpDirection * JumpForceFromClimb, JumpForceFromClimb), ForceMode2D.Impulse);
            }
        }

        if (isClimbing && IsGrounded() && Input.GetKey(KeyCode.DownArrow))
        {
            // Detach from ladder or rope when reaching the ground
            isClimbing = false;
            _rigidbody.gravityScale = 2.0f;
        }
    }
}