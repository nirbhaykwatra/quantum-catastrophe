using UnityEngine;

public class CharacterSpawn : MonoBehaviour
{
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private bool SpawnFromCheckpoint = true;
    private Vector3 m_spawnPosition;
    
    private CharacterHealth m_health;
    
    private void Awake()
    {
        m_health = GetComponent<CharacterHealth>();
    }

    private void Start()
    {
        if (SpawnFromCheckpoint)
        {
            m_spawnPosition = PlayerData.LastCheckpoint;
            transform.SetPositionAndRotation(m_spawnPosition, Quaternion.identity);
        }
        m_health.SetHealth(PlayerData.Health);
    }
    
    public void SetSpawnPoint(Vector3 position)
    {
        PlayerData.LastCheckpoint = position;
    }
}
