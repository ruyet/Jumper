using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricObstacle : MonoBehaviour
{
    private bool isDamaging = false;
    private HashSet<GameObject> knockedBackPlayers = new HashSet<GameObject>();

    public void EnableDamage()
    {
        isDamaging = true;
    }

    public void DisableDamage()
    {
        isDamaging = false;
        knockedBackPlayers.Clear(); // Clear all players from the knockback list when damage is disabled
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckAndApplyKnockback(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        CheckAndApplyKnockback(other);
    }

    private void CheckAndApplyKnockback(Collider2D other)
    {
        if (isDamaging && other.CompareTag("Player") && !knockedBackPlayers.Contains(other.gameObject))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                knockedBackPlayers.Add(other.gameObject); // Add player to the knocked back list
                player.StartCoroutine("HandleKnockback");
            }
        }
        else if (!isDamaging && knockedBackPlayers.Contains(other.gameObject))
        {
            knockedBackPlayers.Remove(other.gameObject); // Remove player from list if obstacle is not damaging
        }
    }
}
