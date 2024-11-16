// Necessary Unity packages
using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    // Public variables for movement settings
    public float MovementSpeed = 5f;   // Horizontal movement speed of the player
    public float JumpForce = 7f;       // Jump force for the player
    public float JumpForceFromClimb = 5f; // Reduced jump force for jumping off a ladder or rope
    public float MaxFallSpeed = -10f;  // Maximum fall speed to limit the player's falling speed

    // Private variables for references and state
    private Rigidbody2D _rigidbody;    // Reference to the player's Rigidbody2D component
    private Animator _animator;        // Reference to the player's Animator component
    private bool IsFacingRight = true; // Tracks which direction the player is facing
    AudioSource jumpSound;             // Reference to the AudioSource component for jump sound

    // Video player component
    public VideoPlayer videoPlayer;    // Reference to the VideoPlayer component to play videos

    // Ground check variables
    [SerializeField] private Transform groundCheck;    // Reference to an empty GameObject at the player's feet to check if grounded
    [SerializeField] private float groundCheckRadius = 0.1f; // Radius for detecting if the player is on the ground
    [SerializeField] private LayerMask groundLayer;    // Layer representing the ground for ground detection

    // Knockback variables
    private bool isKnockbacked = false; // Whether the player is in a knockback state
    [SerializeField] private float knockbackForceX = 10f; // Horizontal knockback force
    [SerializeField] private float knockbackForceY = 2f; // Vertical knockback force
    [SerializeField] private float knockbackDuration = 0.5f; // Duration of the knockback effect
    private float knockbackTimer = 0f; // Timer to keep track of knockback duration

    // Climbing variables
    private bool isClimbing = false;          // Whether the player is currently climbing
    private bool isNearLadderOrRope = false;  // Whether the player is near a climbable object
    private Transform currentClimbObject;     // Reference to the current climbable object

    private bool isCrouching = false; // Whether the player is crouching

    private SpriteRenderer _spriteRenderer; // Reference to the player's SpriteRenderer component

    private void Start()
    {
        // Get references to necessary components
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 2.0f;  // Set the gravity scale for a more grounded feel
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        jumpSound = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Update knockback timer and reset state if knockback duration is over
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
        }
        else
        {
            isKnockbacked = false;
        }

        // Get horizontal input for player movement
        float inputX = Input.GetAxisRaw("Horizontal");

        // Check if player is crouching
        isCrouching = Input.GetKey(KeyCode.DownArrow) && inputX == 0;

        // Handle movement if not climbing and not in knockback state
        if (!isClimbing && !isKnockbacked)
        {
            if (!isCrouching)
            {
                // Move the player horizontally based on input
                _rigidbody.velocity = new Vector2(inputX * MovementSpeed, _rigidbody.velocity.y);
            }
            else
            {
                // If crouching, stop horizontal movement
                _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
            }

            // Handle jump input if grounded and not crouching
            if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)) && IsGrounded() && !isCrouching)
            {
                _rigidbody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse); // Apply vertical force to jump
                jumpSound.Play(); // Play jump sound
            }

            // Handle character flipping based on direction
            if (inputX > 0 && !IsFacingRight)
            {
                Flip();
            }
            else if (inputX < 0 && IsFacingRight)
            {
                Flip();
            }
        }

        // Set animation parameters based on player's state
        _animator.SetBool("IsInAir", !IsGrounded() && !isClimbing);
        _animator.SetBool("IsWalking", inputX != 0 && !isClimbing && !isKnockbacked);
        _animator.SetBool("IsCrouching", isCrouching);
        _animator.SetBool("IsClimbing", isClimbing);
        _animator.SetBool("IsKnockback", isKnockbacked);
    }

    // Method to check if player is grounded
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // Method to flip the player's direction
    // This is apparently one of the best ways to do this (this is important for the camera movement lateron)
    private void Flip()
    {
        IsFacingRight = !IsFacingRight; // Toggle the direction the player is facing
        Vector3 scale = transform.localScale;
        scale.x *= -1; // Invert the scale to flip the player
        transform.localScale = scale;
    }

    // Trigger detection for climbing objects or win condition
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder") || collision.CompareTag("Rope"))
        {
            isNearLadderOrRope = true; // Player is near a ladder or rope
            currentClimbObject = collision.transform; // Store reference to the climbable object
        }

        if (collision.CompareTag("Win"))
        {
            PlayWinVideo(); // Play the win video if player reaches the win box
        }
    }

    // Method to play the win video
    private void PlayWinVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play(); // Play the video using VideoPlayer component
        }
        else
        {
            //Debug.LogWarning("VideoPlayer not assigned!"); // Log warning if VideoPlayer is not assigned
        }
    }

    // Trigger exit detection for climbing objects
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder") || collision.CompareTag("Rope"))
        {
            isNearLadderOrRope = false; // Player is no longer near a climbable object
            isClimbing = false; // Stop climbing
            _rigidbody.gravityScale = 2.0f; // Restore gravity
            currentClimbObject = null; // Clear reference to climbable object
        }
    }

    // Coroutine to handle the knockback effect
    private IEnumerator HandleKnockback()
    {
        // Exit climbing mode when knockback starts
        if (isClimbing)
        {
            isClimbing = false;
            _rigidbody.gravityScale = 2.0f; // Restore gravity
        }

        isKnockbacked = true; // Set knockback state
        knockbackTimer = knockbackDuration; // Set knockback timer

        // Apply knockback force in the opposite direction of the player's facing direction
        float knockbackDirection = IsFacingRight ? -1f : 1f;
        Vector2 knockbackForce = new Vector2(knockbackDirection * knockbackForceX, knockbackForceY);
        _rigidbody.velocity = Vector2.zero; // Reset velocity before applying knockback
        _rigidbody.AddForce(knockbackForce, ForceMode2D.Impulse); // Apply knockback force

        // Flicker the player's sprite to indicate knockback effect
        for (float t = 0; t < 2f; t += 0.1f)
        {
            _spriteRenderer.enabled = !_spriteRenderer.enabled; // Toggle sprite visibility
            yield return new WaitForSeconds(0.1f); // Wait briefly before toggling again
        }
        _spriteRenderer.enabled = true; // Ensure sprite is visible after flickering
        isKnockbacked = false; // Reset knockback state after flickering
    }

    private void FixedUpdate()
    {
        // Handle climbing if player is near a ladder or rope and presses the up key
        if (isNearLadderOrRope && Input.GetKey(KeyCode.UpArrow))
        {
            float distanceToCenter = Mathf.Abs(transform.position.x - currentClimbObject.position.x);
            if (distanceToCenter < 0.1f)
            {
                isClimbing = true; // Start climbing
                _rigidbody.gravityScale = 0; // Disable gravity while climbing
                transform.position = new Vector3(currentClimbObject.position.x, transform.position.y, transform.position.z); // Center player on ladder or rope
            }
        }

        // Handle climbing movement
        if (isClimbing)
        {
            float inputY = Input.GetAxisRaw("Vertical");
            _rigidbody.velocity = new Vector2(0, inputY * MovementSpeed); // Move player up or down the ladder or rope

            // Allow player to jump off the ladder or rope
            if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                isClimbing = false; // Stop climbing
                _rigidbody.gravityScale = 2.0f; // Restore gravity
                float jumpDirection = Input.GetKey(KeyCode.LeftArrow) ? -1f : 1f; // Determine jump direction
                _rigidbody.AddForce(new Vector2(jumpDirection * JumpForceFromClimb, JumpForceFromClimb), ForceMode2D.Impulse); // Apply jump force
            }
        }

        // Exit climbing state if grounded and pressing down key
        if (isClimbing && IsGrounded() && Input.GetKey(KeyCode.DownArrow))
        {
            isClimbing = false;
            _rigidbody.gravityScale = 2.0f; // Restore gravity
        }

        // Limit the player's fall speed when not grounded and not climbing
        if (!IsGrounded() && !isClimbing && _rigidbody.velocity.y < 0)
        {
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, Mathf.Max(_rigidbody.velocity.y, MaxFallSpeed)); // Cap the fall speed
        }
    }
}
