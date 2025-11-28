using System;
using UnityEngine;

public class KillZ : MonoBehaviour
{
    [SerializeField] private int DamageAmount = 1;
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player)
        {
            CharacterHealth health = player.GetComponent<CharacterHealth>();
            health.Damage(DamageAmount);
            ResetPlayerToCheckpoint(player.gameObject);
        }
    }

    private void ResetPlayerToCheckpoint(GameObject player)
    {
        CharacterHealth health = player.GetComponent<CharacterHealth>();
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        player.transform.position = health.ResetPoint;
    }
}
