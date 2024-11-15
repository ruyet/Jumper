using UnityEngine;
using UnityEngine.Video;
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
    AudioSource jumpSound;

    public VideoPlayer videoPlayer; // Reference to the VideoPlayer component

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

    private bool isCrouching = false;

    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 2.0f;  // Adjust gravity to make jumping feel more grounded
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        jumpSound = GetComponent<AudioSource>();
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
            if (!isCrouching)
            {
                _rigidbody.velocity = new Vector2(inputX * MovementSpeed, _rigidbody.velocity.y);
            }
            else
            {
                _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
            }

            if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)) && IsGrounded() && !isCrouching)
            {
                _rigidbody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
                jumpSound.Play();
            }

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
        _animator.SetBool("IsInAir", !IsGrounded() && !isClimbing);
        _animator.SetBool("IsWalking", inputX != 0 && !isClimbing && !isKnockbacked);
        _animator.SetBool("IsCrouching", isCrouching);
        _animator.SetBool("IsClimbing", isClimbing);
        _animator.SetBool("IsKnockback", isKnockbacked);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void Flip()
    {
        IsFacingRight = !IsFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder") || collision.CompareTag("Rope"))
        {
            isNearLadderOrRope = true;
            currentClimbObject = collision.transform;
        }

        if (collision.CompareTag("Win"))
        {
            PlayWinVideo();
        }
    }

    private void PlayWinVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play(); // Play the video
        }
        else
        {
            Debug.LogWarning("VideoPlayer not assigned!");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder") || collision.CompareTag("Rope"))
        {
            isNearLadderOrRope = false;
            isClimbing = false;
            _rigidbody.gravityScale = 2.0f;
            currentClimbObject = null;
        }
    }

    private IEnumerator HandleKnockback()
    {
        isKnockbacked = true;
        knockbackTimer = knockbackDuration;

        float knockbackDirection = IsFacingRight ? -1f : 1f;
        Vector2 knockbackForce = new Vector2(knockbackDirection * knockbackForceX, knockbackForceY);
        _rigidbody.velocity = Vector2.zero;
        _rigidbody.AddForce(knockbackForce, ForceMode2D.Impulse);

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
            if (distanceToCenter < 0.1f)
            {
                isClimbing = true;
                _rigidbody.gravityScale = 0;
                transform.position = new Vector3(currentClimbObject.position.x, transform.position.y, transform.position.z);
            }
        }

        if (isClimbing)
        {
            float inputY = Input.GetAxisRaw("Vertical");
            _rigidbody.velocity = new Vector2(0, inputY * MovementSpeed);

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
            isClimbing = false;
            _rigidbody.gravityScale = 2.0f;
        }

        if (!IsGrounded() && !isClimbing && _rigidbody.velocity.y < 0)
        {
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, Mathf.Max(_rigidbody.velocity.y, MaxFallSpeed));
        }
    }
}
