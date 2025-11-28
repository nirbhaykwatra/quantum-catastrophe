using UnityEngine;

public class HealthPickup : MonoBehaviour, IConsumable
{
    [SerializeField] private int healthAmount = 1;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>())
        {
            Consume(other.GetComponent<PlayerController>());
            Destroy(gameObject);
        }
    }

    public void Consume(PlayerController player)
    {
        CharacterHealth health = player.GetComponent<CharacterHealth>();
        
        health.Heal(healthAmount);
    }
}
