using System;
using UnityEngine;

public class KillZ : MonoBehaviour
{
    [SerializeField] private int DamageAmount = 1;
    [SerializeField] private float InputDisableTimer;
    
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
        CharacterSpawn spawn = player.GetComponent<CharacterSpawn>();
        spawn.Respawn(InputDisableTimer);
    }
}
