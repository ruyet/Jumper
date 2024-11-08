using UnityEngine;

public class CornerCollisionHandler : MonoBehaviour
{
    private Rigidbody2D rb;
    public float slideForce = 2f; // Adjust this value for sliding strength

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 contactPoint = contact.point;
            Vector2 contactNormal = contact.normal;

            // Check if the contact normal is at an angle that indicates a corner
            float angle = Vector2.Angle(contactNormal, Vector2.up);

            // Identify corner collisions based on angle range (adjust as necessary)
            if (angle > 45f && angle < 135f)
            {
                // Apply a force to push the character away from the corner and simulate sliding
                Vector2 slideDirection = new Vector2(contactNormal.x, -Mathf.Abs(contactNormal.y));
                rb.AddForce(slideDirection * slideForce, ForceMode2D.Impulse);
            }
        }
    }
}
