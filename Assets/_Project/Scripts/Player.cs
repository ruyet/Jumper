using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float MovementSpeed = 5f;   // Horizontal movement speed
    public float JumpForce = 7f;       // Jump force for the player
    public float JumpForceFromClimb = 5f; // Reduced jump force for jumping off a ladder or rope
    public float MaxFallSpeed = -10f;  // Maximum fall speed to keep the fall feeling natural
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private bool IsFacingRight = true;

    // Ground check variables
    [SerializeField] private Transform groundCheck;    // Reference to an empty GameObject at player's feet
    [SerializeField] private float groundCheckRadius = 0.1f; // Radius for ground detection
    [SerializeField] private LayerMask groundLayer;    // Layer to represent the ground

    // Knockback variables
    private bool isKnockbacked = false;
    [SerializeField] private float knockbackForceX = 10f; // Increased horizontal knockback force
    [SerializeField] private float knockbackForceY = 2f; // Reduced vertical knockback force
    [SerializeField] private float knockbackDuration = 0.5f; // Duration of the knockback effect
    private float knockbackTimer = 0f;

    // Climbing variables
    private bool isClimbing = false;
    private bool isNearLadderOrRope = false;
    private Transform currentClimbObject;

    private bool isCrouching = false; // Define isCrouching as a class-level variable

    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 2.0f;  // Adjust gravity to make jumping feel more grounded
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Update knockback timer
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
        }
        else
        {
            isKnockbacked = false;
        }

        // Get horizontal input
        float inputX = Input.GetAxisRaw("Horizontal");

        // Check if player is crouching
        isCrouching = Input.GetKey(KeyCode.DownArrow) && inputX == 0;

        // Handle movement if not climbing and not in knockback state
        if (!isClimbing && !isKnockbacked)
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
        _animator.SetBool("IsWalking", inputX != 0 && !isClimbing && !isKnockbacked);    // Set to true if there's horizontal input
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
        if (collision.CompareTag("Ladder") || collision.CompareTag("Rope"))
        {
            //Debug.Log("Player collided with Ladder/Rope"); // Debug Log
            isNearLadderOrRope = true;
            currentClimbObject = collision.transform;  // Store reference to the ladder or rope
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder") || collision.CompareTag("Rope"))
        {
            // Debug.Log("Player exited Ladder/Rope area"); // Debug Log
            isNearLadderOrRope = false;
            isClimbing = false;
            _rigidbody.gravityScale = 2.0f; // Restore gravity
            currentClimbObject = null;  // Clear reference to the ladder or rope
        }
    }

    private IEnumerator HandleKnockback()
    {
        isKnockbacked = true;
        knockbackTimer = knockbackDuration;

        // Apply impulse force to simulate knockback backward with both horizontal and vertical components
        float knockbackDirection = IsFacingRight ? -1f : 1f; // Knockback pushes opposite to the facing direction
        Vector2 knockbackForce = new Vector2(knockbackDirection * knockbackForceX, knockbackForceY);
        _rigidbody.velocity = Vector2.zero; // Reset velocity before applying knockback
        _rigidbody.AddForce(knockbackForce, ForceMode2D.Impulse);

        // Flicker the player's sprite for 2 seconds
        for (float t = 0; t < 2f; t += 0.1f)
        {
            _spriteRenderer.enabled = !_spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        _spriteRenderer.enabled = true;
    }

    private void FixedUpdate()
    {
        if (isNearLadderOrRope && Input.GetKey(KeyCode.UpArrow))
        {
            float distanceToCenter = Mathf.Abs(transform.position.x - currentClimbObject.position.x);
            if (distanceToCenter < 0.1f) // Allow climbing only if close enough to the center
            {
                isClimbing = true;
                _rigidbody.gravityScale = 0;  // Disable gravity while climbing
                // Snap the player to the center of the ladder or rope when starting to climb
                transform.position = new Vector3(currentClimbObject.position.x, transform.position.y, transform.position.z);
            }
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

        // Handle falling to ensure a controlled fall speed while maintaining natural gravity
        if (!IsGrounded() && !isClimbing && _rigidbody.velocity.y < 0)
        {
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, Mathf.Max(_rigidbody.velocity.y, MaxFallSpeed));
        }
    }
}
