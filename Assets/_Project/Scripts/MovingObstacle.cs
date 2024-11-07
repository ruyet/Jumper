using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    public float moveDistance = 3f; // Distance the obstacle moves in each direction
    public float speed = 2f; // Speed of the obstacle's movement

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Move the obstacle left and right
        float offset = Mathf.PingPong(Time.time * speed, moveDistance);
        transform.position = startPosition + new Vector3(offset, 0, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object is the player
        if (other.CompareTag("Player"))
        {
            // Trigger knockback by invoking the HandleKnockback coroutine in the PlayerMovement script
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.StartCoroutine("HandleKnockback");
            }
        }
    }
}
