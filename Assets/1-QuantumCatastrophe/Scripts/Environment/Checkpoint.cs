using System;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Vector3 m_currentPosition => transform.position;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player)
        {
            SetCheckpointOnPlayer(player);
        }
    }

    private void SetCheckpointOnPlayer(PlayerController player)
    {
        CharacterHealth health = player.GetComponent<CharacterHealth>();
        health.SetResetPoint(m_currentPosition);
    }
}
